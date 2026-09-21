namespace StockReplenishment.Application.Interfaces;

public interface IStockValidationQueue
{
    ValueTask QueueAsync(
        int replenishmentRequestId,
        CancellationToken cancellationToken = default);

    ValueTask<int> DequeueAsync(
        CancellationToken cancellationToken);
}