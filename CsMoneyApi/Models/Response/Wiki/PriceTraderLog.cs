using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CsMoneyAPI.Models.Response.Wiki
{
    public class PriceTraderLog
    {
        public class ErrorResponse
        {
            public string Message { get; set; }
        }
        public class DataResponse
        {
            public class ItemPriceTraderLog
            {
                public class Value
                {
                    [JsonProperty(PropertyName = "price_trader_new")]
                    public double Price { get; set; }
                    public long Time { get; set; }
                }

                [JsonProperty(PropertyName = "name_id")]
                public string NameId { get; set; }

                public Value[] Values { get; set; }
            }

            [JsonProperty(PropertyName = "price_trader_log")]
            public ItemPriceTraderLog[] PriceTraderLogs { get; set; }
        }

        public DataResponse Data { get; set; }

        public ErrorResponse[] Errors { get; set; }
    }
}

