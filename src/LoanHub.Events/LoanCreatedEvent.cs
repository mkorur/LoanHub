namespace LoanHub.Events;

public static class LoanEventTypes
{
    public const string LoanCreatedV1 = "LoanCreated.v1";
}

public sealed class LoanCreatedEvent
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = "";
    public Guid LoanId { get; init; }
    public string CustomerId { get; init; } = "";
    public long AmountCents { get; init; }
    public string Currency { get; init; } = "TRY";
    public DateTimeOffset OccurredAt { get; init; }

    public static LoanCreatedEvent Create(Guid loanId, string customerId, long amountCents, string currency) =>
        new()
        {
            EventId = Guid.NewGuid(),
            EventType = LoanEventTypes.LoanCreatedV1,
            LoanId = loanId,
            CustomerId = customerId,
            AmountCents = amountCents,
            Currency = currency,
            OccurredAt = DateTimeOffset.UtcNow,
        };
}
