using LoanHub.Loan.Application.Outbox;
using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;

namespace LoanHub.Loan.Application.Abstractions;

public interface ILoanWriteRepository
{
    Task AddAsync(LoanAggregate loan, CancellationToken cancellationToken);

    Task DisburseAsync(
        Guid loanId,
        Func<LoanAggregate, OutboxMessage> createOutbox,
        CancellationToken cancellationToken);
}
