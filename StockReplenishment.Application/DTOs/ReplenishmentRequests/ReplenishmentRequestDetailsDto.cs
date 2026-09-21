namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class ReplenishmentRequestDetailsDto
{
    public int Id { get; set; }

    public int StockLocationId { get; set; }

    public string StockLocationCode { get; set; } = string.Empty;

    public string StockLocationName { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public DateTime? FulfilledAt { get; set; }

    public string? RejectionReason { get; set; }

    public string StockValidationStatus { get; set; } = string.Empty;

    public string? StockValidationMessage { get; set; }

    public DateTime? StockValidationCompletedAt { get; set; }

    public List<ReplenishmentRequestItemDto> Items { get; set; } = [];
}