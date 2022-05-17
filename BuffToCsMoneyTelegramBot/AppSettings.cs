using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuffToCsMoneyTelegramBot
{
    public abstract class AppSettings<T> : INotifyPropertyChanged where T : new()
    {
        private const string DEFAULT_FILENAME = "settings.json";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static void Save(T t, string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(t));
        }
        public void Save(string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(this));
        }

        public static T Load(string fileName = DEFAULT_FILENAME)
        {
            T t = new T();
            if (File.Exists(fileName))
                t = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
            return t;
        }
    }
}
