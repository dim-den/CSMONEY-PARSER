using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuffToCsMoneyTelegramBot.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        private string _itemName;
        public ItemNotFoundException(string itemName)
        {
            _itemName = itemName;
        }

        public string ItemName { get => _itemName; set => _itemName = value; }
    }
}
