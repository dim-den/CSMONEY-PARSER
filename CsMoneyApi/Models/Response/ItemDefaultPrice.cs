using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CsMoneyAPI.Models.Results
{
    public class ItemDefaultPrice
    {
        public class CSMResponse
        {
            public class BuyResponse
            {
                [JsonProperty(PropertyName = "-6")]
                public double Price { get; set; }
            }

            public BuyResponse Buy { get; set; }
           
            public Int64 Updated { get; set; }
        }

        public CSMResponse CSM { get; set; }
    }
}
