using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using BuffApi.Models.Results;
using BuffApi.Services;
using CsMoneyAPI.Models.Results;
using CsMoneyAPI.Services;

namespace BuffToCsMoneyParser
{
    class Program
    {
        private static float usd_to_cny = 6.34f;
        private static int page = 1;

        static async Task Main(string[] args)
        {
            TimerCallback currentUpdateCallback = new TimerCallback(UpdateUSDtoCNYCurrency);
            TimerCallback parsePageCallback = new TimerCallback(ParsePage);


            Timer currencyUpdateTimer = new Timer(currentUpdateCallback, null, 0, 10 * 60 * 1000);
            Timer parsePageTimer = new Timer(parsePageCallback, null, 0, 5000);

            Console.Read();
        }

        static async void ParsePage(object obj)
        {
            try
            {
                PageItems result = await BuffApi.Services.ItemsService.GetPageItems(1, 5.0f, 3000.0f);

                foreach (var item in result.Data.Items)
                {
                    double csmPrice = (await ItemDefaultPriceService.GetItemDefaultPrice(item.Name)).CSM.Buy.Price;
                    double csmPriceWithFees = Math.Floor(csmPrice * 96) / 100;
                    double buffPriceUSD = Math.Floor((item.SellMinPrice / usd_to_cny) * 100) / 100;
                    double mult = Math.Floor((csmPriceWithFees / buffPriceUSD) * 100) / 100;


                    if (mult > 1.8)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        ItemOverstockStatus overstockStatus = await OverstockStatusService.GetItemOverstockStatus(item.Name);
                        Console.WriteLine($"{item.Name}: buff {buffPriceUSD}$, cs.money {csmPrice}$ ({mult}x, &fees {csmPriceWithFees}$, {overstockStatus.Status.ToString().ToLower()}: {overstockStatus.OverstockDiff})");

                        if (buffPriceUSD > 5.0f) SystemSounds.Beep.Play();
                    }
                    else if (buffPriceUSD >= 10.0f)
                        Console.WriteLine($"{item.Name}: buff {buffPriceUSD}$, cs.money {csmPrice}$ ({mult}x, &fees {csmPriceWithFees}$)");

                    Console.ResetColor();
                }

                Console.WriteLine($"{page++}----------------------------------------------------------------");
            }
            catch (Exception exc)
            {
                Console.WriteLine($"EROR: {exc.Message}");
            }
        }

        static async void UpdateUSDtoCNYCurrency(object obj)
        {
            float result = await BuffApi.Services.CNYtoUSDCurrencyService.GetUSDtoCNYCurrency();

            usd_to_cny = result != 0 ? result : usd_to_cny;

            Console.WriteLine(usd_to_cny);
        }
    }
}
