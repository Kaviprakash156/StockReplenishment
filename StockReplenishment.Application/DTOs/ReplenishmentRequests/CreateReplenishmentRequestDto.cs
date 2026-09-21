using System.ComponentModel.DataAnnotations;

namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class CreateReplenishmentRequestDto
{
    [Required]
    public int StockLocationId { get; set; }

    [Required]
    public int Priority { get; set; }

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateReplenishmentRequestItemDto> Items { get; set; } = [];
}

public class CreateReplenishmentRequestItemDto
{
    [Required]
    public string ArticleNumber { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int RequestedQuantity { get; set; }
}