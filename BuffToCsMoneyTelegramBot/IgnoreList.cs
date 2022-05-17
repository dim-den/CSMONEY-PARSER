using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuffToCsMoneyTelegramBot
{
    public enum IgnorListActionType
    {
        Add,
        Remove
    }
    public class IgnoreList : AppSettings<IgnoreList>
    {
        private const string DEFAULT_FILENAME = "IgnoreList.json";

        private static IgnoreList _ignoreList;
        private static object _syncRoot = new Object();

        public static IgnoreList GetInstance()
        {
            if (_ignoreList == null)
            {
                lock (_syncRoot)
                {
                    if (_ignoreList == null)
                        _ignoreList = Load(DEFAULT_FILENAME);
                }
            }
            return _ignoreList;
        }

        public ObservableCollection<string> IgnoredItems { get; set; }

        public IgnoreList()
        {
            IgnoredItems = new ObservableCollection<string>();

            IgnoredItems.CollectionChanged += 
                (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Save(DEFAULT_FILENAME);
        }

        public bool Contains(string itemName) => IgnoredItems.Contains(itemName);
        public void Add(string itemName) => IgnoredItems.Add(itemName);
        public bool Remove(string itemName) => IgnoredItems.Remove(itemName);
        public bool Remove(int pos)
        {
            if (pos >= 0 && pos < IgnoredItems.Count)
            {
                IgnoredItems.RemoveAt(pos);
                return true;
            }

            return false;
        }

    }
}
