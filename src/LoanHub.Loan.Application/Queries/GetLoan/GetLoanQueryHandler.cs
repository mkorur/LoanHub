using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Application.Common;
using LoanHub.Loan.Domain.Exceptions;
using MediatR;

namespace LoanHub.Loan.Application.Queries.GetLoan;

public sealed class GetLoanQueryHandler : IRequestHandler<GetLoanQuery, LoanDto>
{
    private readonly ILoanReadRepository _reads;

    public GetLoanQueryHandler(ILoanReadRepository reads) => _reads = reads;

    public async Task<LoanDto> Handle(GetLoanQuery request, CancellationToken cancellationToken)
    {
        var loan = await _reads.GetByIdAsync(request.LoanId, cancellationToken)
                   ?? throw new LoanNotFoundException(request.LoanId);
        return LoanMapper.ToDto(loan);
    }
}
