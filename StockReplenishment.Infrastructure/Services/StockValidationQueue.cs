using System.Threading.Channels;
using StockReplenishment.Application.Interfaces;

namespace StockReplenishment.Infrastructure.Services;

public class StockValidationQueue : IStockValidationQueue
{
    private readonly Channel<int> _queue;

    public StockValidationQueue()
    {
        _queue = Channel.CreateUnbounded<int>();
    }

    public async ValueTask QueueAsync(
        int replenishmentRequestId,
        CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(
            replenishmentRequestId,
            cancellationToken);
    }

    public async ValueTask<int> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}