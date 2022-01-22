using System;
using System.Collections.Generic;
using OverstockTelegramBot.Models.Commands;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace OverstockTelegramBot
{
    class Program
    {
        private static string Token { get; set; } = "1822527504:AAFyMh-nSNyzd_7yb4kBSfQIECH2YSUSHJo";
        private static List<Command> commandsList;

        private static TelegramBotClient client;

        static void Main(string[] args)
        {
            client = new TelegramBotClient(Token);
            SetCommands();

            client.StartReceiving();
            client.OnMessage += OnMessageHandler;

            while (true)
            {
                if ("exit" == Console.ReadLine())
                    break;
            }

            client.StopReceiving();
        }

        private static void SetCommands()
        {
            commandsList = new List<Command>();
            commandsList.Add(new HelloCommand());
            commandsList.Add(new StatusCommand());
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            if (msg.Text != null && msg.Text.StartsWith('/'))
            {
                int endOfCommand = msg.Text.IndexOf(' ');
                if (endOfCommand == -1) endOfCommand = msg.Text.Length;
                string receivedCommand = msg.Text.Substring(1, endOfCommand - 1);

                foreach (var command in commandsList)
                {
                    if (command.Contains(receivedCommand))
                    {
                        await command.ExecuteAsync(msg, client);
                        break;
                    }
                }
            }
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Стикер"}, new KeyboardButton { Text = "Картинка"} },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "123"}, new KeyboardButton { Text = "456"} }
                }
            };
        }
    }
}
