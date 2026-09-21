using Microsoft.EntityFrameworkCore;
using StockReplenishment.Domain.Entities;
using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        StockReplenishmentDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.StockLocations.AnyAsync())
        {
            return;
        }

        var locations = new List<StockLocation>
        {
            new()
            {
                Code = "LINE-A-01",
                Name = "Production Line A - Station 01"
            },
            new()
            {
                Code = "LINE-A-02",
                Name = "Production Line A - Station 02"
            },
            new()
            {
                Code = "LINE-B-01",
                Name = "Production Line B - Station 01"
            },
            new()
            {
                Code = "WAREHOUSE-01",
                Name = "Main Warehouse"
            }
        };

        await context.StockLocations.AddRangeAsync(locations);
        await context.SaveChangesAsync();

        var requests = new List<ReplenishmentRequest>
        {
            new()
            {
                StockLocationId = locations[0].Id,
                Priority = RequestPriority.Urgent,
                Status = RequestStatus.Draft,
                CreatedBy = "worker01",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                StockValidationStatus = StockValidationStatus.NotStarted,
                Items =
                {
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-1001",
                        Description = "Steel Bolt",
                        RequestedQuantity = 50,
                        FulfilledQuantity = 0
                    },
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-1002",
                        Description = "Metal Washer",
                        RequestedQuantity = 50,
                        FulfilledQuantity = 0
                    }
                }
            },

            new()
            {
                StockLocationId = locations[1].Id,
                Priority = RequestPriority.Normal,
                Status = RequestStatus.Submitted,
                CreatedBy = "worker02",
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                SubmittedAt = DateTime.UtcNow.AddHours(-3),
                StockValidationStatus = StockValidationStatus.Available,
                StockValidationMessage = "All requested materials are available.",
                StockValidationCompletedAt = DateTime.UtcNow.AddHours(-2),
                Items =
                {
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-2001",
                        Description = "Bearing",
                        RequestedQuantity = 10,
                        FulfilledQuantity = 0
                    }
                }
            },

            new()
            {
                StockLocationId = locations[2].Id,
                Priority = RequestPriority.Urgent,
                Status = RequestStatus.Approved,
                CreatedBy = "worker03",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                SubmittedAt = DateTime.UtcNow.AddHours(-20),
                ApprovedAt = DateTime.UtcNow.AddHours(-18),
                StockValidationStatus = StockValidationStatus.Available,
                StockValidationMessage = "Requested stock is available.",
                StockValidationCompletedAt = DateTime.UtcNow.AddHours(-19),
                Items =
                {
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-3001",
                        Description = "Hydraulic Hose",
                        RequestedQuantity = 5,
                        FulfilledQuantity = 0
                    }
                }
            },

            new()
            {
                StockLocationId = locations[0].Id,
                Priority = RequestPriority.Low,
                Status = RequestStatus.Rejected,
                CreatedBy = "worker01",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SubmittedAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
                RejectedAt = DateTime.UtcNow.AddDays(-2).AddHours(3),
                RejectionReason = "Requested quantity exceeds the current replenishment limit.",
                StockValidationStatus = StockValidationStatus.Unavailable,
                StockValidationMessage = "Insufficient stock available.",
                StockValidationCompletedAt = DateTime.UtcNow.AddDays(-2).AddHours(2),
                Items =
                {
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-4001",
                        Description = "Motor Assembly",
                        RequestedQuantity = 2,
                        FulfilledQuantity = 0
                    }
                }
            },

            new()
            {
                StockLocationId = locations[3].Id,
                Priority = RequestPriority.Normal,
                Status = RequestStatus.Fulfilled,
                CreatedBy = "worker04",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                SubmittedAt = DateTime.UtcNow.AddDays(-3).AddHours(1),
                ApprovedAt = DateTime.UtcNow.AddDays(-3).AddHours(3),
                FulfilledAt = DateTime.UtcNow.AddDays(-2),
                StockValidationStatus = StockValidationStatus.Available,
                StockValidationMessage = "Requested stock is available.",
                StockValidationCompletedAt = DateTime.UtcNow.AddDays(-3).AddHours(2),
                Items =
                {
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-5001",
                        Description = "Safety Gloves",
                        RequestedQuantity = 100,
                        FulfilledQuantity = 100
                    },
                    new ReplenishmentRequestItem
                    {
                        ArticleNumber = "MAT-5002",
                        Description = "Safety Goggles",
                        RequestedQuantity = 25,
                        FulfilledQuantity = 25
                    }
                }
            }
        };

        await context.ReplenishmentRequests.AddRangeAsync(requests);
        await context.SaveChangesAsync();
    }
}