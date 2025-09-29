using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> value, IDictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            foreach (var entity in dictionary)
                value.Add(entity.Key, entity.Value);
        }
    }
}
