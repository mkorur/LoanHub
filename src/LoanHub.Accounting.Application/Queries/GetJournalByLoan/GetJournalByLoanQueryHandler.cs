using LoanHub.Accounting.Application.Abstractions;
using MediatR;

namespace LoanHub.Accounting.Application.Queries.GetJournalByLoan;

public sealed class GetJournalByLoanQueryHandler
    : IRequestHandler<GetJournalByLoanQuery, IReadOnlyList<JournalEntryDto>>
{
    private readonly IAccountingReadRepository _reads;

    public GetJournalByLoanQueryHandler(IAccountingReadRepository reads) => _reads = reads;

    public Task<IReadOnlyList<JournalEntryDto>> Handle(
        GetJournalByLoanQuery request,
        CancellationToken cancellationToken) =>
        _reads.GetJournalByLoanAsync(request.LoanId, cancellationToken);
}
