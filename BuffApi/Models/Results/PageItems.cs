using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuffApi.Models.Results
{
    public class PageItems
    {
        public class DataResponse
        {
            public Item[] Items { get; set; }

            [JsonProperty(PropertyName = "page_num")]
            public int PageNum { get; set; }   

            [JsonProperty(PropertyName = "page_size")]
            public int PageSize { get; set; }

            [JsonProperty(PropertyName = "total_count")]
            public int TotalCount { get; set; }

            [JsonProperty(PropertyName = "total_page")]
            public int TotalPage { get; set; }
        }

        public string Code { get; set; }
        public DataResponse Data { get; set; }
        public string Msg { get; set; }
  
    }
}
