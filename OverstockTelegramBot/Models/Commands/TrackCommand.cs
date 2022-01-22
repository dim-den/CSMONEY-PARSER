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
    public class TrackCommand : Command
    {
        public override string Name => "track";

        public override async Task ExecuteAsync(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            string itemName = message.Text.Substring(message.Text.IndexOf(' ') + 1);

            string response = "";

            ItemOverstockStatus itemOverstockStatus = await OverstockStatusService.GetItemOverstockStatus(itemName);

            if (itemOverstockStatus.Error == null)
            {
                if(itemOverstockStatus.Status == Status.Tradable)
                {
                    response = $"Item \'{itemName}\' is tradable now (overstock limit: {itemOverstockStatus.OverstockDiff}";
                }
                else if(itemOverstockStatus.Status == Status.Overstock)
                {
                    response = $"Item \'{itemName}\' was added to your track list (command \'/list\' to see yours). We will notify you when it will become tradable";

                    //todo add db + logic
                }                
            }
            else
            {
                response = $"There is no item with name \'{itemName}\'";
            }

            await client.SendTextMessageAsync(chatId, response);
        }
    }
}
