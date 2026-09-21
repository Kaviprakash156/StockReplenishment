using StockReplenishment.Domain.Enums;

namespace StockReplenishment.Application.DTOs.StockValidation;

public class StockAvailabilityResult
{
    public StockValidationStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;
}