using LoanHub.Loan.Domain.Enums;
using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;

namespace LoanHub.Loan.Application.Common;

internal static class LoanMapper
{
    public static LoanDto ToDto(LoanAggregate loan) =>
        new(
            loan.Id,
            loan.CustomerId,
            loan.AmountCents,
            loan.Currency,
            loan.Status == LoanStatus.Disbursed ? "disbursed" : "pending",
            loan.DisbursedAt);
}
