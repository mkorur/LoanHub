namespace LoanHub.Loan.Domain.Exceptions;

public sealed class LoanAlreadyDisbursedException : Exception
{
    public Guid LoanId { get; }

    public LoanAlreadyDisbursedException(Guid loanId)
        : base($"Loan {loanId} is already disbursed.")
    {
        LoanId = loanId;
    }
}
