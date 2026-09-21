using StockReplenishment.Application.DTOs.Common;
using StockReplenishment.Application.DTOs.ReplenishmentRequests;
using StockReplenishment.Application.DTOs.StockLocations;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Domain.Entities;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Application.Services;

public class ReplenishmentRequestService : IReplenishmentRequestService
{
    private readonly IReplenishmentRequestRepository _repository;
    private readonly IStockValidationQueue _stockValidationQueue;

    public ReplenishmentRequestService(
        IReplenishmentRequestRepository repository,
        IStockValidationQueue stockValidationQueue)
    {
        _repository = repository;
        _stockValidationQueue = stockValidationQueue;
    }

    public async Task<ReplenishmentRequestDetailsDto> GetByIdAsync(int id)
    {
        var request = await _repository.GetByIdAsync(id);

        if (request is null)
        {
            throw new KeyNotFoundException(
                $"Replenishment request with ID {id} was not found.");
        }

        return MapToDetailsDto(request);
    }

    public async Task<PagedResultDto<ReplenishmentRequestListDto>> GetPagedAsync(
        int? status,
        int? priority,
        int? stockLocationId,
        int page,
        int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException(
                "Page must be greater than zero.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "Page size must be between 1 and 100.");
        }

        ValidateEnum<RequestStatus>(status, "status");
        ValidateEnum<RequestPriority>(priority, "priority");

        var result = await _repository.GetPagedAsync(
            status,
            priority,
            stockLocationId,
            page,
            pageSize);

        return new PagedResultDto<ReplenishmentRequestListDto>
        {
            Items = result.Items
                .Select(MapToListDto)
                .ToList(),

            Page = page,

            PageSize = pageSize,

            TotalCount = result.TotalCount
        };
    }

    public async Task<ReplenishmentRequestDetailsDto> CreateAsync(
        CreateReplenishmentRequestDto request)
    {
        ValidatePriority(request.Priority);

        ValidateItems(request.Items);

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            throw new ArgumentException(
                "CreatedBy is required.");
        }

        var location =
            await _repository.GetStockLocationByIdAsync(
                request.StockLocationId);

        if (location is null)
        {
            throw new ArgumentException(
                $"Stock location with ID {request.StockLocationId} does not exist.");
        }

        var entity = new ReplenishmentRequest
        {
            StockLocationId = request.StockLocationId,

            Priority = (RequestPriority)request.Priority,

            Status = RequestStatus.Draft,

            CreatedBy = request.CreatedBy.Trim(),

            CreatedAt = DateTime.UtcNow,

            StockValidationStatus =
                StockValidationStatus.NotStarted
        };

        foreach (var item in request.Items)
        {
            entity.Items.Add(new ReplenishmentRequestItem
            {
                ArticleNumber = item.ArticleNumber.Trim(),

                Description = item.Description.Trim(),

                RequestedQuantity = item.RequestedQuantity,

                FulfilledQuantity = 0
            });
        }

        await _repository.AddAsync(entity);

        await _repository.SaveChangesAsync();

        // The repository has the location relationship available,
        // so use the supplied location for the response.
        entity.StockLocation = location;

        return MapToDetailsDto(entity);
    }

    public async Task<ReplenishmentRequestDetailsDto> UpdateAsync(
        int id,
        UpdateReplenishmentRequestDto request)
    {
        var entity = await GetEntityAsync(id);

        EnsureStatus(
            entity,
            RequestStatus.Draft,
            "Only draft requests can be updated.");

        ValidatePriority(request.Priority);

        ValidateItems(request.Items);

        var location =
            await _repository.GetStockLocationByIdAsync(
                request.StockLocationId);

        if (location is null)
        {
            throw new ArgumentException(
                $"Stock location with ID {request.StockLocationId} does not exist.");
        }

        entity.StockLocationId = request.StockLocationId;

        entity.StockLocation = location;

        entity.Priority =
            (RequestPriority)request.Priority;

        entity.Items.Clear();

        foreach (var item in request.Items)
        {
            entity.Items.Add(new ReplenishmentRequestItem
            {
                ArticleNumber = item.ArticleNumber.Trim(),

                Description = item.Description.Trim(),

                RequestedQuantity = item.RequestedQuantity,

                FulfilledQuantity = 0
            });
        }

        await _repository.SaveChangesAsync();

        return MapToDetailsDto(entity);
    }

    public async Task SubmitAsync(int id)
    {
        var entity = await GetEntityAsync(id);

        EnsureStatus(
            entity,
            RequestStatus.Draft,
            "Only draft requests can be submitted.");

        if (entity.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A request must contain at least one item.");
        }

        entity.Status = RequestStatus.Submitted;

        entity.SubmittedAt = DateTime.UtcNow;

        entity.StockValidationStatus =
            StockValidationStatus.Pending;

        entity.StockValidationMessage =
            "Stock validation is in progress.";

        entity.StockValidationCompletedAt = null;

        await _repository.SaveChangesAsync();

        await _stockValidationQueue.QueueAsync(entity.Id);
    }

    public async Task ApproveAsync(int id)
    {
        var entity = await GetEntityAsync(id);

        EnsureStatus(
            entity,
            RequestStatus.Submitted,
            "Only submitted requests can be approved.");

        if (entity.StockValidationStatus ==
            StockValidationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Stock validation is still in progress.");
        }

        if (entity.StockValidationStatus ==
            StockValidationStatus.Unavailable)
        {
            throw new InvalidOperationException(
                "The request cannot be approved because stock is unavailable.");
        }

        if (entity.StockValidationStatus ==
            StockValidationStatus.Failed)
        {
            throw new InvalidOperationException(
                "The request cannot be approved because stock validation failed.");
        }

        entity.Status = RequestStatus.Approved;

        entity.ApprovedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();
    }

    public async Task RejectAsync(
        int id,
        RejectReplenishmentRequestDto request)
    {
        var entity = await GetEntityAsync(id);

        EnsureStatus(
            entity,
            RequestStatus.Submitted,
            "Only submitted requests can be rejected.");

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ArgumentException(
                "A rejection reason is required.");
        }

        entity.Status = RequestStatus.Rejected;

        entity.RejectedAt = DateTime.UtcNow;

        entity.RejectionReason =
            request.Reason.Trim();

        await _repository.SaveChangesAsync();
    }

    public async Task FulfillAsync(
        int id,
        FulfillReplenishmentRequestDto request)
    {
        var entity = await GetEntityAsync(id);

        EnsureStatus(
            entity,
            RequestStatus.Approved,
            "Only approved requests can be fulfilled.");

        if (request.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one fulfillment item is required.");
        }

        foreach (var fulfillment in request.Items)
        {
            var item = entity.Items
                .FirstOrDefault(x =>
                    x.Id == fulfillment.ItemId);

            if (item is null)
            {
                throw new ArgumentException(
                    $"Item with ID {fulfillment.ItemId} does not belong to request {id}.");
            }

            if (fulfillment.FulfilledQuantity < 0)
            {
                throw new ArgumentException(
                    "Fulfilled quantity cannot be negative.");
            }

            if (fulfillment.FulfilledQuantity >
                item.RequestedQuantity)
            {
                throw new ArgumentException(
                    $"Fulfilled quantity for item {item.Id} cannot exceed requested quantity.");
            }

            item.FulfilledQuantity =
                fulfillment.FulfilledQuantity;
        }

        if (entity.Items.Any(x =>
                x.FulfilledQuantity <
                x.RequestedQuantity))
        {
            throw new InvalidOperationException(
                "All requested quantities must be fulfilled before completing the request.");
        }

        entity.Status = RequestStatus.Fulfilled;

        entity.FulfilledAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();
    }

    private async Task<ReplenishmentRequest> GetEntityAsync(
        int id)
    {
        var entity =
            await _repository.GetByIdAsync(id);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Replenishment request with ID {id} was not found.");
        }

        return entity;
    }

    private static void EnsureStatus(
        ReplenishmentRequest entity,
        RequestStatus expectedStatus,
        string message)
    {
        if (entity.Status != expectedStatus)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void ValidateItems(
        List<CreateReplenishmentRequestItemDto> items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ArgumentException(
                "At least one material item is required.");
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(
                    item.ArticleNumber))
            {
                throw new ArgumentException(
                    "Article number is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.Description))
            {
                throw new ArgumentException(
                    "Item description is required.");
            }

            if (item.RequestedQuantity <= 0)
            {
                throw new ArgumentException(
                    "Requested quantity must be greater than zero.");
            }
        }
    }

    private static void ValidatePriority(
        int priority)
    {
        if (!Enum.IsDefined(
                typeof(RequestPriority),
                priority))
        {
            throw new ArgumentException(
                "Invalid request priority.");
        }
    }

    private static void ValidateEnum<T>(
        int? value,
        string fieldName)
        where T : struct, Enum
    {
        if (value.HasValue &&
            !Enum.IsDefined(typeof(T), value.Value))
        {
            throw new ArgumentException(
                $"Invalid {fieldName}.");
        }
    }

    private static ReplenishmentRequestListDto MapToListDto(
        ReplenishmentRequest entity)
    {
        return new ReplenishmentRequestListDto
        {
            Id = entity.Id,

            StockLocationCode =
                entity.StockLocation.Code,

            StockLocationName =
                entity.StockLocation.Name,

            Priority =
                entity.Priority.ToString(),

            Status =
                entity.Status.ToString(),

            CreatedBy =
                entity.CreatedBy,

            CreatedAt =
                entity.CreatedAt,

            StockValidationStatus =
                entity.StockValidationStatus.ToString()
        };
    }

    private static ReplenishmentRequestDetailsDto
        MapToDetailsDto(
            ReplenishmentRequest entity)
    {
        return new ReplenishmentRequestDetailsDto
        {
            Id = entity.Id,

            StockLocationId =
                entity.StockLocationId,

            StockLocationCode =
                entity.StockLocation.Code,

            StockLocationName =
                entity.StockLocation.Name,

            Priority =
                entity.Priority.ToString(),

            Status =
                entity.Status.ToString(),

            CreatedBy =
                entity.CreatedBy,

            CreatedAt =
                entity.CreatedAt,

            SubmittedAt =
                entity.SubmittedAt,

            ApprovedAt =
                entity.ApprovedAt,

            RejectedAt =
                entity.RejectedAt,

            FulfilledAt =
                entity.FulfilledAt,

            RejectionReason =
                entity.RejectionReason,

            StockValidationStatus =
                entity.StockValidationStatus.ToString(),

            StockValidationMessage =
                entity.StockValidationMessage,

            StockValidationCompletedAt =
                entity.StockValidationCompletedAt,

            Items = entity.Items
                .Select(x =>
                    new ReplenishmentRequestItemDto
                    {
                        Id = x.Id,

                        ArticleNumber =
                            x.ArticleNumber,

                        Description =
                            x.Description,

                        RequestedQuantity =
                            x.RequestedQuantity,

                        FulfilledQuantity =
                            x.FulfilledQuantity
                    })
                .ToList()
        };
    }
    public async Task<List<StockLocationDto>> GetStockLocationsAsync()
    {
        var locations =
            await _repository.GetStockLocationsAsync();

        return locations
            .Select(x => new StockLocationDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            })
            .ToList();
    }
}