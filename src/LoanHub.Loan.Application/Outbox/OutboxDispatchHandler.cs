using LoanHub.Loan.Application.Abstractions;

namespace LoanHub.Loan.Application.Outbox;

public sealed class OutboxDispatchHandler
{
    private readonly IOutboxRelay _relay;

    public OutboxDispatchHandler(IOutboxRelay relay) => _relay = relay;

    public async Task DispatchBatchAsync(int maxItems, CancellationToken cancellationToken)
    {
        for (var i = 0; i < maxItems; i++)
        {
            if (!await _relay.RelayNextAsync(cancellationToken))
                break;
        }
    }
}
