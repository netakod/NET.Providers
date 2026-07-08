using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Simple.Collections
{
    public class SecondKeyDictionary<TKey, TSecondKey, TValue> : SimpleDictionary<TSecondKey, TValue>, IDictionaryWithEvents<TSecondKey, TValue>, IDictionary<TSecondKey, TValue>, ICollection<KeyValuePair<TSecondKey, TValue>>, IDictionaryEvents<TSecondKey, TValue>, IEnumerable<KeyValuePair<TSecondKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        public SecondKeyDictionary(IDictionaryWithEvents<TKey, TValue> originalDictionary, Func<TKey, TValue, TSecondKey> getSecondKey)
        {
            this.OriginalDictionary = originalDictionary;
            this.GetSecondKey = getSecondKey;

            foreach (KeyValuePair<TKey, TValue> originalKeyValuePair in this.OriginalDictionary)
            {
                TSecondKey secondKey = this.GetSecondKey(originalKeyValuePair.Key, originalKeyValuePair.Value);
                this.Add(secondKey, originalKeyValuePair.Value);
            }

            this.IsReadOnly = true;

            this.OriginalDictionary.AfterAdd += new DictionaryActionEventHandler<TKey, TValue>(OriginalDictionary_AfterAdd);
            this.OriginalDictionary.AfterSet += new DictionaryActionOldValueEventHandler<TKey, TValue>(OriginalDictionary_AfterSet);
            this.OriginalDictionary.BeforeRemove += new DictionaryActionEventHandler<TKey, TValue>(OriginalDictionary_BeforeRemove);
            this.OriginalDictionary.BeforeClear += new EventHandler(OriginalDictionary_BeforeClear);
        }

        protected IDictionaryWithEvents<TKey, TValue> OriginalDictionary { get; private set; }
        protected Func<TKey, TValue, TSecondKey> GetSecondKey { get; private set; }

        private void OriginalDictionary_AfterAdd(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            TSecondKey secondKey = this.GetSecondKey(e.Key, e.Value);
            this.InternalDictionaryAdd(secondKey, e.Value);
        }

        private void OriginalDictionary_AfterSet(object sender, DictionaryActionOldValueEventArgs<TKey, TValue> e)
        {
            TSecondKey secondKey = this.GetSecondKey(e.Key, e.Value);
            this.InternalDictionarySet(secondKey, e.Value);
        }

        private void OriginalDictionary_BeforeRemove(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            TSecondKey secondKey = this.GetSecondKey(e.Key, e.Value);
            this.InternalDictionaryRemove(secondKey);
        }

        private void OriginalDictionary_BeforeClear(object sender, EventArgs e)
        {
            this.InternalDictionaryClear();
        }
    }
}
