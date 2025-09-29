using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Threading;
using Simple;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a read-only wrapper around a generic IDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    [XmlRoot("Dictionary")]
    public class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> , IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, IXmlSerializable
    {
		private IDictionary<TKey, TValue> dictionary;
        private IDictionary idictionary;
		private object syncRoot;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="originalDictionary">The dictionary to wrap.</param>
		/// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
		public ReadOnlyDictionary(IDictionary<TKey, TValue> originalDictionary)
        {
            if (originalDictionary == null)
				throw new System.ArgumentNullException("Dictionary is null");

			this.dictionary = originalDictionary; // new Dictionary<TKey, TValue>(originalDictionary);
			this.idictionary = dictionary as IDictionary;
        }

        /// <summary>
        /// Add is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">Key is null.</exception>
        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets a read-only <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        /// <summary>
        /// Remove is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }

        /// <summary>
        /// Gets the <see cref="TValue"/> with the specified key. Set is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="TValue"/> value.</returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public TValue this[TKey key]
        {
            get { return this.dictionary[key]; }
            set { throw new NotSupportedException("Dictionary is read-only"); }
        }

        /// <summary>
        /// Add is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Clear is not allowed on a read-only Dictionary.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Remove is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
			IEnumerable<KeyValuePair<TKey, TValue>> enumerator = this.dictionary;
			return enumerator.GetEnumerator(); //return this.dictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
			return this.idictionary.GetEnumerator();
		}

        /// <summary>
        /// Add is not allowed on a read-only Dictionary.
        /// </summary>
        /// <param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.IDictionary"></see> object. </exception>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
        public void Add(object key, object value)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
        public bool Contains(object key)
        {
            return this.idictionary.Contains(key);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return idictionary.GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return this.idictionary.IsFixedSize; }
        }

		/// <summary>
		/// Gets an <see cref="ICollection></see> containing the keys of the <see cref="IDictionary{TKey, TValue}"></see>.
		/// </summary>
		ICollection IDictionary.Keys
        {
            get { return this.idictionary.Keys; }
        }

		/// <summary>
		/// Gets an <see cref="IEnumerable{TKey}"></see> containing the keys of the <see cref="IDictionary{TKey, TValue}"></see>.
		/// </summary>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get { return this.dictionary.Keys; }
		}

		/// <summary>
		/// Remove does not affect a read-only Dictionary
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
		/// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		public void Remove(object key)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Values
        {
            get { return this.idictionary.Values; }
        }

		/// <summary>
		/// Gets an <see cref="IEnumerable{TValue}"></see> containing the values of the <see cref="IDictionary{TKey, TValue}"></see>.
		/// </summary>
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get { return this.dictionary.Values; }
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified key. Set is not allowed on a read-only Dictionary.
		/// </summary>
		/// <value></value>
		public object this[object key]
        {
            get { return this.idictionary[key]; }
            set { throw new NotSupportedException("Dictionary is read-only"); }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"></see> to an <see cref="Array"></see>, starting at a particular <see cref="Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"></see> that is the destination of the elements copied from <see cref="ICollection"></see>. The <see cref="Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than zero. </exception>
        /// <exception cref="ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
        /// <exception cref="InvalidCastException">The type of the source <see cref="ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
        public void CopyTo(Array array, int index)
        {
            (this.dictionary as IList).CopyTo(array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return this.idictionary.IsSynchronized; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
        public object SyncRoot
        {
            get
			{
				//return this.idictionary.SyncRoot;

					if (this.syncRoot == null)
					{
						ICollection collection = this.dictionary as ICollection;

						if (collection != null)
						{
							this.syncRoot = collection.SyncRoot;
						}
						else
						{
							Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
						}
					}

					return this.syncRoot;

				}
		}

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            IDeserializationCallback callback = dictionary as IDeserializationCallback;
            callback.OnDeserialization(sender);
        }

        /// <summary>
        /// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ISerializable serializable = dictionary as ISerializable;
            serializable.GetObjectData(info, context);
        }

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
	}
}
