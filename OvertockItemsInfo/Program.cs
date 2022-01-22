using System;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using CsMoneyAPI.Models.Results;
using CsMoneyAPI.Services;
using Newtonsoft.Json;

namespace OvertockItemsInfo
{
    class Program
    {
        public static float ProfitCalculator(Item item)
        {
            return (float)(item.Price * ((96.0 / (100.0 + item.Overprice)) - 1));
        }

        static async Task Main(string[] args)
        {
            ThreadPool.SetMaxThreads(32, 32);
            Console.OutputEncoding = Encoding.UTF8;

            int pages = 128;

            while (true)
            {
                try
                {
                    for (int i = 0; i < pages; i++)
                    {
                        if (i % 50 == 0)
                            Console.WriteLine(i);

                        BotInventoryItemsResponse result = await ItemsService.GetItemsByPage(page: i, minPrice: 3.0f);

                        if (result.Error == null)
                        {
                            var overpriceItems = from item in result.Items
                                                 where item.Overprice < -4
                                                 group item by item.NameId into g
                                                 select new
                                                 {
                                                     Item = (from p in g where ProfitCalculator(p) == g.Max(f => ProfitCalculator(f)) select p).FirstOrDefault(),
                                                     Count = g.Count()
                                                 };

                            foreach (var el in overpriceItems)
                            {
                                var item = el.Item;

                                ItemOverstockStatus overstockStatus = await OverstockStatusService.GetItemOverstockStatus(item.FullName);

                                if (item.Price < 10 && item.Overprice > -6)
                                    continue;

                                if (item.Overprice <= -8)
                                    Console.ForegroundColor = ConsoleColor.Yellow;

                                float profit = (float)Math.Round(ProfitCalculator(item), 2);

                                if (profit >= 1 && overstockStatus.OverstockDiff >= -3)
                                    Console.ForegroundColor = ConsoleColor.Blue;

                                if (profit >= 0.5 && profit < 1 && overstockStatus.OverstockDiff >= -1)
                                    Console.ForegroundColor = ConsoleColor.Cyan;

                                if (overstockStatus.Status == Status.Tradable)
                                {
                                    if(profit > 1) SystemSounds.Beep.Play();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }

                                Console.WriteLine($"({el.Count}) {item.FullName} : {item.Price} (profit: {profit}, overprice: {item.Overprice}, {overstockStatus.Status.ToString().ToLower()}: {overstockStatus.OverstockDiff})");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            break;
                        }             
                    }
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            Console.Read();
        }
    }
}
