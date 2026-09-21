using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.Domain.Enums
{
    public enum RequestStatus
    {
        Draft = 1,
        Submitted = 2,
        Approved = 3,
        Rejected = 4,
        Fulfilled = 5
    }
}
