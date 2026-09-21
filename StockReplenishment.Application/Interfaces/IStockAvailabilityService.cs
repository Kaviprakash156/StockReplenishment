using StockReplenishment.Application.DTOs.StockValidation;

namespace StockReplenishment.Application.Interfaces;

public interface IStockAvailabilityService
{
    Task<StockAvailabilityResult> CheckAvailabilityAsync(
        int replenishmentRequestId,
        CancellationToken cancellationToken = default);
}