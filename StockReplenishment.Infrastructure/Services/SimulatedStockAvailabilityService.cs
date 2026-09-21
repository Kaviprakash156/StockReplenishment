using StockReplenishment.Application.DTOs.StockValidation;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Infrastructure.Services;

public class SimulatedStockAvailabilityService
    : IStockAvailabilityService
{
    private readonly Random _random = new();

    public async Task<StockAvailabilityResult> CheckAvailabilityAsync(
        int replenishmentRequestId,
        CancellationToken cancellationToken = default)
    {
        var delayMilliseconds = _random.Next(3000, 7001);

        await Task.Delay(
            delayMilliseconds,
            cancellationToken);

        var available = _random.Next(0, 100) < 70;

        if (available)
        {
            return new StockAvailabilityResult
            {
                Status = StockValidationStatus.Available,
                Message = "Stock is available for all requested items."
            };
        }

        return new StockAvailabilityResult
        {
            Status = StockValidationStatus.Unavailable,
            Message = "One or more requested items are currently unavailable."
        };
    }
}