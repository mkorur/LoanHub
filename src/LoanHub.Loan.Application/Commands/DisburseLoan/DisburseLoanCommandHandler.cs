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
    private readonly ILoanUnitOfWork _unitOfWork;

    public DisburseLoanCommandHandler(ILoanWriteRepository writes, ILoanUnitOfWork unitOfWork)
    {
        _writes = writes;
        _unitOfWork = unitOfWork;
    }

    public async Task<DisburseLoanCommandResult> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        LoanAggregate? disbursed = null;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
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

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return new DisburseLoanCommandResult(
            LoanMapper.ToDto(disbursed!),
            "Kredi kullandırıldı; LoanCreated outbox'a yazıldı.");
    }
}
