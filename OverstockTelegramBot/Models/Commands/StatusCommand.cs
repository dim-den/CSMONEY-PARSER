using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsMoneyAPI.Models.Results;
using CsMoneyAPI.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OverstockTelegramBot.Models.Commands
{
    public class StatusCommand : Command
    {
        public override string Name => "status";

        public override async Task ExecuteAsync(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            string itemName = message.Text.Substring(message.Text.IndexOf(' ') + 1);

            string response = "";

            if (itemName == '/' + Name)
            {
                response = "You have to set item name after status: '/status M4A4 | In Living Color (Minimal Wear)'";
            }
            else
            {
                ItemOverstockStatus itemOverstockStatus = await OverstockStatusService.GetItemOverstockStatus(itemName);

                if(itemOverstockStatus.Error == null)
                {
                    response = $"Item \'{itemName}\' status: {itemOverstockStatus.Status.ToString().ToLower()} (overstock limit: {itemOverstockStatus.OverstockDiff})";
                }
                else
                {
                    response = $"There is no item with name \'{itemName}\'";
                }
            }

            await client.SendTextMessageAsync(chatId, response);
        }
    }
}
