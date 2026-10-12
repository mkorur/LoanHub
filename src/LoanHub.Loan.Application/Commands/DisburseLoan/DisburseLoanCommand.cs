using MediatR;

namespace LoanHub.Loan.Application.Commands.DisburseLoan;

public sealed record DisburseLoanCommand(Guid LoanId) : IRequest<DisburseLoanCommandResult>;
