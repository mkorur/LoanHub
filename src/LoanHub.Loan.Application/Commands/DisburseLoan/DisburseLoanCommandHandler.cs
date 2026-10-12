using System.Text.Json;
using LoanHub.Events;
using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Common;
using LoanHub.Loan.Application.Outbox;
using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;
using MediatR;

namespace LoanHub.Loan.Application.Commands.DisburseLoan;

public sealed class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, DisburseLoanCommandResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILoanWriteRepository _writes;

    public DisburseLoanCommandHandler(ILoanWriteRepository writes) => _writes = writes;

    public async Task<DisburseLoanCommandResult> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        LoanAggregate? disbursed = null;

        await _writes.DisburseAsync(request.LoanId, loan =>
        {
            loan.Disburse();
            disbursed = loan;

            var integrationEvent = LoanCreatedEvent.Create(
                loan.Id, loan.CustomerId, loan.AmountCents, loan.Currency);

            return new OutboxMessage(
                integrationEvent.EventId,
                "loan",
                loan.Id,
                integrationEvent.EventType,
                JsonSerializer.Serialize(integrationEvent, JsonOptions));
        }, cancellationToken);

        return new DisburseLoanCommandResult(
            LoanMapper.ToDto(disbursed!),
            "Kredi kullandırıldı; LoanCreated outbox'a yazıldı.");
    }
}
