using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and values. Original dictionary is <see cref="T:System.Collections.IDictionary`2"></see> interface and it is wrapped to UValue type.
    /// Note that any element of the input list must be custable to the UValue type.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="UValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public class CustomDictionary<TKey, TValue, UValue> : IDictionary<TKey, UValue>, ICollection<KeyValuePair<TKey, UValue>>, IEnumerable<KeyValuePair<TKey, UValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
        where UValue : TValue
    {
        #region |   Private Members   |

        private IDictionary<TKey, TValue> originalDictionary = null;
        private CustomCollection<TKey> customKeys;
        private CustomCollection<TValue, UValue> customValues;
        private ReadOnlyDictionary<TKey, UValue> readOnlyDictionary = null;

        #endregion |   Private Members   |

        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2"></see> 
        /// class that wrap around the specified non-generic dictionary.
        /// </summary>
        /// <param name="dictionaryToWrap">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public CustomDictionary(IDictionary<TKey, TValue> dictionaryToWrap)
        {
            if (dictionaryToWrap != null)
            {
                this.originalDictionary = dictionaryToWrap;
            }
            else
            {
                throw new System.ArgumentNullException("dictionary is null.");
            }
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Protected Properties   |

        protected IDictionary<TKey, TValue> OriginalDictionary
        {
            get { return this.originalDictionary; }
        }

        #endregion |   Protected Properties   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the <see cref="UValue"/> with the specified key. Set is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="UValue"/> value.</returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public UValue this[TKey key]
        {
            get { return (UValue)this.DictionaryGet(key); }
            set { this.DictionarySet(key, value); }
        }

        /// <summary>
        /// Gets a read-only <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get { return this.CustomKeys; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<UValue> Values
        {
            get { return this.CustomValues; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return this.OriginalDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return this.OriginalDictionary.IsReadOnly; }
        }

        #endregion |   Public Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2"></see> wrapper for the current dictionary.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2"></see> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IDictionary"></see>.</returns>
        public ReadOnlyDictionary<TKey, UValue> AsReadOnly()
        {
            if (this.readOnlyDictionary == null)
            {
                this.readOnlyDictionary = new ReadOnlyDictionary<TKey, UValue>(this);
            }

            return readOnlyDictionary;
        }

        /// <summary>
        /// Determines whether the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/>.</param>
        /// <returns>
        /// true if the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return this.CustomKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/> contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/>. The value can be null for reference types.
        /// <returns>
        /// true if the <see cref="CustomDictionary&lt;TKey, TValue, UValue&gt;"/> contains an element with the value; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">value is null.</exception>
        public bool ContainsValue(UValue value)
        {
            return this.CustomValues.Contains(value);
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">Key is null.</exception>
        public void Add(TKey key, UValue value)
        {
            this.DictionaryAdd(key, value);
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            bool keyExists = this.ContainsKey(key);

            if (keyExists)
            {
                this.DictionaryRemove(key);
            }

            return keyExists;
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out UValue value)
        {
            value = default(UValue);
            bool keyExists = this.ContainsKey(key);

            if (keyExists)
            {
                value = (UValue)this.DictionaryGet(key);
            }

            return keyExists;
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.
        /// </summary>
        public void Clear()
        {
            this.DictionaryClear();
        }

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            IDeserializationCallback callback = originalDictionary as IDeserializationCallback;
            callback.OnDeserialization(sender);
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ISerializable serializable = originalDictionary as ISerializable;
            serializable.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, UValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this.OriginalDictionary)
                yield return new KeyValuePair<TKey, UValue>(keyValuePair.Key, (UValue)keyValuePair.Value);
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        /// <summary>
        /// Gets a read-only <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected CustomCollection<TKey> CustomKeys
        {
            get
            {
                if (this.customKeys == null)
                    this.customKeys = new CustomCollection<TKey>(this.originalDictionary.Keys);

                return this.customKeys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected CustomCollection<TValue, UValue> CustomValues
        {
            get
            {
                if (this.customValues == null)
                    this.customValues = new CustomCollection<TValue, UValue>(this.originalDictionary.Values);

                return this.customValues;
            }
        }

        protected void DictionaryAdd(TKey key, TValue value)
        {
            this.OnAdd(key, value);
            this.OriginalDictionary.Add(key, value);
        }

        protected void DictionaryRemove(TKey key)
        {
            this.OnRemove(key);
            this.OriginalDictionary.Remove(key);
        }

        protected object DictionaryGet(TKey key)
        {
            return this.OriginalDictionary[key];
        }

        protected void DictionarySet(TKey key, TValue value)
        {
            this.OnSet(key, value);
            this.OriginalDictionary[key] = value;
        }

        protected void DictionaryClear()
        {
            this.OnClear();
            this.OriginalDictionary.Clear();
        }

        protected virtual void OnAdd(TKey key, TValue value)
        {
        }

        protected virtual void OnRemove(TKey key)
        {
        }

        protected virtual void OnSet(TKey key, TValue value)
        {
        }

        protected virtual void OnClear()
        {
        }

        #endregion |   Protected Methods   |

        #region |   Interface ICollection<KeyValuePair<TKey, TValue>>   |

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, UValue>>.Contains(KeyValuePair<TKey, UValue> item)
        {

            foreach (KeyValuePair<TKey, UValue> keyValuePair in this)
            {
                if (keyValuePair.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        void ICollection<KeyValuePair<TKey, UValue>>.Add(KeyValuePair<TKey, UValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the value with the specified key from the dictionary.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, UValue>>.Remove(KeyValuePair<TKey, UValue> item)
        {
            return this.Remove(item.Key);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        void ICollection<KeyValuePair<TKey, UValue>>.CopyTo(KeyValuePair<TKey, UValue>[] array, int arrayIndex)
        {
            int j = arrayIndex;
            
            foreach (TValue value in this.CustomValues)
            {
                array.SetValue(value, j);
                j++;
            }
        }

        #endregion |   Interface ICollection<KeyValuePair<TKey, TValue>>   |

        #region |   Interface IDictionary   |

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- 
        /// The property is set, key does not exist in the collection, and the <see cref="T:System.Collections.IDictionary"></see> has a fixed size.</exception>
        object IDictionary.this[object key]
        {
            get { return this[(TKey)key]; }
            set { this[(TKey)key] = (UValue)value; }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
        /// <returns> true if the see <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add.</param>
        /// <param name="value"> The <see cref="T:System.Object"></see> to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.IDictionary"></see> object.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size.</exception>
        void IDictionary.Add(object key, object value)
        {
            this.Add((TKey)key, (UValue)value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size.</exception>
        void IDictionary.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Keys
        {
            get { return this.CustomKeys; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Values
        {
            get { return this.CustomValues; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.</returns>
        bool IDictionary.IsFixedSize
        {
            get { return (this.OriginalDictionary as IDictionary).IsFixedSize; }
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (this.OriginalDictionary as IDictionary).GetEnumerator();
        }

        #endregion |   Interface IDictionary   |

        #region |   Interface ICollection   |

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
        /// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
        void ICollection.CopyTo(Array array, int index)
        {
            (this.OriginalDictionary as ICollection).CopyTo(array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return (this.OriginalDictionary as ICollection).IsSynchronized; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return (this.OriginalDictionary as ICollection).SyncRoot; }
        }

        #endregion |   Interface ICollection   |

        #region |   Interface IEnumerable   |

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.OriginalDictionary.GetEnumerator();
        }

        #endregion |   Interface IEnumerable   |
    }
}
