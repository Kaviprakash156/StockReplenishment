using System.ComponentModel.DataAnnotations;

namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class UpdateReplenishmentRequestDto
{
    [Required]
    public int StockLocationId { get; set; }

    [Required]
    public int Priority { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateReplenishmentRequestItemDto> Items { get; set; } = [];
}