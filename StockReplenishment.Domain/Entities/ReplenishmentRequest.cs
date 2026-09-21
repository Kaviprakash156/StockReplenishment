using StockReplenishment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Domain.Entities
{
    public class ReplenishmentRequest
    {
        public int Id { get; set; }

        public int StockLocationId { get; set; }

        public RequestPriority Priority { get; set; }

        public RequestStatus Status { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public DateTime? FulfilledAt { get; set; }

        public string? RejectionReason { get; set; }

        public StockValidationStatus StockValidationStatus { get; set; }

        public string? StockValidationMessage { get; set; }

        public DateTime? StockValidationCompletedAt { get; set; }

        public StockLocation StockLocation { get; set; } = null!;

        public ICollection<ReplenishmentRequestItem> Items { get; set; }
            = new List<ReplenishmentRequestItem>();
    }
}
