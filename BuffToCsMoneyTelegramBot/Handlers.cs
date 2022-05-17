using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OverstockTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BuffToCsMoneyTelegramBot
{
    public class Handlers
    {
        struct ApplyingSettings
        {
            public bool IsApplying;
            public SettingType SettingType;
        }

        struct ChangeIgnoreList
        {
            public bool IsChanging;
            public IgnorListActionType ActionType;
        }

        private static ApplyingSettings _applyingSettings = new ApplyingSettings() { IsApplying = false };
        private static ChangeIgnoreList _changeIgnoreList = new ChangeIgnoreList() { IsChanging = false };

        private static Settings _settings = Settings.GetInstance();
        private static IgnoreList _ignoreList = IgnoreList.GetInstance();

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                //UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0] switch
            {
                "/settings" => SendSettingsInlineKeyboard(botClient, message),
                "/ignore" => SendIgnoreListInlineKeyboard(botClient, message),
                "/stop" => StopBotParsing(botClient, message),
                "/continue" => ContinueBotParsing(botClient, message),
                _ => _applyingSettings.IsApplying ? ApplyBotSettings(botClient, message) :
                     _changeIgnoreList.IsChanging ? EditIgnoreList(botClient, message) :
                     Usage(botClient, message)
            };

            Message sentMessage = await action;

            static async Task<Message> SendIgnoreListInlineKeyboard(ITelegramBotClient botClient, Message message)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                   new[]
                   {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Add item", IgnorListActionType.Add.ToString()),
                        InlineKeyboardButton.WithCallbackData("Remove item", IgnorListActionType.Remove.ToString()),
                    },
                   });

                string msg = $"Current ignore list:\n\n";

                if (_ignoreList.IgnoredItems.Count == 0)
                    msg += "No items yet";

                for (int i = 0; i < _ignoreList.IgnoredItems.Count; i++)
                {
                    msg += $"{i + 1}. \'{_ignoreList.IgnoredItems[i]}\'\n";
                }

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: msg,
                                                            replyMarkup: inlineKeyboard);
            }

            static async Task<Message> EditIgnoreList(ITelegramBotClient botClient, Message message)
            {
                _changeIgnoreList.IsChanging = false;

                string msg = message.Text;
                string response = String.Empty;

                if (_changeIgnoreList.ActionType == IgnorListActionType.Add)
                {
                    if (!_ignoreList.Contains(msg))
                    {
                        _ignoreList.Add(msg);
                        response = $"Succesfully added item \'{msg}\' to ignore list";
                    }
                    else
                    {
                        response = "Item is already in ignore list";
                    }
                }
                else if (_changeIgnoreList.ActionType == IgnorListActionType.Remove)
                {
                    bool isRemoved = false;
                    string removedItemName = msg;

                    if (Int32.TryParse(msg, out int index))
                    {
                        removedItemName = _ignoreList.IgnoredItems.ElementAtOrDefault(index - 1);
                        isRemoved = _ignoreList.Remove(index - 1);
                    }
                    else
                    {
                        isRemoved = _ignoreList.Remove(msg);
                    }

                    response = isRemoved switch
                    {
                        true => $"Succesfully removed item \'{removedItemName}\'",
                        false => "Wrong item name or position",
                    };
                }

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            replyToMessageId: message.MessageId,
                                                            text: response);
            }

            static async Task<Message> ApplyBotSettings(ITelegramBotClient botClient, Message message)
            {
                _applyingSettings.IsApplying = false;

                string msg = message.Text;
                string response = String.Empty;

                switch (_applyingSettings.SettingType)
                {
                    case SettingType.Fees:
                        if (!Int32.TryParse(msg, out int fees))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.Fees = fees;
                        break;

                    case SettingType.MinProfit:
                        if (!Int32.TryParse(msg, out int minProfit))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.MinProfitPercent = minProfit;
                        break;

                    case SettingType.ParseMode:
                        if (!Enum.TryParse(msg, out BotParseMode botParseMode))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.ParseMode = botParseMode;
                        break;

                    case SettingType.MinPrice:
                        if (!Double.TryParse(msg, out double minPrice))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.MinBuffPriceUSD = minPrice;
                        break;

                    case SettingType.MaxPrice:
                        if (!Double.TryParse(msg, out double maxPrice))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.MaxBuffPriceUSD = maxPrice;
                        break;

                    case SettingType.OverstockLimit:
                        if (!Int32.TryParse(msg, out int overstockLimit))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.MinOverstockLimit = overstockLimit;
                        break;

                    case SettingType.SessionToken:
                        _settings.BuffSessionToken = msg.Length > 15 ? msg : String.Empty;
                        break;

                    case SettingType.TimeoutLoading:
                        if (!Int32.TryParse(msg, out int timeoutLoading))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.TimeoutLoadingMS = timeoutLoading;
                        break;

                    case SettingType.ItemCachedTimeS:
                        if (!Int32.TryParse(msg, out int itemCachedTimeS))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.ItemCachedTimeS = itemCachedTimeS;
                        break;

                    case SettingType.DepositPriceService:
                        if (!Enum.TryParse(msg, out DepositPriceService depositPriceService))
                            response = $"Wrong value for parameter \'{_applyingSettings.SettingType}\'";
                        else
                            _settings.DepositPriceService = depositPriceService;
                        break;
                };

                if (response == String.Empty)
                {
                    response = $"Succesfully set new value for parameter \'{_applyingSettings.SettingType.ToFriendlyString()}\', " +
                    $"new value: {_settings.GetStringValueBySettingType(_applyingSettings.SettingType)}";
                }

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            replyToMessageId: message.MessageId,
                                                            text: response);
            }

            static async Task<Message> SendSettingsInlineKeyboard(ITelegramBotClient botClient, Message message)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(SettingType.Fees.ToFriendlyString(), SettingType.Fees.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.MinProfit.ToFriendlyString(), SettingType.MinProfit.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.ParseMode.ToFriendlyString(), SettingType.ParseMode.ToString()),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(SettingType.MinPrice.ToFriendlyString(), SettingType.MinPrice.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.MaxPrice.ToFriendlyString(), SettingType.MaxPrice.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.OverstockLimit.ToFriendlyString(), SettingType.OverstockLimit.ToString()),
                    },
                    // third row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(SettingType.SessionToken.ToFriendlyString(), SettingType.SessionToken.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.TimeoutLoading.ToFriendlyString(), SettingType.TimeoutLoading.ToString()),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(SettingType.DepositPriceService.ToFriendlyString(), SettingType.DepositPriceService.ToString()),
                        InlineKeyboardButton.WithCallbackData(SettingType.ItemCachedTimeS.ToFriendlyString(), SettingType.ItemCachedTimeS.ToString()),
                    },
                    });

                string msg = $"Current settings:\n" +
                    $"Fees: {_settings.Fees}%\n" +
                    $"Min profit: {_settings.MinProfitPercent}%\n" +
                    $"Parse mode: {_settings.ParseMode} (0-{BotParseMode.AllPages}, 1-{BotParseMode.NewItems})\n" +
                    $"Min price: {_settings.MinBuffPriceUSD}$\n" +
                    $"Max price: {_settings.MaxBuffPriceUSD}$\n" +
                    $"Overtock limit: {_settings.MinOverstockLimit}\n" +
                    $"Session token: {_settings.BuffSessionToken}\n" +
                    $"Timeout loading: {_settings.TimeoutLoadingMS}ms\n" +
                    $"Item cached time: {_settings.ItemCachedTimeS}sec\n" +
                    $"Deposit price service: {_settings.DepositPriceService} (0-{DepositPriceService.MrInki}, 1-{DepositPriceService.WikiCsMoney})\n\n" +
                    $"What setting you want tot change?";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: msg,
                                                            replyMarkup: inlineKeyboard);
            }


            static async Task<Message> StopBotParsing(ITelegramBotClient botClient, Message message)
            {
                Program.ParsePageTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Program.ParsePageStopped = true;

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            replyToMessageId: message.MessageId,
                                                            text: "Bot parsing stopped");
            }

            static async Task<Message> ContinueBotParsing(ITelegramBotClient botClient, Message message)
            {
                Program.ParsePageTimer.Change(0, _settings.TimeoutLoadingMS);
                Program.ParsePageStopped = false;

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            replyToMessageId: message.MessageId,
                                                            text: "Bot parsing continued");
            }



            static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/settings - set bot settings\n" +
                                     "/ignore - list of ignored items\n" +
                                     "/stop - stop parsing\n" +
                                     "/continue - continue parsing\n";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (Enum.TryParse(callbackQuery.Data, out SettingType settingType))
            {
                string msg = String.Empty;
                _applyingSettings.SettingType = settingType;
                _applyingSettings.IsApplying = true;

                msg = $"Send new value for parameter \'{settingType.ToFriendlyString()}' (current value: {_settings.GetStringValueBySettingType(settingType)})";

                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: msg);
            }
            else if (Enum.TryParse(callbackQuery.Data, out IgnorListActionType actionType))
            {
                string msg = actionType switch
                {
                    IgnorListActionType.Add => "Send item name to ignore",
                    IgnorListActionType.Remove => "Send item name or position in list to remove from ignore list",
                    _ => throw new NotImplementedException(),
                };

                _changeIgnoreList.ActionType = actionType;
                _changeIgnoreList.IsChanging = true;

                await botClient.SendTextMessageAsync(
                       chatId: callbackQuery.Message.Chat.Id,
                       text: msg);
            }
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
