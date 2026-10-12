using LoanHub.Loan.Application.Common;
using MediatR;

namespace LoanHub.Loan.Application.Queries.GetLoan;

public sealed record GetLoanQuery(Guid LoanId) : IRequest<LoanDto>;
