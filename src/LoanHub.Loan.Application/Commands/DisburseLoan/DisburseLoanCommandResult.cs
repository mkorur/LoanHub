using LoanHub.Loan.Application.Common;

namespace LoanHub.Loan.Application.Commands.DisburseLoan;

public sealed record DisburseLoanCommandResult(LoanDto Loan, string Message);
