using Confluent.Kafka;
using LoanHub.Loan.Application;
using LoanHub.Loan.Application.Abstractions;
using LoanHub.Loan.Infrastructure.Outbox;
using LoanHub.Loan.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LoanHub.Loan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLoanInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=loan_db;Username=loanhub;Password=loanhub";

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddScoped<LoanUnitOfWork>();
        services.AddScoped<ILoanUnitOfWork>(sp => sp.GetRequiredService<LoanUnitOfWork>());
        services.AddScoped<ILoanDbSession>(sp => sp.GetRequiredService<LoanUnitOfWork>());

        services.AddScoped<LoanRepository>();
        services.AddScoped<ILoanWriteRepository>(sp => sp.GetRequiredService<LoanRepository>());
        services.AddScoped<ILoanReadRepository>(sp => sp.GetRequiredService<LoanRepository>());
        services.AddScoped<IOutboxRelay, KafkaOutboxRelay>();
        services.AddSingleton<IProducer<string, string>>(_ =>
        {
            var bootstrap = configuration["Kafka:BootstrapServers"] ?? "localhost:19092";
            return new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrap,
                Acks = Acks.All,
            }).Build();
        });

        services.AddLoanApplication();
        services.AddHostedService<OutboxRelayBackgroundService>();

        return services;
    }
}
