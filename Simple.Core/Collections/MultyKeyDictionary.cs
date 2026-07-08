// *************************************************
// Code written by Aron Weiler and Richard Massey
// Feel free to use this code in any way you like
// Comments?  Email aronweiler@gmail.com
// *************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Multi-Key Dictionary Class
    /// </summary>
    /// <typeparam name="V">Value Type</typeparam>
    /// <typeparam name="K">Primary Key Type</typeparam>
    /// <typeparam name="L">Sub Key Type</typeparam>
    public class MultiKeyDictionary<K, L, V>
    {
        internal readonly Dictionary<K, V> baseDictionary = new Dictionary<K, V>();
        internal readonly Dictionary<L, K> subDictionary = new Dictionary<L, K>();
        internal readonly Dictionary<K, L> primaryToSubkeyMapping = new Dictionary<K, L>();

        readonly object lockObject = new object();

        public V this[L subKey]
        {
            get
            {
                V item;
                if (TryGetValue(subKey, out item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey.ToString());
            }
        }

        public V this[K primaryKey]
        {
            get
            {
                V item;
                if (TryGetValue(primaryKey, out item))
                    return item;

                throw new KeyNotFoundException("primary key not found: " + primaryKey.ToString());
            }
        }

        public void Associate(L subKey, K primaryKey)
        {
            lock (lockObject)
            {
                if (!baseDictionary.ContainsKey(primaryKey))
                    throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'", primaryKey));

                if (subDictionary.ContainsKey(subKey))
                {
                    subDictionary[subKey] = primaryKey;
                    primaryToSubkeyMapping[primaryKey] = subKey;
                }
                else
                {
                    subDictionary.Add(subKey, primaryKey);
                    primaryToSubkeyMapping.Add(primaryKey, subKey);
                }
            }
        }

        public bool TryGetValue(L subKey, out V val)
        {
            val = default(V);

            lock (lockObject)
            {
                K ep;
                if (subDictionary.TryGetValue(subKey, out ep))
                {
                    if (!TryGetValue(ep, out val))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetValue(K primaryKey, out V val)
        {
            lock (lockObject)
            {
                if (!baseDictionary.TryGetValue(primaryKey, out val))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsKey(L subKey)
        {
            V val;

            return TryGetValue(subKey, out val);
        }

        public bool ContainsKey(K primaryKey)
        {
            V val;

            return TryGetValue(primaryKey, out val);
        }

        public void Remove(K primaryKey)
        {
            lock (lockObject)
            {
                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                        subDictionary.Remove(primaryToSubkeyMapping[primaryKey]);

                    primaryToSubkeyMapping.Remove(primaryKey);
                }

                baseDictionary.Remove(primaryKey);
            }
        }

        public void Remove(L subKey)
        {
            lock (lockObject)
            {
                baseDictionary.Remove(subDictionary[subKey]);

                primaryToSubkeyMapping.Remove(subDictionary[subKey]);
                subDictionary.Remove(subKey);
            }
        }

        public void Add(K primaryKey, V val)
        {
            lock (lockObject)
                baseDictionary.Add(primaryKey, val);
        }

        public void Add(K primaryKey, L subKey, V val)
        {
            lock (lockObject)
                baseDictionary.Add(primaryKey, val);

            Associate(subKey, primaryKey);
        }

        public V[] CloneValues()
        {
            lock (lockObject)
            {
                V[] values = new V[baseDictionary.Values.Count];

                baseDictionary.Values.CopyTo(values, 0);

                return values;
            }
        }

        public K[] ClonePrimaryKeys()
        {
            lock (lockObject)
            {
                K[] values = new K[baseDictionary.Keys.Count];

                baseDictionary.Keys.CopyTo(values, 0);

                return values;
            }
        }

        public L[] CloneSubKeys()
        {
            lock (lockObject)
            {
                L[] values = new L[subDictionary.Keys.Count];

                subDictionary.Keys.CopyTo(values, 0);

                return values;
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                baseDictionary.Clear();
                subDictionary.Clear();
                primaryToSubkeyMapping.Clear();
            }
        }

        public int Count
        {
            get
            {
                lock (lockObject)
                    return baseDictionary.Count;
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            lock (lockObject)
                return baseDictionary.GetEnumerator();
        }
    }
}