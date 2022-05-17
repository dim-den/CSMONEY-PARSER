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
    public class ItemsService
    {
        private static readonly int _offset = 60;

        public static async Task<BotInventoryItemsResponse> GetItemsByPage(int page = 0, int count = 60, float minPrice = 0.0f)
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(
                    $"https://inventories.cs.money/5.0/load_bots_inventory/730?isMarket=true&hasTradeLock=true&hasTradeLock=false&minPrice={minPrice}&offset={page * _offset}");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                BotInventoryItemsResponse itemsResponse = JsonConvert.DeserializeObject<BotInventoryItemsResponse>(jsonResponse);

                return itemsResponse;
            }
        }

    }
}
