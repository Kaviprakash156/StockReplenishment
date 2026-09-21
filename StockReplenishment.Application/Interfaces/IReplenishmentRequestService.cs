using StockReplenishment.Application.DTOs.Common;
using StockReplenishment.Application.DTOs.ReplenishmentRequests;
using StockReplenishment.Application.DTOs.StockLocations;

namespace StockReplenishment.Application.Interfaces;

public interface IReplenishmentRequestService
{
    Task<ReplenishmentRequestDetailsDto> GetByIdAsync(int id);

    Task<PagedResultDto<ReplenishmentRequestListDto>> GetPagedAsync(
        int? status,
        int? priority,
        int? stockLocationId,
        int page,
        int pageSize);

    Task<ReplenishmentRequestDetailsDto> CreateAsync(
        CreateReplenishmentRequestDto request);

    Task<ReplenishmentRequestDetailsDto> UpdateAsync(
        int id,
        UpdateReplenishmentRequestDto request);

    Task<List<StockLocationDto>> GetStockLocationsAsync();

    Task SubmitAsync(int id);

    Task ApproveAsync(int id);

    Task RejectAsync(
        int id,
        RejectReplenishmentRequestDto request);

    Task FulfillAsync(
        int id,
        FulfillReplenishmentRequestDto request);
}