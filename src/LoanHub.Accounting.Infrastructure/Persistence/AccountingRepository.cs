using LoanHub.Accounting.Application.Abstractions;
using LoanHub.Accounting.Domain.Entities;
using Npgsql;

namespace LoanHub.Accounting.Infrastructure.Persistence;

public sealed class AccountingRepository : IAccountingWriteRepository, IAccountingReadRepository
{
    private readonly NpgsqlDataSource _db;
    private readonly IAccountingDbSession _session;

    public AccountingRepository(NpgsqlDataSource db, IAccountingDbSession session)
    {
        _db = db;
        _session = session;
    }

    public async Task PostJournalWithInboxAsync(
        JournalEntry entry,
        string eventType,
        CancellationToken cancellationToken)
    {
        var (conn, tx) = _session.RequireActiveTransaction();

        await using (var exists = new NpgsqlCommand(
                         "SELECT EXISTS(SELECT 1 FROM inbox WHERE message_id = @id)", conn, tx))
        {
            exists.Parameters.AddWithValue("id", entry.MessageId);
            if ((bool)(await exists.ExecuteScalarAsync(cancellationToken) ?? false))
                return;
        }

        await using (var inbox = new NpgsqlCommand(
                         "INSERT INTO inbox (message_id, event_type) VALUES (@id, @type)", conn, tx))
        {
            inbox.Parameters.AddWithValue("id", entry.MessageId);
            inbox.Parameters.AddWithValue("type", eventType);
            await inbox.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var journal = new NpgsqlCommand(
                         """
                         INSERT INTO journal_entries (
                             id, message_id, loan_id, description,
                             debit_account, credit_account, amount_cents, currency
                         ) VALUES (@id, @messageId, @loanId, @desc, @debit, @credit, @amount, @currency)
                         """, conn, tx))
        {
            journal.Parameters.AddWithValue("id", entry.Id);
            journal.Parameters.AddWithValue("messageId", entry.MessageId);
            journal.Parameters.AddWithValue("loanId", entry.LoanId);
            journal.Parameters.AddWithValue("desc", entry.Description);
            journal.Parameters.AddWithValue("debit", entry.DebitAccount);
            journal.Parameters.AddWithValue("credit", entry.CreditAccount);
            journal.Parameters.AddWithValue("amount", entry.AmountCents);
            journal.Parameters.AddWithValue("currency", entry.Currency);
            await journal.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<JournalEntryDto>> GetJournalByLoanAsync(
        Guid loanId,
        CancellationToken cancellationToken)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT id, message_id, loan_id, description, debit_account, credit_account, amount_cents, currency
            FROM journal_entries WHERE loan_id = @loanId ORDER BY posted_at
            """, conn);
        cmd.Parameters.AddWithValue("loanId", loanId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var list = new List<JournalEntryDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new JournalEntryDto(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetInt64(6),
                reader.GetString(7)));
        }

        return list;
    }
}
