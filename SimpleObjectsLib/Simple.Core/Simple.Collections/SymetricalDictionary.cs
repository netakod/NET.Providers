using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and values. Keys must be unique, and values also.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the symetrical dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the symetrical dictionary.</typeparam>
    public class SymetricalDictionary<TKey, TValue>
    {
        private SimpleDictionary<TKey, TValue> valuesByKey = new SimpleDictionary<TKey, TValue>();
        private SimpleDictionary<TValue, TKey> keysByValue = new SimpleDictionary<TValue, TKey>();

        public void Add(TKey key, TValue value)
        {
            this.valuesByKey.Add(key, value);
            this.keysByValue.Add(value, key);
        }

        public bool RemoveByKey(TKey key)
        {
            if (this.valuesByKey.ContainsKey(key))
            {
                TValue value = this.valuesByKey[key];
                this.valuesByKey.Remove(key);
                this.keysByValue.Remove(value);

                return true;
            }

            return false;
        }

        public bool RemoveByValue(TValue value)
        {
            if (this.keysByValue.ContainsKey(value))
            {
                TKey key = this.keysByValue[value];
                this.valuesByKey.Remove(key);
                this.keysByValue.Remove(value);

                return true;
            }

            return false;
        }

        public TValue GetValueByKey(TKey key)
        {
            TValue value;
            this.valuesByKey.TryGetValue(key, out value);

            return value;
        }

        public TKey GetKeyByValue(TValue value)
        {
            TKey key;
            this.keysByValue.TryGetValue(value, out key);

            return key;
        }

        public bool ContainsKey(TKey key)
        {
            return this.valuesByKey.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return this.keysByValue.ContainsKey(value);
        }

        public void Clear()
        {
            this.valuesByKey.Clear();
            this.keysByValue.Clear();
        }

        public IDictionary<TKey, TValue> ValuesByKey
        {
            get { return this.valuesByKey.AsReadOnly(); }
        }

        public IDictionary<TValue, TKey> KeysByValue
        {
            get { return this.keysByValue.AsReadOnly(); }
        }
    }
}
