using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using CsMoneyAPI.Models.Results;
using CsMoneyAPI.Services;

namespace CsMoneyOverstockNotifier
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            ObservableCollection<string> itemNamesForSearch = new()
            {
                "Nova | Predator (Factory New)"
            };

            while (true)
            {
                foreach (var itemName in itemNamesForSearch)
                {
                    try
                    {
                        ItemOverstockStatus overtockStatus = await OverstockStatusService.GetItemOverstockStatus(itemName);

                        if (overtockStatus.Status == Status.Tradable)
                        {
                            SystemSounds.Beep.Play();
                            Console.ForegroundColor = ConsoleColor.Green;
                        }

                        Console.WriteLine($"{itemName} : {overtockStatus.Status.ToString().ToLower()} ({overtockStatus.OverstockDiff})");
                        Console.ResetColor();
                    }
                    catch(Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }

                }

                Console.WriteLine("\n--------------------------------------------------------------\n");
                Thread.Sleep(1500);                
            }            
        }

    }
}
