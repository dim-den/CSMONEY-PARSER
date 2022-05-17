using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsMoneyAPI.Models.Results
{
    public class BotInventoryItemsResponse
    {
        public Item[] Items { get; set; }
        public int? Error { get; set; }
        public string[] Details { get; set; }
    }
}
