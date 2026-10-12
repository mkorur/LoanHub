namespace LoanHub.Loan.Application.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    string AggregateType,
    Guid AggregateId,
    string EventType,
    string PayloadJson);
