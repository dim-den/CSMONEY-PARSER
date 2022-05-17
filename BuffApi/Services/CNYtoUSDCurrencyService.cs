using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuffApi.Services
{
    public class CNYtoUSDCurrencyService
    {
        private static readonly HttpClient _client;


        public static async Task<float> GetUSDtoCNYCurrency()
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(
                    $"https://free.currconv.com/api/v7/convert?q=USD_CNY&compact=ultra&apiKey=730d03f660b1f3490414");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<Dictionary<string, float>>(jsonResponse);

                return result.Values.First();
            }

        }
    }
}
