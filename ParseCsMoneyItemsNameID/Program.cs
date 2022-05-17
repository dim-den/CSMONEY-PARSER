using System;
using System.Collections.Generic;
using System.IO;
using CsMoneyAPI.Models.Results;
using CsMoneyAPI.Services;
using Newtonsoft.Json;

namespace ParseCsMoneyItemsNameID
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            try
            {
                const string filename = "CSMItemNameToNameId.json";
                var itemNameToNameId = new Dictionary<string, long>();

                itemNameToNameId = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(filename));
                int count = 0;

                Console.WriteLine(itemNameToNameId.Count);
                for (int i = 0; ; i++)
                {
                    if (i % 10 == 0)
                        Console.WriteLine(i);

                    BotInventoryItemsResponse result = await ItemsService.GetItemsByPage(page: i);

                    if (result.Error == null)
                    {
                        foreach (var el in result.Items)
                        {
                            if (!itemNameToNameId.ContainsKey(el.FullName))
                            {
                                itemNameToNameId.Add(el.FullName, el.NameId);
                                count++;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Save to file (total pages:{i}, total items: {itemNameToNameId.Count}, items added: {count})");
                        File.WriteAllText(filename, JsonConvert.SerializeObject(itemNameToNameId));
                        break;
                    }

                }

            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }
    }
}
