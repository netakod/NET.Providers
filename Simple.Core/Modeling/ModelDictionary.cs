using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Simple.Collections;

namespace Simple.Modeling
{
    public class ModelDictionary<TKey, TValue> : IDictionaryWithEvents<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionaryEvents<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback 
		where TValue : ModelElement
    {
        #region |   Constructor(s) and Initialization   |
        
        public ModelDictionary()
        {
            this.InnerDictionary = new SimpleDictionaryHelper<TKey, TValue>();
        }

        public ModelDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                this.InnerDictionary = new SimpleDictionaryHelper<TKey, TValue>(dictionary);
            }
            else
            {
                throw new ArgumentNullException("dictionary is null.");
            }

            this.Initialize();
        }


        #endregion |   Constructor(s) and Initialization   |

        #region |   Events   |

        public event DictionaryActionEventHandler<TKey, TValue> BeforeAdd;
        public event DictionaryActionEventHandler<TKey, TValue> AfterAdd;
        public event DictionaryActionEventHandler<TKey, TValue> BeforeRemove;
        public event DictionaryActionEventHandler<TKey, TValue> AfterRemove;
        public event DictionaryActionOldValueEventHandler<TKey, TValue> BeforeSet;
        public event DictionaryActionOldValueEventHandler<TKey, TValue> AfterSet;
        public event EventHandler BeforeClear;
        public event OldCountEventHandler AfterClear;

        #endregion |   Events   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the <see cref="TValue"/> with the specified key. Set is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="TValue"/> value.</returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        public TValue this[TKey key]
        {
            get { return this.InnerDictionary[key]; }
            set { this.InnerDictionary[key] = value; }
        }

        /// <summary>
        /// Gets a read-only <see cref="System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get { return this.InnerDictionary.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.Generic.ICollection`1"/> containing the custom values in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TValue> Values
        {
            get { return this.InnerDictionary.Values; }
        }

        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the dictionary.</returns>
        public int Count
        {
            get { return this.InnerDictionary.Count; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return this.InnerDictionary.IsReadOnly; }
            set { this.InnerDictionary.IsReadOnly = value; }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        protected SimpleDictionaryHelper<TKey, TValue> InnerDictionary { get; private set; }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        //public void BuildModels(Func<TValue, TKey> getKey)
        //{
        //    this.BuildModels(this, getKey);
        //}

        //public void BuildModels<TObjectModelFieldHolderType>(Func<TValue, TKey> getKey)
        //{
        //    object objectModelFieldHolderInstance = Activator.CreateInstance<TObjectModelFieldHolderType>();
        //    this.BuildModels(objectModelFieldHolderInstance, getKey);
        //}

        //public void BuildModels(object objectModelFieldHolderInstance, Func<TValue, TKey> getKey)
        //{
        //    IDictionary<TKey, TValue> dictionary = this.CreateModelDictionary<TKey, TValue>(objectModelFieldHolderInstance, getKey);
        //    this.AddRange(dictionary);
        //}

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> wrapper for the current dictionary.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> that acts as a read-only wrapper around the current <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</returns>
        public ReadOnlyDictionary<TKey, TValue> AsReadOnly()
        {
            return this.InnerDictionary.AsReadOnly();
        }

        /// <summary>
        /// Returns a <see cref="CustomDictionary&lt;TKey, TValue&gt;"/> type dictionary object that is wrapper for the current TValue dictionary value type.
        /// </summary>
        /// <typeparam name="UValue">Type parameter UValue to convert values from TValue type.</typeparam>
        /// <returns>Customized dictionary with values converted from TValue to UValue type.</returns>
        public CustomDictionary<TKey, UValue> AsCustom<UValue>()
        {
            return this.InnerDictionary.AsCustom<UValue>();
        }

        /// <summary>
        /// Returns a <see cref="CustomDictionary&lt;TKey, TValue&gt;"/> type dictionary object that is wrapper for the current TKey and TValue dictionary value type.
        /// </summary>
        /// <typeparam name="UKey">Type parameter UKey to convert values from TKey type.</typeparam>
        /// <typeparam name="UValue">Type parameter UValue to convert values from TValue type.</typeparam>
        /// <returns>Customized dictionary with values converted from TKey and TValue to UKey and UValue type, respectively.</returns>
        public CustomDictionary<UKey, UValue> AsCustom<UKey, UValue>()
        {
            return this.InnerDictionary.AsCustom<UKey, UValue>();
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return this.InnerDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>. The value can be null for reference types.
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the value; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">value is null.</exception>
        public bool ContainsValue(TValue value)
        {
            return this.InnerDictionary.ContainsValue(value);
        }

        /// <summary>
        /// Adds the specified key and value to the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">Key is null.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            this.InnerDictionary.Add(key, value);
        }

        /// <summary>
        /// Adds the elements of the specified dictionary to the end of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements should be added at the end of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public void AddRange(IDictionary<TKey, TValue> dictionary)
        {
            this.InnerDictionary.AddRange(dictionary);
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        public bool Remove(TKey key)
        {
            return this.InnerDictionary.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value"> When this method returns, contains the value associated with the specified 
        /// key, if the key is found; otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">Key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.InnerDictionary.TryGetValue(key, out value);
        }


        /// <summary>
        /// Removes all keys and values from the  <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        public void Clear()
        {
            this.InnerDictionary.Clear();
        }

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            this.InnerDictionary.OnDeserialization(sender);
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.InnerDictionary.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the dictionary.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        #endregion |   Public Methods   |

        //#region |   Protected Overrided Methods   |

        //protected override void OnSetOwner()
        //{
        //    base.OnSetOwner();

        //    foreach (KeyValuePair<TKey, TValue> model in this)
        //        model.Value.Owner = this.Owner;
        //}

        //#endregion |   Protected Overrided Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual void InnerDictionary_BeforeAdd(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            this.BeforeAdd?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_AfterAdd(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            this.AfterAdd?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_BeforeRemove(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            this.BeforeRemove?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_AfterRemove(object sender, DictionaryActionEventArgs<TKey, TValue> e)
        {
            this.AfterRemove?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_BeforeSet(object sender, DictionaryActionOldValueEventArgs<TKey, TValue> e)
        {
            this.BeforeSet?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_AfterSet(object sender, DictionaryActionOldValueEventArgs<TKey, TValue> e)
        {
            this.AfterSet?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_BeforeClear(object sender, EventArgs e)
        {
            this.BeforeClear?.Invoke(this, e);
        }

        protected virtual void InnerDictionary_AfterClear(object sender, OldCountEventArgs e)
        {
            this.AfterClear?.Invoke(this, e);
        }

        #endregion |   Protected Virtual Methods   |

        #region |   Private Methods   |

        private void Initialize()
        {
            this.InnerDictionary.BeforeAdd += new DictionaryActionEventHandler<TKey, TValue>(InnerDictionary_BeforeAdd);
            this.InnerDictionary.AfterAdd += new DictionaryActionEventHandler<TKey, TValue>(InnerDictionary_AfterAdd);
            this.InnerDictionary.BeforeSet += new DictionaryActionOldValueEventHandler<TKey, TValue>(InnerDictionary_BeforeSet);
            this.InnerDictionary.AfterSet += new DictionaryActionOldValueEventHandler<TKey, TValue>(InnerDictionary_AfterSet);
            this.InnerDictionary.BeforeRemove += new DictionaryActionEventHandler<TKey, TValue>(InnerDictionary_BeforeRemove);
            this.InnerDictionary.AfterRemove += new DictionaryActionEventHandler<TKey, TValue>(InnerDictionary_AfterRemove);
            this.InnerDictionary.BeforeClear += new EventHandler(InnerDictionary_BeforeClear);
            this.InnerDictionary.AfterClear += new OldCountEventHandler(InnerDictionary_AfterClear);
        }

        #endregion |   Private Methods   |

        #region |   Interface ICollection<KeyValuePair<TKey, TValue>>   |

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.InnerDictionary.Contains(item);
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
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
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
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
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (this.InnerDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
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
            set { this[(TKey)key] = (TValue)value; }
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
            this.Add((TKey)key, (TValue)value);
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
            get { return this.InnerDictionary.CustomKeys; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Values
        {
            get { return this.InnerDictionary.CustomValues; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.</returns>
        bool IDictionary.IsFixedSize
        {
            get { return (this.InnerDictionary as IDictionary).IsFixedSize; }
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (this.InnerDictionary as IDictionary).GetEnumerator();
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
            (this.InnerDictionary as ICollection).CopyTo(array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return (this.InnerDictionary as ICollection).IsSynchronized; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return (this.InnerDictionary as ICollection).SyncRoot; }
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
            return this.InnerDictionary.GetEnumerator();
        }

        #endregion |   Interface IEnumerable   |
    }
}
