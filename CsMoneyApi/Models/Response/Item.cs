using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsMoneyAPI.Models.Results
{
    public class Item
    {
        public int AppId { get; set; }
        public Int64 AssetId { get; set; }
        public float? Float { get; set; }
        public bool hasHighDemand { get; set; }
        public Int64 Id { get; set; }
        public bool IsMarket { get; set; }
        public Int64 NameId { get; set; }
        public float Overprice { get; set; }
        public float Price { get; set; }
        public string Quality { get; set; }
        public string Rarity { get; set; }
        public string SteamId { get; set; }
        public Int64 TradeLock { get; set; }
        public int Type { get; set; }
        public int? UserId { get; set; }
        public int? Pattern { get; set; }
        public string Rank { get; set; }
        public Overpay Overpay { get; set; }
        public string FullName { get; set; }
    }
}
