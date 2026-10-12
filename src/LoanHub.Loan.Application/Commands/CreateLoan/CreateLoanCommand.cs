using LoanHub.Loan.Application.Common;
using MediatR;

namespace LoanHub.Loan.Application.Commands.CreateLoan;

public sealed record CreateLoanCommand(string CustomerId, long AmountCents, string? Currency)
    : IRequest<LoanDto>;
