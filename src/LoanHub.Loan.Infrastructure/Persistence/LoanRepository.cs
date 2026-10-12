using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Outbox;
using LoanHub.Loan.Domain.Enums;
using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;
using LoanHub.Loan.Domain.Exceptions;
using Npgsql;

namespace LoanHub.Loan.Infrastructure.Persistence;

public sealed class LoanRepository : ILoanWriteRepository, ILoanReadRepository
{
    private readonly NpgsqlDataSource _db;

    public LoanRepository(NpgsqlDataSource db) => _db = db;

    public async Task AddAsync(LoanAggregate loan, CancellationToken cancellationToken)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO loans (id, customer_id, amount_cents, currency, status)
            VALUES (@id, @customerId, @amount, @currency, 'pending')
            """, conn);
        cmd.Parameters.AddWithValue("id", loan.Id);
        cmd.Parameters.AddWithValue("customerId", loan.CustomerId);
        cmd.Parameters.AddWithValue("amount", loan.AmountCents);
        cmd.Parameters.AddWithValue("currency", loan.Currency);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<LoanAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT id, customer_id, amount_cents, currency, status, disbursed_at
            FROM loans WHERE id = @id
            """, conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadLoan(reader) : null;
    }

    public async Task DisburseAsync(
        Guid loanId,
        Func<LoanAggregate, OutboxMessage> createOutbox,
        CancellationToken cancellationToken)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        LoanAggregate loan;
        await using (var select = new NpgsqlCommand(
                         """
                         SELECT id, customer_id, amount_cents, currency, status, disbursed_at
                         FROM loans WHERE id = @id FOR UPDATE
                         """, conn, tx))
        {
            select.Parameters.AddWithValue("id", loanId);
            await using var reader = await select.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new LoanNotFoundException(loanId);
            loan = ReadLoan(reader);
        }

        var outbox = createOutbox(loan);

        await using (var update = new NpgsqlCommand(
                         """
                         UPDATE loans SET status = 'disbursed', disbursed_at = @at, updated_at = NOW()
                         WHERE id = @id
                         """, conn, tx))
        {
            update.Parameters.AddWithValue("id", loan.Id);
            update.Parameters.AddWithValue("at", loan.DisbursedAt ?? DateTimeOffset.UtcNow);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var outboxCmd = new NpgsqlCommand(
                         """
                         INSERT INTO outbox_events (id, aggregate_type, aggregate_id, event_type, payload)
                         VALUES (@id, @aggregateType, @aggregateId, @eventType, @payload::jsonb)
                         """, conn, tx))
        {
            outboxCmd.Parameters.AddWithValue("id", outbox.Id);
            outboxCmd.Parameters.AddWithValue("aggregateType", outbox.AggregateType);
            outboxCmd.Parameters.AddWithValue("aggregateId", outbox.AggregateId);
            outboxCmd.Parameters.AddWithValue("eventType", outbox.EventType);
            outboxCmd.Parameters.AddWithValue("payload", outbox.PayloadJson);
            await outboxCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    private static LoanAggregate ReadLoan(NpgsqlDataReader reader)
    {
        var statusText = reader.GetString(4);
        var status = statusText == "disbursed" ? LoanStatus.Disbursed : LoanStatus.Pending;
        return LoanAggregate.Restore(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetInt64(2),
            reader.GetString(3),
            status,
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5));
    }
}
