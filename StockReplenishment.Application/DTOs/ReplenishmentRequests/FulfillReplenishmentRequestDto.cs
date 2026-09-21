using System.ComponentModel.DataAnnotations;

namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class FulfillReplenishmentRequestDto
{
    [Required]
    [MinLength(1)]
    public List<FulfillReplenishmentRequestItemDto> Items { get; set; } = [];
}

public class FulfillReplenishmentRequestItemDto
{
    [Required]
    public int ItemId { get; set; }

    [Range(0, int.MaxValue)]
    public int FulfilledQuantity { get; set; }
}