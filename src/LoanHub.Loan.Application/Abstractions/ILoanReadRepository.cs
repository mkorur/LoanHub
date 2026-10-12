using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;

namespace LoanHub.Loan.Application.Abstractions;

public interface ILoanReadRepository
{
    Task<LoanAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
