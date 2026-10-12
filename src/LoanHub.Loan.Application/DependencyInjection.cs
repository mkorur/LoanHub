using LoanHub.Loan.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace LoanHub.Loan.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLoanApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<OutboxDispatchHandler>();
        return services;
    }
}
