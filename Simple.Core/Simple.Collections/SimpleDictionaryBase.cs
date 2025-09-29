using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Simple.Collections
{
    public abstract class SimpleDictionaryBase<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        #region |   Private Members   |

        private CustomCollection<TKey> customKeys;
        private CustomCollection<TValue> customValues;
        private ReadOnlyDictionary<TKey, TValue> readOnlyDictionary = null;
        private Hashtable customDictionariesByCustomValueType = null;
        private Hashtable customDictionariesByCustomKeyType = null;
        private bool isReadOnly = false;

        private object lockObject = new object();
        private object lockCustomObject = new object();

        #endregion |   Private Members   |

        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SimpleDictionaryBase()
	    {
            this.InnerDictionary = new Dictionary<TKey, TValue>();
	    }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryBase(int capacity)
        {
            this.InnerDictionary = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/>
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public SimpleDictionaryBase(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                this.InnerDictionary = dictionary;
            }
            else
            {
                throw new System.ArgumentNullException("dictionary is null.");
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/>
        /// class that wrap around the specified non-generic dictionary.
        /// </summary>
        /// <param name="dictionaryToWrap">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public SimpleDictionaryBase(IDictionary dictionaryToWrap)
        {
            if (dictionaryToWrap != null)
            {
                this.InnerDictionary = new CustomDictionary<TKey, TValue>(dictionaryToWrap);
            }
            else
            {
                throw new System.ArgumentNullException("dictionary is null.");
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
        public SimpleDictionaryBase(IEqualityComparer<TKey> comparer)
        {
            this.InnerDictionary = new Dictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> 
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> whose elements are
        /// copied to the new <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        /// <exception cref="T:System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
        public SimpleDictionaryBase(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            this.InnerDictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2"></see> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryBase(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.InnerDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> 
        ///// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        ///// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        ///// </summary>
        ///// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.IEnumerable`1"></see> whose elements are
        ///// copied to the new <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param></param>
        //public SimpleDictionaryBase(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePair)
        //{
        //    this.InnerDictionary = new Dictionary<TKey, TValue>();

        //    foreach (var item in keyValuePair)
        //        this.InnerDictionary.Add(item);
        //}
        
        #endregion |   Constructor(s) and Initialization   |

        #region |   Private Properties   |

        private Hashtable CustomDictionariesByCustomValueType
        {
            get
            {
                if (this.customDictionariesByCustomValueType == null)
                    this.customDictionariesByCustomValueType = new Hashtable();

                return this.customDictionariesByCustomValueType;
            }
        }

        private Hashtable CustomDictionariesByCustomKeyType
        {
            get
            {
                if (this.customDictionariesByCustomKeyType == null)
                    this.customDictionariesByCustomKeyType = new Hashtable();

                return this.customDictionariesByCustomKeyType;
            }
        }

        #endregion |   Private Properties   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the dictionary.</returns>
        public int Count
        {
            get 
            {
                lock (lockObject)
                {
                    return this.InnerDictionary.Count;
                }
            }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        IDictionary<TKey, TValue> idict = null;
        string ro = String.Empty;

        protected IDictionary<TKey, TValue> InnerDictionary 
        { get => this.idict; 
            set
			{
                this.idict = value;

                if (value.IsReadOnly)
                    this.ro = "RO";
			}
        }

        /// <summary>
        /// Gets a read-only <see cref="SimpleCollection&lt;TKey&gt;"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see> that can be customized.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="SimpleCollection&lt;TKey&gt;"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected CustomCollection<TKey> CustomKeys
        {
            get
            {
                if (this.customKeys == null)
                    this.customKeys = new CustomCollection<TKey>(this.InnerDictionary.Keys);

                return this.customKeys;
            }
        }

        /// <summary>
        /// Gets an read-only <see cref="SimpleCollection&lt;TKey&gt;"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see> that can be customized.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="SimpleCollection&lt;TKey&gt;"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected CustomCollection<TValue> CustomValues
        {
            get
            {
                if (this.customValues == null)
                    this.customValues = new CustomCollection<TValue>(this.InnerDictionary.Values);

                return this.customValues;
            }
        }

        /// <summary>
        /// Gets a read-only <see cref="System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected ICollection<TKey> Keys
        {
            get { return this.CustomKeys; }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.Generic.ICollection`1"/> containing the custom values in the <see cref="SimpleDictionaryBase&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        protected ICollection<TValue> Values
        {
            get { return this.CustomValues; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> is read-only.
        /// </summary>
        protected bool IsReadOnly
        {
            get { return this.isReadOnly || (this.InnerDictionary != null && this.InnerDictionary.IsReadOnly); }
            set { this.isReadOnly = value; }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

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

        #region |   Protected Abstract Methods   |

        protected abstract CustomDictionary<TKey, UValue> CreateCustomDictionaryByValueType<UValue>();
        protected abstract CustomDictionary<UKey, UValue> CreateCustomDictionaryByKeyAndValueType<UKey, UValue>();

        #endregion |   Protected Abstract Methods   |

        #region |   Protected Methods   |

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> wrapper for the current dictionary.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> that acts as a read-only wrapper around the current <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</returns>
        protected ReadOnlyDictionary<TKey, TValue> AsReadOnly()
        {
            if (this.readOnlyDictionary == null)
                this.readOnlyDictionary = new ReadOnlyDictionary<TKey, TValue>(this.InnerDictionary);

            return this.readOnlyDictionary;
        }

        /// <summary>
        /// Returns a <see cref="CustomDictionary&lt;TKey, TValue&gt;"/> type dictionary object that is wrapper for the current TValue dictionary value type.
        /// </summary>
        /// <typeparam name="UValue">Type parameter UValue to convert values from TValue type.</typeparam>
        /// <returns>Customized dictionary with values converted from TValue to UValue type.</returns>
        protected CustomDictionary<TKey, UValue> AsCustom<UValue>()
        {
            return this.GetCustomDictionaryByValueType<UValue>();
        }

        protected CustomDictionary<UKey, UValue> AsCustom<UKey, UValue>()
        {
            return this.GetCustomDictionaryByKeyAndValueType<UKey, UValue>();
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        protected bool ContainsKey(TKey key)
        {
            lock (lockObject)
            {
                return this.InnerDictionary.Keys.Contains(key);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>. The value can be null for reference types.
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the value; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">value is null.</exception>
        protected bool ContainsValue(TValue value)
        {
            lock (lockObject)
            {
                return this.InnerDictionary.Values.Contains(value);
            }
        }

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        protected void OnDeserialization(object sender)
        {
            lock (lockObject)
            {
                IDeserializationCallback callback = this.InnerDictionary as IDeserializationCallback;
                callback.OnDeserialization(sender);
            }
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        protected void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (lockObject)
            {
                ISerializable serializable = this.InnerDictionary as ISerializable;
                serializable.GetObjectData(info, context);
            }
        }

        protected TValue DictionaryGet(TKey key)
        {
            lock (lockObject)
            {
                TValue value;
                this.InnerDictionary.TryGetValue(key, out value);

                return value;
            }
        }

        protected void DictionarySet(TKey key, TValue value)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("dictionary is read-only");

            this.InternalDictionarySet(key, value);
        }

        protected void DictionaryAdd(TKey key, TValue value)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("dictionary is read-only");

            this.InternalDictionaryAdd(key, value);
        }

        protected void DictionaryAddRange(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new NotSupportedException("dictionary is null");

            this.InternalDictionaryAddRange(dictionary);
        }

        protected bool DictionaryRemove(TKey key)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("dictionary is read-only");

            return this.InternalDictionaryRemove(key);
        }


        protected void DictionaryClear()
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("dictionary is read-only");

            this.InternalDictionaryClear();
        }

		#endregion |   Protected Methods   |

		#region |   Protected Virtual Methods   |

		protected virtual void InternalDictionaryAdd(TKey key, TValue value)
		{
			this.InternalDictionaryAdd(key, value, callOnCountChange: true);
		}

		protected virtual void InternalDictionaryAddRange(IDictionary<TKey, TValue> dictionary)
		{
			lock (lockObject)
			{
				int oldCount = this.Count;

				foreach (var entity in dictionary)
					this.InternalDictionaryAdd(entity.Key, entity.Value, callOnCountChange: false);

				if (this.Count != oldCount)
					this.OnCountChange(this.Count, oldCount);
			}
		}

		protected virtual bool InternalDictionaryRemove(TKey key)
		{
			bool isRemoved = false;

			lock (lockObject)
			{
				if (this.InnerDictionary.ContainsKey(key))
				{
					TValue value = this.InnerDictionary[key];

					this.OnBeforeRemove(key, value);
					this.InnerDictionary.Remove(key);
					this.OnAfterRemove(key, value);
					this.OnCountChange(this.Count, oldCount: this.InnerDictionary.Count + 1);

					isRemoved = true;
				}
			}

			return isRemoved;
		}

		protected virtual void InternalDictionarySet(TKey key, TValue value)
		{
			lock (lockObject)
			{
				TValue oldValue = this.DictionaryGet(key);

				this.OnBeforeSet(key, value, oldValue);
				this.InnerDictionary[key] = value;
				this.OnAfterSet(key, value, oldValue);
			}
		}

		protected virtual void InternalDictionaryClear()
		{
			lock (lockObject)
			{
				if (this.Count > 0)
				{
					int oldCount = this.Count;

					this.OnBeforeClear();
					this.InnerDictionary.Clear();
					this.OnAfterClear(oldCount);
					this.OnCountChange(this.Count, oldCount);
				}
			}
		}

		protected virtual void OnBeforeAdd(TKey key, TValue value)
        {
        }

        protected virtual void OnAfterAdd(TKey key, TValue value)
        {
        }

        protected virtual void OnBeforeRemove(TKey key, TValue value)
        {
        }

        protected virtual void OnAfterRemove(TKey key, TValue value)
        {
        }

        protected virtual void OnBeforeSet(TKey key, TValue value, TValue oldValue)
        {
        }

        protected virtual void OnAfterSet(TKey key, TValue value, TValue oldValue)
        {
        }

        protected virtual void OnBeforeClear()
        {
        }

        protected virtual void OnAfterClear(int oldCount)
        {
        }

		protected virtual void OnCountChange(int count, int oldCount)
		{
		}

		#endregion |   Protected Virtual Methods   |

		#region |   Private Methods   |

		private void InternalDictionaryAdd(TKey key, TValue value, bool callOnCountChange)
		{
			lock (lockObject)
			{
				this.OnBeforeAdd(key, value);
				this.InnerDictionary.Add(key, value);
				this.OnAfterAdd(key, value);

				if (callOnCountChange)
					this.OnCountChange(this.Count, oldCount: this.InnerDictionary.Count - 1);
			}
		}

		private CustomDictionary<TKey, UValue> GetCustomDictionaryByValueType<UValue>()
        {
            lock (lockCustomObject)
            {
                CustomDictionary<TKey, UValue> value = this.CustomDictionariesByCustomValueType[typeof(UValue)] as CustomDictionary<TKey, UValue>;

                if (value == null)
                {
                    value = this.CreateCustomDictionaryByValueType<UValue>();
                    this.CustomDictionariesByCustomValueType.Add(typeof(UValue), value);
                }

                return value;
            }
        }

        private CustomDictionary<UKey, UValue> GetCustomDictionaryByKeyAndValueType<UKey, UValue>()
        {
            lock (lockCustomObject)
            {
                CustomDictionary<UKey, UValue> value = null;
                Hashtable customDictionariesHastableByKeyType = this.CustomDictionariesByCustomKeyType[typeof(UKey)] as Hashtable;

                if (customDictionariesHastableByKeyType == null)
                {
                    customDictionariesHastableByKeyType = new Hashtable();
                    this.CustomDictionariesByCustomKeyType.Add(typeof(UKey), customDictionariesHastableByKeyType);
                }

                value = customDictionariesHastableByKeyType[typeof(UValue)] as CustomDictionary<UKey, UValue>;

                if (value == null)
                {
                    value = this.CreateCustomDictionaryByKeyAndValueType<UKey, UValue>();
                    customDictionariesHastableByKeyType.Add(typeof(UValue), value);
                }

                return value;
            }
        }

        #endregion |   Private Methods   |

        #region |   IEnumerable Interface   |

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        #endregion |   IEnumerable Interface   |
    }

    #region |   Delegates   |

    public delegate void DictionaryActionEventHandler<TKey, TValue>(object sender, DictionaryActionEventArgs<TKey, TValue> e);
    public delegate void DictionaryActionOldValueEventHandler<TKey, TValue>(object sender, DictionaryActionOldValueEventArgs<TKey, TValue> e);
	public delegate void OldCountEventHandler(object sender, OldCountEventArgs e);

	#endregion |   Delegates   |

	#region |   EventArgs Classes   |

	public class DictionaryActionEventArgs<TKey, TValue> : EventArgs
    {
        public DictionaryActionEventArgs(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public TKey Key { get; private set; }
        public TValue Value { get; private set; }
    }

    public class DictionaryActionOldValueEventArgs<TKey, TValue> : DictionaryActionEventArgs<TKey, TValue>
    {
        public DictionaryActionOldValueEventArgs(TKey index, TValue value, TValue oldValue)
            : base(index, value)
        {
            this.OldValue = oldValue;
        }

        public TValue OldValue { get; private set; }
    }

    #endregion |   EventArgs Classes   |
}
