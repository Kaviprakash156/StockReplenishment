using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Domain.Entities
{
    public class StockLocation
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ICollection<ReplenishmentRequest> ReplenishmentRequests { get; set; }
            = new List<ReplenishmentRequest>();
    }
}
