using LoanHub.Loan.Domain.Enums;
using LoanHub.Loan.Domain.Exceptions;

namespace LoanHub.Loan.Domain.Entities;

/// <summary>
/// Kredi aggregate kökü — kullandırım kuralları burada.
/// </summary>
public sealed class Loan
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = "";
    public long AmountCents { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public LoanStatus Status { get; private set; }
    public DateTimeOffset? DisbursedAt { get; private set; }

    private Loan() { }

    public static Loan CreateNew(string customerId, long amountCents, string currency)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));
        if (amountCents <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountCents));

        return new Loan
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Trim(),
            AmountCents = amountCents,
            Currency = string.IsNullOrWhiteSpace(currency) ? "TRY" : currency,
            Status = LoanStatus.Pending,
        };
    }

    public static Loan Restore(
        Guid id,
        string customerId,
        long amountCents,
        string currency,
        LoanStatus status,
        DateTimeOffset? disbursedAt) =>
        new()
        {
            Id = id,
            CustomerId = customerId,
            AmountCents = amountCents,
            Currency = currency,
            Status = status,
            DisbursedAt = disbursedAt,
        };

    public void Disburse()
    {
        if (Status == LoanStatus.Disbursed)
            throw new LoanAlreadyDisbursedException(Id);

        Status = LoanStatus.Disbursed;
        DisbursedAt = DateTimeOffset.UtcNow;
    }
}
