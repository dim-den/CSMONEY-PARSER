using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuffApi.Models.Results
{
    public class Item
    {
        public class GoodsInfoResponse
        {
            [JsonProperty(PropertyName = "steam_price")]
            public double SteamPrice { get; set; }

            [JsonProperty(PropertyName = "steam_price_cny")]
            public double SteamPriceCny { get; set; }
        }

        [JsonProperty(PropertyName = "buy_max_price")]
        public double BuyMaxPrice { get; set; }

        [JsonProperty(PropertyName = "goods_info")]
        public GoodsInfoResponse GoodsInfo { get; set; }

        public int Id { get; set; }

        [JsonProperty(PropertyName = "market_hash_name")]
        public string MarketHashName { get; set; }

        public string Name { get; set; }

        [JsonProperty(PropertyName = "quick_price")]
        public double QuickPrice { get; set; }

        [JsonProperty(PropertyName = "sell_min_price")]
        public double SellMinPrice { get; set; }

        [JsonProperty(PropertyName = "sell_num")]
        public int SellNum { get; set; }

        [JsonProperty(PropertyName = "sell_reference_price")]
        public double SellReferencePrice { get; set; }
    }
}
