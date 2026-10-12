namespace LoanHub.Accounting.Application.Abstractions;

public interface IAccountingReadRepository
{
    Task<IReadOnlyList<JournalEntryDto>> GetJournalByLoanAsync(Guid loanId, CancellationToken cancellationToken);
}

public sealed record JournalEntryDto(
    Guid Id,
    Guid MessageId,
    Guid LoanId,
    string Description,
    string DebitAccount,
    string CreditAccount,
    long AmountCents,
    string Currency);
