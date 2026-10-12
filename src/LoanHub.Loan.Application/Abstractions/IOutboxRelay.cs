namespace LoanHub.Loan.Application.Abstractions;

/// <summary>
/// Outbox'tan bir kaydı Kafka'ya iletir (claim + publish + mark — altyapıda tek transaction).
/// </summary>
public interface IOutboxRelay
{
    Task<bool> RelayNextAsync(CancellationToken cancellationToken);
}
