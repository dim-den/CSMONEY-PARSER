using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsMoneyAPI.Models.Results;
using Newtonsoft.Json;

namespace CsMoneyAPI.Services.MrInki
{
    public class ItemDefaultPriceService
    {
        public static async Task<double> GetItemDefaultPrice(string itemName)
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync($"http://188.166.72.201:8080/singleitem?i={itemName}");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                ItemDefaultPrice itemDefaultPrice = JsonConvert.DeserializeObject<ItemDefaultPrice>(jsonResponse);

                return itemDefaultPrice.CSM.Buy.Price;
            }
        }
    }
}