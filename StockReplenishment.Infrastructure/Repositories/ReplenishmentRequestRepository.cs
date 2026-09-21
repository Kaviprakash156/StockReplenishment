using Microsoft.EntityFrameworkCore;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Domain.Entities;
using StockReplenishment.Infrastructure.Data;

namespace StockReplenishment.Infrastructure.Repositories;

public class ReplenishmentRequestRepository
    : IReplenishmentRequestRepository
{
    private readonly StockReplenishmentDbContext _dbContext;

    public ReplenishmentRequestRepository(
        StockReplenishmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReplenishmentRequest?> GetByIdAsync(int id)
    {
        return await _dbContext.ReplenishmentRequests
            .Include(x => x.StockLocation)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        
    }

    public async Task<(IReadOnlyList<ReplenishmentRequest> Items, int TotalCount)> GetPagedAsync(
        int? status,
        int? priority,
        int? stockLocationId,
        int page,
        int pageSize)
    {
        var query = _dbContext.ReplenishmentRequests
            .AsNoTracking()
            .Include(x => x.StockLocation)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x =>
                (int)x.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(x =>
                (int)x.Priority == priority.Value);
        }

        if (stockLocationId.HasValue)
        {
            query = query.Where(x =>
                x.StockLocationId == stockLocationId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<StockLocation?> GetStockLocationByIdAsync(int id)
    {
        return await _dbContext.StockLocations
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(ReplenishmentRequest request)
    {
        await _dbContext.ReplenishmentRequests.AddAsync(request);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<StockLocation>> GetStockLocationsAsync()
    {
        return await _dbContext.StockLocations
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync();
    }
}