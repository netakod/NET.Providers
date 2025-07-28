using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and values. Original dictionary can be <see cref="T:System.Collections.IDictionary"></see> interface and it is wrapped to T type.
    /// Note that any element of the input list must be custable to the T type. As addition
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SimpleTwoKeysDictionary<TKey, TKey2, TValue> : SimpleDictionary<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private SimpleDictionary<TKey2, TValue> key2Dictionary = new SimpleDictionary<TKey2, TValue>();

    }
}
