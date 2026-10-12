using Confluent.Kafka;
using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Outbox;
using LoanHub.Loan.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LoanHub.Loan.Infrastructure.Outbox;

public sealed class KafkaOutboxRelay : IOutboxRelay
{
    private readonly ILoanDbSession _session;
    private readonly ILoanUnitOfWork _unitOfWork;
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaOutboxRelay(
        ILoanDbSession session,
        ILoanUnitOfWork unitOfWork,
        IProducer<string, string> producer,
        IConfiguration configuration)
    {
        _session = session;
        _unitOfWork = unitOfWork;
        _producer = producer;
        _topic = configuration["Kafka:Topic"] ?? "loan.events";
    }

    public async Task<bool> RelayNextAsync(CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var (conn, tx) = _session.RequireActiveTransaction();

            OutboxMessage? message;
            await using (var select = new NpgsqlCommand(
                             """
                             SELECT id, aggregate_type, aggregate_id, event_type, payload::text
                             FROM outbox_events
                             WHERE published_at IS NULL
                             ORDER BY created_at
                             LIMIT 1
                             FOR UPDATE SKIP LOCKED
                             """, conn, tx))
            {
                await using var reader = await select.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return false;
                }

                message = new OutboxMessage(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetGuid(2),
                    reader.GetString(3),
                    reader.GetString(4));
            }

            await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = message.Id.ToString(),
                Value = message.PayloadJson,
                Headers = new Headers
                {
                    { "message-id", System.Text.Encoding.UTF8.GetBytes(message.Id.ToString()) },
                    { "content-type", "application/json"u8.ToArray() },
                },
            }, cancellationToken);

            await using (var update = new NpgsqlCommand(
                             "UPDATE outbox_events SET published_at = NOW() WHERE id = @id", conn, tx))
            {
                update.Parameters.AddWithValue("id", message.Id);
                await update.ExecuteNonQueryAsync(cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
