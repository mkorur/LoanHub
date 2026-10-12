using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Common;
using LoanAggregate = LoanHub.Loan.Domain.Entities.Loan;
using MediatR;

namespace LoanHub.Loan.Application.Commands.CreateLoan;

public sealed class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, LoanDto>
{
    private readonly ILoanWriteRepository _writes;

    public CreateLoanCommandHandler(ILoanWriteRepository writes) => _writes = writes;

    public async Task<LoanDto> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = LoanAggregate.CreateNew(request.CustomerId, request.AmountCents, request.Currency ?? "TRY");
        await _writes.AddAsync(loan, cancellationToken);
        return LoanMapper.ToDto(loan);
    }
}
