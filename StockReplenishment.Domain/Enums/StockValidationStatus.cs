using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Domain.Enums
{
    public enum StockValidationStatus
    {
        NotStarted = 1,
        Pending = 2,
        Available = 3,
        Unavailable = 4,
        Failed = 5
    }
}
