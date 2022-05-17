using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BuffApi.Models.Results;
using Newtonsoft.Json;

namespace BuffApi.Services
{
    public class ItemsService
    {
        private static readonly HttpClient _client;
        private static string _token;


        static ItemsService()
        {
            _client = new HttpClient(new HttpClientHandler { UseCookies = false });

            _client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,zh-TW;q=0.6,zh;q=0.5");
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
        }

        public static string Token
        {
            get => _token;
            set
            {
                _token = value;
                _client.DefaultRequestHeaders.Remove("Cookie");
                if(!String.IsNullOrEmpty(_token)) _client.DefaultRequestHeaders.Add("Cookie", $"session={_token}");
            }
        }

        public static async Task<PageItems> GetPageItems(int pageNum = 1, double minPrice = 0.0d, double maxPrice = 1000000.0d)
        {
            HttpResponseMessage response =
               String.IsNullOrEmpty(Token) ?
               await _client.GetAsync($"https://buff.163.com/api/market/goods?game=csgo&page_num=1") :
               await _client.GetAsync($"https://buff.163.com/api/market/goods?game=csgo&page_num=" + $"{pageNum}&min_price={minPrice}&max_price={maxPrice}");

            string jsonResponse = await response.Content.ReadAsStringAsync();

            PageItems itemsResponse = JsonConvert.DeserializeObject<PageItems>(jsonResponse);

            return itemsResponse;

        }

    }
}
