using System.ComponentModel.DataAnnotations;

namespace StockReplenishment.Application.DTOs.ReplenishmentRequests;

public class RejectReplenishmentRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}