using LoanHub.Accounting.Application.Queries.GetJournalByLoan;
using LoanHub.Accounting.Infrastructure;
using LoanHub.Accounting.Infrastructure.Persistence;
using MediatR;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAccountingInfrastructure(builder.Configuration);

var app = builder.Build();

await DatabaseMigrator.RunAsync(
    app.Services.GetRequiredService<NpgsqlDataSource>(),
    CancellationToken.None);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/journal/by-loan/{loanId:guid}", async (Guid loanId, IMediator mediator, CancellationToken ct) =>
{
    var rows = await mediator.Send(new GetJournalByLoanQuery(loanId), ct);
    return Results.Ok(rows.Select(j => new
    {
        id = j.Id,
        messageId = j.MessageId,
        loanId = j.LoanId,
        description = j.Description,
        debitAccount = j.DebitAccount,
        creditAccount = j.CreditAccount,
        amountCents = j.AmountCents,
        currency = j.Currency,
    }));
});

app.Run();
