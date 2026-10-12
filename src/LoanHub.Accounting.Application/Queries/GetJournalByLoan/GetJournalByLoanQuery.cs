using LoanHub.Accounting.Application.Abstractions;
using MediatR;

namespace LoanHub.Accounting.Application.Queries.GetJournalByLoan;

public sealed record GetJournalByLoanQuery(Guid LoanId) : IRequest<IReadOnlyList<JournalEntryDto>>;
