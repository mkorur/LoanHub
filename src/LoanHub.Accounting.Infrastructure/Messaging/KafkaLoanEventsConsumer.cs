using System.Text.Json;
using Confluent.Kafka;
using LoanHub.Accounting.Application.Commands.ProcessLoanCreated;
using MediatR;
using LoanHub.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanHub.Accounting.Infrastructure.Messaging;

public sealed class KafkaLoanEventsConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaLoanEventsConsumer> _logger;
    private readonly ConsumerConfig _config;
    private readonly string _topic;

    public KafkaLoanEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaLoanEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _topic = configuration["Kafka:Topic"] ?? "loan.events";
        _config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:19092",
            GroupId = configuration["Kafka:GroupId"] ?? "accounting-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(_config).Build();
        consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka consumer subscribed to {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var evt = JsonSerializer.Deserialize<LoanCreatedEvent>(result.Message.Value, JsonOptions);
                    if (evt is null)
                    {
                        _logger.LogWarning("Skipped empty payload at offset {Offset}", result.Offset);
                        consumer.Commit(result);
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new ProcessLoanCreatedCommand(evt), stoppingToken);
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Integration event handling failed");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
