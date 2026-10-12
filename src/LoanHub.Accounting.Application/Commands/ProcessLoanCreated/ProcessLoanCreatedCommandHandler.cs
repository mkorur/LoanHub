using LoanHub.Accounting.Application.Abstractions;
using LoanHub.Accounting.Domain.Entities;
using LoanHub.Events;
using MediatR;

namespace LoanHub.Accounting.Application.Commands.ProcessLoanCreated;

public sealed class ProcessLoanCreatedCommandHandler : IRequestHandler<ProcessLoanCreatedCommand>
{
    private readonly IAccountingWriteRepository _writes;
    private readonly IAccountingUnitOfWork _unitOfWork;

    public ProcessLoanCreatedCommandHandler(
        IAccountingWriteRepository writes,
        IAccountingUnitOfWork unitOfWork)
    {
        _writes = writes;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessLoanCreatedCommand request, CancellationToken cancellationToken)
    {
        var integrationEvent = request.IntegrationEvent;
        if (integrationEvent.EventType != LoanEventTypes.LoanCreatedV1)
            return;

        var entry = JournalEntry.CreateForLoanDisbursement(
            integrationEvent.EventId,
            integrationEvent.LoanId,
            integrationEvent.CustomerId,
            integrationEvent.AmountCents,
            integrationEvent.Currency);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _writes.PostJournalWithInboxAsync(entry, integrationEvent.EventType, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
