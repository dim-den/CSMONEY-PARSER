using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsMoneyAPI.Models.Results;
using Newtonsoft.Json;

namespace CsMoneyAPI.Services
{
    public class OverstockStatusService
    {
        public static async Task<Item[]> GetAllOvertockItems()
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync("https://cs.money/list_overstock?appId=730");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var items = JsonConvert.DeserializeObject<Item[]>(jsonResponse);

                return items;
            }
        }

        public static async Task<ItemOverstockStatus> GetItemOverstockStatus(string itemFullName)
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync("https://cs.money/check_skin_status?appId=730&name=" + itemFullName);

                string jsonResponse = await response.Content.ReadAsStringAsync();

                ItemOverstockStatus overstockStatus = JsonConvert.DeserializeObject<ItemOverstockStatus>(jsonResponse);
                if (overstockStatus.Error is not null) 
                    overstockStatus.Status = Status.NotFound;

                return overstockStatus;
            }
        }
    }
}
