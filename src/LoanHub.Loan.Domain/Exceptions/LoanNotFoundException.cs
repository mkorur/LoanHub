namespace LoanHub.Loan.Domain.Exceptions;

public sealed class LoanNotFoundException : Exception
{
    public Guid LoanId { get; }

    public LoanNotFoundException(Guid loanId)
        : base($"Loan {loanId} was not found.")
    {
        LoanId = loanId;
    }
}
