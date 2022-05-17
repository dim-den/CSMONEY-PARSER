using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BuffToCsMoneyTelegramBot.Exceptions;
using CsMoneyAPI.Models.Response.Wiki;
using CsMoneyAPI.Models.Results;
using Newtonsoft.Json;

namespace CsMoneyAPI.Services.WikiCsMoney
{
    public class ItemDefaultPriceService
    {
        private const string _filename = "CSMItemNameToNameId.json";
        private static readonly HttpClient _client;
        private static readonly Dictionary<string, long> _itemNameToNameId = new Dictionary<string, long>();
        static ItemDefaultPriceService()
        {
            _client = new HttpClient();
            if (File.Exists(_filename))
                _itemNameToNameId = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(_filename));
        }

        public static async Task<double> GetItemDefaultPrice(string itemName)
        {
            try
            {
                long nameId = await GetItemNameId(itemName);

                string query = @"query price_trader_log($name_ids: [Int!]!) {   price_trader_log(input: {name_ids: $name_ids}) {     name_id    values {      price_trader_new      time    }  }}";
                string operationName = "price_trader_log";
                string variables = "{\"name_ids\":[" + nameId + @"]}";

                string uri = $"https://wiki.cs.money/graphql?query={query}&operationName={operationName}&variables={variables}";

                HttpResponseMessage response = await _client.GetAsync(uri);

                string jsonResponse = await response.Content.ReadAsStringAsync();

                PriceTraderLog priceTraderLog = JsonConvert.DeserializeObject<PriceTraderLog>(jsonResponse);

                return priceTraderLog.Data.PriceTraderLogs[0].Values.Last().Price;
            }
            catch (Exception)
            {
                Console.WriteLine($"ERROR GETTING PRICE FOR: {itemName}");
                return 0;
            }
        }

        private static async Task<long> GetItemNameId(string itemName)
        {
            if (_itemNameToNameId.TryGetValue(itemName, out long value))
            {
                return value;
            }
            else
            {
                HttpResponseMessage response = await _client.GetAsync($"https://inventories.cs.money/5.0/load_bots_inventory/730?sort=price&order=asc&hasTradeLock=false&hasTradeLock=true&limit=1&name={itemName}&offset=0");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                BotInventoryItemsResponse itemsResponse = JsonConvert.DeserializeObject<BotInventoryItemsResponse>(jsonResponse);

                if (itemsResponse != null && itemsResponse.Items != null && itemsResponse.Items[0] != null && itemsResponse.Items[0].FullName == itemName)
                {
                    _itemNameToNameId.Add(itemsResponse.Items[0].FullName, itemsResponse.Items[0].NameId);
                    File.WriteAllText(_filename, JsonConvert.SerializeObject(_itemNameToNameId));
                    Console.WriteLine($"ADDED ITEM TO DICTIONARY: {itemName}");


                    return itemsResponse.Items[0].NameId;
                }

                throw new ItemNotFoundException(itemName);
            }

        }
    }
}
