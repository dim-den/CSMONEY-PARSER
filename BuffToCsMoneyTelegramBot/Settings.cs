using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuffToCsMoneyTelegramBot;
using Newtonsoft.Json;

namespace OverstockTelegramBot
{
    public enum BotParseMode
    {
        AllPages,
        NewItems
    }
    public enum DepositPriceService
    {
        MrInki,
        WikiCsMoney
    }

    public enum SettingType
    {
        Fees,
        MinProfit,
        ParseMode,
        MinPrice,
        MaxPrice,
        OverstockLimit,
        SessionToken,
        TimeoutLoading,
        ItemCachedTimeS,
        DepositPriceService
    }

    static class SettingTypeExtensions
    {
        public static string ToFriendlyString(this SettingType me)
        {
            return me switch
            {
                SettingType.Fees => "Fees",
                SettingType.MinProfit => "Min profit",
                SettingType.ParseMode => "Parse mode",
                SettingType.MinPrice => "Min price",
                SettingType.MaxPrice => "Max price",
                SettingType.OverstockLimit => "Ovestock limit",
                SettingType.SessionToken => "Session token",
                SettingType.TimeoutLoading => "Timeout loading",
                SettingType.ItemCachedTimeS => "Item cached time",
                SettingType.DepositPriceService => "Deposit price service",
                _ => throw new NotImplementedException()
            };
        }
    }

    public class Settings : AppSettings<Settings>
    {
        private static Settings _settings;
        private static object _syncRoot = new Object();

        private int _timeoutLoadingMS = 4000;
        private double _minBuffPriceUSD = 5.0d;
        private double _maxBuffPriceUSD = 2000.0d;
        private double _fees = 4.0d;
        private string _buffSessionToken;
        private int _minProfitPercent = 85;
        private int _minOverstockLimit = -3;
        private int _itemCachedTimeS = 300;
        private BotParseMode _parseMode = BotParseMode.NewItems;
        private DepositPriceService _depositPriceService = DepositPriceService.MrInki;
        private readonly string botToken = "5171772885:AAEzymbP7KY96vroEREyxGEXRdpG1YAGxdM";
        private readonly int chatID = 445356482;

        public static Settings GetInstance()
        {
            if (_settings == null)
            {
                lock (_syncRoot)
                {
                    if (_settings == null)
                        _settings = Load();
                }
            }
            return _settings;
        }

        public string BotToken => botToken;
        public int ChatID => chatID;
        public string BuffSessionToken
        {
            get => _buffSessionToken;
            set
            {
                _buffSessionToken = value;
                OnPropertyChanged(nameof(BuffSessionToken));
            }
        }
        public double Fees
        {
            get => _fees;
            set
            {
                _fees = value;
                OnPropertyChanged(nameof(Fees));
            }
        }
        public double MinBuffPriceUSD
        {
            get => _minBuffPriceUSD; set
            {
                _minBuffPriceUSD = value;
                OnPropertyChanged(nameof(MinBuffPriceUSD));
            }
        }
        public double MaxBuffPriceUSD
        {
            get => _maxBuffPriceUSD;
            set
            {
                _maxBuffPriceUSD = value;
                OnPropertyChanged(nameof(MaxBuffPriceUSD));
            }
        }
        public int TimeoutLoadingMS
        {
            get => _timeoutLoadingMS;
            set
            {
                _timeoutLoadingMS = value;
                OnPropertyChanged(nameof(TimeoutLoadingMS));
            }
        }

        public int MinProfitPercent
        {
            get => _minProfitPercent;
            set
            {
                _minProfitPercent = value;
                OnPropertyChanged(nameof(MinProfitPercent));
            }
        }
        public int MinOverstockLimit
        {
            get => _minOverstockLimit;
            set
            {
                _minOverstockLimit = value;
                OnPropertyChanged(nameof(MinOverstockLimit));
            }
        }
        public BotParseMode ParseMode
        {
            get => _parseMode;
            set
            {
                _parseMode = value;
                OnPropertyChanged(nameof(ParseMode));
            }
        }

        public DepositPriceService DepositPriceService
        {
            get => _depositPriceService;
            set
            {
                _depositPriceService = value;
                OnPropertyChanged(nameof(DepositPriceService));
            }
        }

        public int ItemCachedTimeS
        {
            get => _itemCachedTimeS;
            set
            {
                _itemCachedTimeS = value;
                OnPropertyChanged(nameof(ItemCachedTimeS));
            }
        }

        public string GetStringValueBySettingType(SettingType settingType)
        {
            return settingType switch
            {
                SettingType.Fees => $"{_settings.Fees}%",
                SettingType.MinProfit => $"{_settings.MinProfitPercent}%",
                SettingType.ParseMode => _settings.ParseMode.ToString(),
                SettingType.MinPrice => $"{_settings.MinBuffPriceUSD}$",
                SettingType.MaxPrice => $"{_settings.MaxBuffPriceUSD}$",
                SettingType.OverstockLimit => _settings.MinOverstockLimit.ToString(),
                SettingType.SessionToken => _settings.BuffSessionToken,
                SettingType.TimeoutLoading => $"{_settings.TimeoutLoadingMS}ms",
                SettingType.ItemCachedTimeS => $"{_settings.ItemCachedTimeS}sec",
                SettingType.DepositPriceService => _settings.DepositPriceService.ToString(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
