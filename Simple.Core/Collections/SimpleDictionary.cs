using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Simple;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and values. To addition to the <see cref="T:System.Collections.Generic.Dictionary`2"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method and 
    /// AsCustom method custom type casting.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    [XmlRoot("Dictionary")]
    public class SimpleDictionary<TKey, TValue> : SimpleDictionaryWithEventsBase<TKey, TValue>, IDictionaryWithEvents<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionaryEvents<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, IXmlSerializable, ICloneable
    {
        #region |   Private Members   |

        protected object lockObject = new object();

        #endregion |   Private Members   |
        
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SimpleDictionary()
	    {
	    }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionary(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary&lt;TKey, TValue&gt;"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public SimpleDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
        public SimpleDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
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
        public SimpleDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
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
        public SimpleDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

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
            get { return this.DictionaryGet(key); }
            set { this.DictionarySet(key, value); }
        }

        /// <summary>
        /// Gets a read-only <see cref="System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public new ICollection<TKey> Keys
        {
            get { return base.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.Generic.ICollection`1"/> containing the custom values in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public new ICollection<TValue> Values
        {
            get { return base.Values; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public new bool IsReadOnly
        {
            get { return base.IsReadOnly; }
            set { base.IsReadOnly = value; }
        }

        #endregion |   Public Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> wrapper for the current dictionary.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> that acts as a read-only wrapper around the current <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</returns>
        public new ReadOnlyDictionary<TKey, TValue> AsReadOnly()
        {
            return base.AsReadOnly();
        }

        /// <summary>
        /// Returns a <see cref="CustomDictionary&lt;TKey, TValue&gt;"/> type dictionary object that is wrapper for the current TValue dictionary value type.
        /// </summary>
        /// <typeparam name="UValue">Type parameter UValue to convert values from TValue type.</typeparam>
        /// <returns>Customized dictionary with values converted from TValue to UValue type.</returns>
        public new CustomDictionary<TKey, UValue> AsCustom<UValue>()
        {
            return base.AsCustom<UValue>();
        }

        /// <summary>
        /// Returns a <see cref="CustomDictionary&lt;TKey, TValue&gt;"/> type dictionary object that is wrapper for the current TValue dictionary value type.
        /// </summary>
        /// <typeparam name="UKey">Type parameter UKey to convert values from TKey type.</typeparam>
        /// <typeparam name="UValue">Type parameter UValue to convert values from TValue type.</typeparam>
        /// <returns>Customized dictionary with values converted from TKey and TValue to UKey and UValue type, respectively.</returns>
        public new CustomDictionary<UKey, UValue> AsCustom<UKey, UValue>()
        {
            return base.AsCustom<UKey, UValue>();
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public new bool ContainsKey(TKey key)
        {
            return base.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>. The value can be null for reference types.
        /// <returns>
        /// true if the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> contains an element with the value; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">value is null.</exception>
        public new bool ContainsValue(TValue value)
        {
            return base.ContainsValue(value);
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
            this.DictionaryAdd(key, value);
        }

        /// <summary>
        /// Adds the elements of the specified dictionary to the end of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements should be added at the end of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public void AddRange(IDictionary<TKey, TValue> dictionary)
        {
            this.DictionaryAddRange(dictionary);
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
            return this.DictionaryRemove(key);
        }

		// Summary:
		//     Removes the element at the specified index of the System.Collections.Generic.List<T>.
		//
		// Parameters:
		//   index:
		//     The zero-based index of the element to remove.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     index is less than 0.-or-index is equal to or greater than System.Collections.Generic.List<T>.Count.

		
		/// <summary>
		/// Removes the element at the specified index of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
		/// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
		public bool RemoveAt(int index)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("collection is read-only.");

			lock (this.lockObject)
			{
				bool itemExists = index >= 0 && index < this.Keys.Count;

				if (itemExists)
				{
					TKey key = this.Keys.ElementAt(index);
					
                    itemExists = this.Remove(key);
				}

				return itemExists;
			}
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
            lock (this.lockObject)
            {
                return this.InnerDictionary.TryGetValue(key, out value);
            }
        }


        /// <summary>
        /// Removes all keys and values from the  <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        public void Clear()
        {
            this.DictionaryClear();
        }

        public SimpleDictionary<TKey, TValue> Clone()
        {
            lock (this.lockObject)
            {
                IDictionary<TKey, TValue> dictionaryCopy = new Dictionary<TKey, TValue>(this);

				return new SimpleDictionary<TKey, TValue>(dictionaryCopy);
            }
        }

        #endregion |   Public Methods   |

        #region |   ISerializable Members   |

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion |   ISerializable Members   |

        #region |   IDeserializationCallback Members   |

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public new void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
        }

        #endregion |   IDeserializationCallback Members   |

        #region |   IXmlSerializable Members   |

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlHelper.ReadDictionaryFromXml<TKey, TValue>(this, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlHelper.WriteDictionaryToXml<TKey, TValue>(this, writer);
        }

        #endregion |   IXmlSerializable Members   |

        #region |   Protected Overrided Abstract Methods   |

        protected override CustomDictionary<TKey, UValue> CreateCustomDictionaryByValueType<UValue>()
        {
            return new CustomDictionary<TKey, UValue>(this); 
        }

        protected override CustomDictionary<UKey, UValue> CreateCustomDictionaryByKeyAndValueType<UKey, UValue>()
        {
            return new CustomDictionary<UKey, UValue>(this);
        }

        #endregion |   Protected Overrided Abstract Methods   |
        
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
            lock (this.lockObject)
            {
                return this.InnerDictionary.Contains(item);
            }
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
            lock (this.lockObject)
            {
                this.InnerDictionary.CopyTo(array, arrayIndex);
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
            lock (this.lockObject)
            {
                (this.InnerDictionary as ICollection).CopyTo(array, index);
            }
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

        #region |   Interface ICloneable   |

        object ICloneable.Clone()
		{
            return this.Clone();
		}

        #endregion |   Interface ICloneable   |
    }

    #region |   Interfaces   |

    public interface IDictionaryWithEvents<TKey, TValue> : IDictionary<TKey, TValue>
    {
        event DictionaryActionEventHandler<TKey, TValue> BeforeAdd;
        event DictionaryActionEventHandler<TKey, TValue> AfterAdd;
        event DictionaryActionEventHandler<TKey, TValue> BeforeRemove;
        event DictionaryActionEventHandler<TKey, TValue> AfterRemove;
        event DictionaryActionOldValueEventHandler<TKey, TValue> BeforeSet;
        event DictionaryActionOldValueEventHandler<TKey, TValue> AfterSet;
        event EventHandler BeforeClear;
        event OldCountEventHandler AfterClear;
    }

    #endregion |   Interfaces   |
}
