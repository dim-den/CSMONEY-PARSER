using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsMoneyAPI.Models.Results
{
    public enum Status
    {
        NotFound = -1,
        Unavailable = 0,
        Overstock = 1,
        Unknown = 2,
        Tradable = 3
    }
    public class ItemOverstockStatus
    {
        public bool HasHighDemand { get; set; }
        public bool Demand { get; set; }
        public Status Status { get; set; }
        public int Limit { get; set; }
        public int OverstockDiff { get; set; }
        public int? Error { get; set; }
    }
}
