using LoanHub.Loan.Application.Commands.CreateLoan;
using LoanHub.Loan.Application.Commands.DisburseLoan;
using LoanHub.Loan.Application.Queries.GetLoan;
using LoanHub.Loan.Domain.Exceptions;
using LoanHub.Loan.Infrastructure;
using LoanHub.Loan.Infrastructure.Persistence;
using MediatR;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLoanInfrastructure(builder.Configuration);

var app = builder.Build();

await DatabaseMigrator.RunAsync(
    app.Services.GetRequiredService<NpgsqlDataSource>(),
    CancellationToken.None);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/loans", async (CreateLoanCommand req, IMediator mediator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.CustomerId) || req.AmountCents <= 0)
        return Results.BadRequest(new { error = "customerId and positive amountCents required" });

    try
    {
        var loan = await mediator.Send(req, ct);
        return Results.Created($"/api/loans/{loan.Id}", loan);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/loans/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        return Results.Ok(await mediator.Send(new GetLoanQuery(id), ct));
    }
    catch (LoanNotFoundException)
    {
        return Results.NotFound(new { error = "loan not found" });
    }
});

app.MapPost("/api/loans/{id:guid}/disburse", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var result = await mediator.Send(new DisburseLoanCommand(id), ct);
        return Results.Ok(new { loan = result.Loan, message = result.Message });
    }
    catch (LoanNotFoundException)
    {
        return Results.NotFound(new { error = "loan not found" });
    }
    catch (LoanAlreadyDisbursedException)
    {
        return Results.Conflict(new { error = "loan already disbursed" });
    }
});

app.Run();
