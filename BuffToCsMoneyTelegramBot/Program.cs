using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using BuffApi.Models.Results;
using CsMoneyAPI.Services;
using System.Media;
using CsMoneyAPI.Models.Results;
using OverstockTelegramBot;
using System.Linq;
using System.ComponentModel;
using Item = BuffApi.Models.Results.Item;
using System.Collections.Generic;
using CsMoneyAPI.Services.MrInki;

namespace BuffToCsMoneyTelegramBot
{
    public static class Program
    {
        private static double USD_TO_CNY = 6.34f;
        private static int page = 1;
        private static int totalPage = 1000;

        private static TelegramBotClient? _bot;
        private static Settings _settings;
        private static IgnoreList _ignoreList;
        private static Timer _currencyUpdateTimer;
        private static Timer _parsePageTimer;
        private static Timer _checkExpiredItemsTimer;

        private static List<Tuple<Item, DateTime>> _profitableItemsCache = new List<Tuple<Item, DateTime>>();
        private static bool _parsePageStopped = false;

        public static Timer ParsePageTimer { get => _parsePageTimer; set => _parsePageTimer = value; }
        public static bool ParsePageStopped { get => _parsePageStopped; set => _parsePageStopped = value; }

        public static async Task Main()
        {
            _settings = Settings.GetInstance();
            _ignoreList = IgnoreList.GetInstance();

            _settings.PropertyChanged += TimeoutLoading_PropertyChanged;
            _settings.PropertyChanged += BuffSessionToken_PropertyChanged;

            _bot = new TelegramBotClient(_settings.BotToken);

            if (!String.IsNullOrEmpty(_settings.BuffSessionToken))
                BuffApi.Services.ItemsService.Token = _settings.BuffSessionToken;

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            using var cts = new CancellationTokenSource();
            _bot.StartReceiving(Handlers.HandleUpdateAsync,
                         Handlers.HandleErrorAsync,
                         receiverOptions,
                         cts.Token);


            TimerCallback currentUpdateCallback = new TimerCallback(UpdateUSDtoCNYCurrency);
            TimerCallback parsePageCallback = new TimerCallback(ParsePage);
            TimerCallback checkExpiredItems = new TimerCallback(CheckExpiredItems);

            _currencyUpdateTimer = new Timer(currentUpdateCallback, null, 0, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
            _parsePageTimer = new Timer(parsePageCallback, null, 0, _settings.TimeoutLoadingMS);
            _checkExpiredItemsTimer = new Timer(checkExpiredItems, null, 0, (int)TimeSpan.FromSeconds(5).TotalMilliseconds);

            Console.ReadLine();
        }

        static async void ParsePage(object obj)
        {
            try
            {
                PageItems result = _settings.ParseMode switch
                {
                    BotParseMode.AllPages => await BuffApi.Services.ItemsService.
                        GetPageItems(page, Math.Round(USD_TO_CNY * _settings.MinBuffPriceUSD, 0), Math.Round(USD_TO_CNY * _settings.MaxBuffPriceUSD, 0)),

                    BotParseMode.NewItems => await BuffApi.Services.ItemsService.
                        GetPageItems(1, Math.Round(USD_TO_CNY * _settings.MinBuffPriceUSD, 0), Math.Round(USD_TO_CNY * _settings.MaxBuffPriceUSD, 0))
                };

                if (result.Data.TotalPage != totalPage)
                    totalPage = result.Data.TotalPage;

                foreach (Item item in result.Data.Items)
                {
                    double csmPrice = _settings.DepositPriceService switch
                    {
                        DepositPriceService.MrInki => await CsMoneyAPI.Services.MrInki.ItemDefaultPriceService.GetItemDefaultPrice(item.Name),
                        DepositPriceService.WikiCsMoney => await CsMoneyAPI.Services.WikiCsMoney.ItemDefaultPriceService.GetItemDefaultPrice(item.Name)
                    };

                    if (_ignoreList.Contains(item.Name))
                        continue;

                    double csmPriceWithFees = Math.Round(csmPrice * ((100.0d - _settings.Fees) / 100.0d), 2);
                    double buffPriceUSD = Math.Round(item.SellMinPrice / USD_TO_CNY, 2);
                    int mult = (int)((csmPriceWithFees / buffPriceUSD - 1) * 100);

                    if (mult >= _settings.MinProfitPercent)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{item.Name} (buff: {buffPriceUSD}$, csm: {csmPriceWithFees}$, profit {mult}%)");
                        Console.ResetColor();
                    }

                    if (mult < _settings.MinProfitPercent || buffPriceUSD < _settings.MinBuffPriceUSD || buffPriceUSD > _settings.MaxBuffPriceUSD)
                        continue;

                    ItemOverstockStatus overstockStatus = await OverstockStatusService.GetItemOverstockStatus(item.Name);

                    if (overstockStatus.OverstockDiff < _settings.MinOverstockLimit)
                        continue;

                    if (_profitableItemsCache.Find(tuple => tuple.Item1.Id == item.Id && tuple.Item1.SellReferencePrice == item.SellReferencePrice) != null) // if item not in cache
                        continue;

                    _profitableItemsCache.Add(new Tuple<Item, DateTime>(item, DateTime.Now));

                    await _bot.SendTextMessageAsync(chatId: _settings.ChatID, parseMode: ParseMode.Html,
                        text: ($"<b>{item.Name}</b>\n" +
                        $"<a href=\"https://buff.163.com/goods/" + $"{item.Id}\">buff price:</a> <b>{buffPriceUSD}$</b> ({item.SellMinPrice}¥)\n" +
                        $"<a href=\"https://cs.money/csgo/trade/?sort=price&order=asc&search=" + $"{item.Name}\">cs.money price:</a> <b>{csmPriceWithFees}$</b> (without fees {csmPrice}$, profit: <b>{mult}%</b>)\n" +
                        $"overstock status: <b>{overstockStatus.Status.ToString().ToLower()} ({overstockStatus.OverstockDiff}) </b>")
                        );
                }

                Console.WriteLine($"{page}----------------------------------------------------------------");

                page = page >= totalPage ? 1 : page + 1;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"EROR: {exc.Message}");
                Thread.Sleep(3 * 1000);
            }
        }

        static async void UpdateUSDtoCNYCurrency(object obj)
        {
            try
            {
                float result = await BuffApi.Services.CNYtoUSDCurrencyService.GetUSDtoCNYCurrency();

                USD_TO_CNY = result != 0 ? result : USD_TO_CNY;

                Console.WriteLine(USD_TO_CNY);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"ERROR UPDATING CURRENCY: {exc.Message}");
                Thread.Sleep(30 * 1000);
            }
        }

        static void CheckExpiredItems(object obj)
        {
            foreach (var tuple in _profitableItemsCache.ToList())
            {
                DateTime addedTime = tuple.Item2;

                if (DateTime.Now - addedTime >= TimeSpan.FromSeconds(_settings.ItemCachedTimeS))
                {
                    _profitableItemsCache.Remove(tuple);
                }
            }
        }

        private static void TimeoutLoading_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.TimeoutLoadingMS))
            {
                if (!ParsePageStopped)
                    ParsePageTimer.Change(0, _settings.TimeoutLoadingMS);
            }
        }

        private static void BuffSessionToken_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.BuffSessionToken))
            {
                BuffApi.Services.ItemsService.Token = _settings.BuffSessionToken;
            }
        }
    }
}
