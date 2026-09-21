namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class ReplenishmentRequestListDto
{
    public int Id { get; set; }

    public string StockLocationCode { get; set; } = string.Empty;

    public string StockLocationName { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string StockValidationStatus { get; set; } = string.Empty;
}