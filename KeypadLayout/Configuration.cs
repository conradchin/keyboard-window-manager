using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeypadLayout
{
    public class Configuration
    {
        private string _fileName;

        private Dictionary<string, string> _data = new Dictionary<string, string>();

        public Configuration(string fileName, Dictionary<string, string> defaults)
        {
            _fileName = fileName;
            if(fileName != null && File.Exists(_fileName))
            {
                Deserialize();
            }

            AppendDefaults(defaults);
        }

        private void AppendDefaults(Dictionary<string, string> defaults)
        {
            var newItems = defaults.Where(a => !_data.ContainsKey(a.Key)).ToDictionary(i => i.Key, i => i.Value);
            if(newItems.Count > 0)
            {
                foreach(var i in newItems)
                {
                    _data[i.Key] = i.Value;
                }
                Serialize();
            }
        }

        public bool Deserialize()
        {
            try
            {
                foreach (var line in File.ReadAllLines(_fileName))
                {
                    var parts = line.Split(new char[] { '=' }, 2);
                    _data[parts[0]] = parts[1].TrimStart();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public bool Serialize()
        {
            using (StreamWriter w = new StreamWriter(_fileName))
            {
                foreach (var item in _data)
                {
                    w.WriteLine($"{item.Key}={item.Value}");
                }

            }

            return true;
        }
        
        internal void SetConfigurationValue(string item, string value)
        {
            _data[item] = value;
        }

        internal string GetConfigurationValue(string item)
        {
            if(_data.ContainsKey(item))
            {
                return _data[item];
            }

            return null;
        }

        internal void WriteChanges()
        {
            Serialize();
        }
    }
}
