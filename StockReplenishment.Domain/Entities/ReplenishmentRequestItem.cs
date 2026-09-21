using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Domain.Entities
{
    public class ReplenishmentRequestItem
    {
        public int Id { get; set; }

        public int ReplenishmentRequestId { get; set; }

        public string ArticleNumber { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int RequestedQuantity { get; set; }

        public int FulfilledQuantity { get; set; }

        public ReplenishmentRequest ReplenishmentRequest { get; set; } = null!;
    }
}
