using StockReplenishment.Domain.Entities;

namespace StockReplenishment.Application.Interfaces;

public interface IReplenishmentRequestRepository
{
    Task<ReplenishmentRequest?> GetByIdAsync(int id);

    Task<(IReadOnlyList<ReplenishmentRequest> Items, int TotalCount)> GetPagedAsync(
        int? status,
        int? priority,
        int? stockLocationId,
        int page,
        int pageSize);

    Task<StockLocation?> GetStockLocationByIdAsync(int id);

    Task<List<StockLocation>> GetStockLocationsAsync();

    Task AddAsync(ReplenishmentRequest request);

    Task SaveChangesAsync();

}