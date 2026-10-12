namespace LoanHub.Accounting.Domain.Entities;

public sealed class JournalEntry
{
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid LoanId { get; private set; }
    public string Description { get; private set; } = "";
    public string DebitAccount { get; private set; } = "";
    public string CreditAccount { get; private set; } = "";
    public long AmountCents { get; private set; }
    public string Currency { get; private set; } = "TRY";

    private JournalEntry() { }

    public static JournalEntry CreateForLoanDisbursement(
        Guid messageId,
        Guid loanId,
        string customerId,
        long amountCents,
        string currency)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("MessageId is required.", nameof(messageId));
        if (loanId == Guid.Empty)
            throw new ArgumentException("LoanId is required.", nameof(loanId));

        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            LoanId = loanId,
            Description = $"Kredi kullandırımı — müşteri {customerId}, kredi {loanId}",
            DebitAccount = "120.01",
            CreditAccount = "300.01",
            AmountCents = amountCents,
            Currency = string.IsNullOrWhiteSpace(currency) ? "TRY" : currency,
        };
    }
}
