using LoanHub.Accounting.Application;
using LoanHub.Accounting.Application.Abstractions;
using LoanHub.Accounting.Infrastructure.Messaging;
using LoanHub.Accounting.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LoanHub.Accounting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=accounting_db;Username=loanhub;Password=loanhub";

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddScoped<AccountingUnitOfWork>();
        services.AddScoped<IAccountingUnitOfWork>(sp => sp.GetRequiredService<AccountingUnitOfWork>());
        services.AddScoped<IAccountingDbSession>(sp => sp.GetRequiredService<AccountingUnitOfWork>());

        services.AddScoped<AccountingRepository>();
        services.AddScoped<IAccountingWriteRepository>(sp => sp.GetRequiredService<AccountingRepository>());
        services.AddScoped<IAccountingReadRepository>(sp => sp.GetRequiredService<AccountingRepository>());
        services.AddAccountingApplication();
        services.AddHostedService<KafkaLoanEventsConsumer>();

        return services;
    }
}
