using Confluent.Kafka;
using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Outbox;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LoanHub.Loan.Infrastructure.Outbox;

public sealed class KafkaOutboxRelay : IOutboxRelay
{
    private readonly NpgsqlDataSource _db;
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaOutboxRelay(
        NpgsqlDataSource db,
        IProducer<string, string> producer,
        IConfiguration configuration)
    {
        _db = db;
        _producer = producer;
        _topic = configuration["Kafka:Topic"] ?? "loan.events";
    }

    public async Task<bool> RelayNextAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

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
                return false;

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

        await tx.CommitAsync(cancellationToken);
        return true;
    }
}
