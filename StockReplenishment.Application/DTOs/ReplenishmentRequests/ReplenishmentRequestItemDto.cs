namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class ReplenishmentRequestItemDto
{
    public int Id { get; set; }

    public string ArticleNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int RequestedQuantity { get; set; }

    public int FulfilledQuantity { get; set; }
}