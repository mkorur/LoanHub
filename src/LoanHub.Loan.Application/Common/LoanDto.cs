namespace LoanHub.Loan.Application.Common;

public sealed record LoanDto(
    Guid Id,
    string CustomerId,
    long AmountCents,
    string Currency,
    string Status,
    DateTimeOffset? DisbursedAt);
