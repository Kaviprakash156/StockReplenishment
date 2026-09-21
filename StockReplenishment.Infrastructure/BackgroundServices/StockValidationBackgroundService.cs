using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Infrastructure.BackgroundServices;

public class StockValidationBackgroundService : BackgroundService
{
    private readonly IStockValidationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public StockValidationBackgroundService(
        IStockValidationQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var requestId =
                await _queue.DequeueAsync(stoppingToken);

            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var stockAvailabilityService =
                    scope.ServiceProvider
                        .GetRequiredService<IStockAvailabilityService>();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IReplenishmentRequestRepository>();

                var request =
                    await repository.GetByIdAsync(requestId);

                if (request is null)
                {
                    continue;
                }

                var result =
                    await stockAvailabilityService
                        .CheckAvailabilityAsync(
                            requestId,
                            stoppingToken);

                request.StockValidationStatus =
                    result.Status;

                request.StockValidationMessage =
                    result.Message;

                request.StockValidationCompletedAt =
                    DateTime.UtcNow;

                await repository.SaveChangesAsync();
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var repository =
                        scope.ServiceProvider
                            .GetRequiredService<
                                IReplenishmentRequestRepository>();

                    var request =
                        await repository.GetByIdAsync(requestId);

                    if (request is not null)
                    {
                        request.StockValidationStatus =
                            StockValidationStatus.Failed;

                        request.StockValidationMessage =
                            "Stock validation failed.";

                        request.StockValidationCompletedAt =
                            DateTime.UtcNow;

                        await repository.SaveChangesAsync();
                    }
                }
                catch
                {
                    // Keep the background worker alive
                    // if updating the failed request also fails.
                }
            }
        }
    }
}