using LoanHub.Accounting.Domain.Entities;

namespace LoanHub.Accounting.Application.Abstractions;

public interface IAccountingWriteRepository
{
    Task PostJournalWithInboxAsync(
        JournalEntry entry,
        string eventType,
        CancellationToken cancellationToken);
}
