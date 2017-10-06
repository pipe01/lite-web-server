using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public class Configuration : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> _Entries = new Dictionary<string, object>();
        private const string KeyValueSeparator = "=";
        
        public string FilePath { get; set; } = "config.cfg";

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var item in _Entries)
                yield return item;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in _Entries)
                yield return item;
        }

        public T Get<T>(string key, T def)
        {
            if (!_Entries.ContainsKey(key))
            {
                _Entries.Add(key, def);
                Save();
            }

            return (T)_Entries[key];
        }
        public bool TryGet<T>(string key, out T value)
        {
            if (_Entries.ContainsKey(key))
            {
                value = (T)_Entries[key];
                return true;
            }

            value = default(T);
            return false;
        }
        
        public bool IsSet(string key)
        {
            return _Entries.ContainsKey(key);
        }

        public void Set(string key, object val)
        {
            _Entries[key] = val;
            Save();
        }

        public void Save(string path = null)
        {
            path = path ?? FilePath;

            List<string> lines = new List<string>();
            
            foreach (var item in _Entries)
            {
                string key = WebUtility.UrlEncode(item.Key);
                object val = item.Value;

                if (val is string)
                    val = "\"" + val + "\"";

                lines.Add(key + KeyValueSeparator + val);
            }
            
            File.WriteAllLines(path, lines.ToArray());
        }

        public void Load(string path = null)
        {
            path = path ?? FilePath;

            _Entries.Clear();

            if (!File.Exists(path))
                Save();

            string[] lines = File.ReadAllLines(path);
            foreach (var item in lines)
            {
                if (!item.Contains(KeyValueSeparator) || item.StartsWith("#", StringComparison.Ordinal))
                    continue;

                int sepIndex = item.IndexOf(KeyValueSeparator, StringComparison.Ordinal);

                string key = WebUtility.UrlDecode(item.Substring(0, sepIndex));
                string value = item.Substring(sepIndex + KeyValueSeparator.Length);

                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    string str = value.Substring(1, value.Length - 2);
                    _Entries.Add(key, str);
                }
                else if (int.TryParse(value, out int intVal))
                {
                    _Entries.Add(key, intVal);
                }
                else if (bool.TryParse(value, out bool boolVal))
                {
                    _Entries.Add(key, boolVal);
                }
                else if (value.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    _Entries.Add(key, null);
                }
            }
        }
    }
}
