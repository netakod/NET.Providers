using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Simple.Collections
{
	public class NullableDictionary<TKey, TValue> : DictionaryBase<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		private Dictionary<NullObject<TKey>, TValue> dictionary = null;

		#region |   Constructor(s) and Initialization   |

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
		/// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
		/// </summary>
		public NullableDictionary()
		{
			this.dictionary = new Dictionary<NullObject<TKey>, TValue>();
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> 
		///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> can contain.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
		public NullableDictionary(int capacity)
		{
			this.dictionary = new Dictionary<NullObject<TKey>, TValue>(capacity);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
		/// class whose elements are copied from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
		/// and uses the default equality comparer for the key type.
		/// </summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
		/// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
		public NullableDictionary(IDictionary<TKey, TValue> dictionary)
			: this()
		{
			foreach (var item in dictionary)
				this.dictionary.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/>
		/// class that is empty, has the default initial capacity, and uses the specified
		/// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
		/// </summary>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
		/// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
		public NullableDictionary(IEqualityComparer<NullObject<TKey>> comparer)
		{
			this.dictionary = new Dictionary<NullObject<TKey>, TValue>(comparer);
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
		public NullableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<NullObject<TKey>> comparer)
			: this(comparer)
		{
			foreach (var item in dictionary)
				this.dictionary.Add(item.Key, item.Value);
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
		public NullableDictionary(int capacity, IEqualityComparer<NullObject<TKey>> comparer)
		{
			this.dictionary = new Dictionary<NullObject<TKey>, TValue>(capacity, comparer);
		}

		#endregion |   Constructor(s) and Initialization   |


		#region |   Abstract Members Implementation   |

		public override int Count => this.dictionary.Count();

		public override void Add(TKey key, TValue value)
		{
			this.dictionary.Add(key, value);
		}

		public override bool Remove(TKey key)
		{
			return this.dictionary.Remove(key);
		}

		public override bool TryGetValue(TKey key, out TValue value)
		{
			return this.dictionary.TryGetValue(key, out value);
		}

		public override void Clear()
		{
			this.dictionary.Clear();
		}

		public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var item in this.dictionary)
				yield return new KeyValuePair<TKey, TValue>(item.Key.Item, item.Value);
		}

		#endregion |   Abstract Members Implementation   |

		public CustomDictionary<TKey, UValue> AsCustom<UValue>()
		{
			return new CustomDictionary<TKey, UValue>(this.dictionary);
		}
	}

	public struct NullObject<T>
	{
		[DefaultValue(true)]
		private bool isNull;// default property initializers are not supported for structs

		private NullObject(T item, bool isnull) : this()
		{
			this.isNull = isnull;
			this.Item = item;
		}

		public NullObject(T item) : this(item, item == null)
		{
		}

		public static NullObject<T> Null()
		{
			return new NullObject<T>();
		}

		public T Item { get; private set; }

		public bool IsNull()
		{
			return this.isNull;
		}

		public static implicit operator T(NullObject<T> nullObject)
		{
			return nullObject.Item;
		}

		public static implicit operator NullObject<T>(T item)
		{
			return new NullObject<T>(item);
		}

		public override string ToString()
		{
			return (Item != null) ? Item.ToString() : "null";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return this.IsNull();

			if (!(obj is NullObject<T>))
				return false;

			var no = (NullObject<T>)obj;

			if (this.IsNull())
				return no.IsNull();

			if (no.IsNull())
				return false;

			return this.Item.Equals(no.Item);
		}

		public override int GetHashCode()
		{
			if (this.isNull)
				return 0;

			var result = Item.GetHashCode();

			if (result >= 0)
				result++;

			return result;
		}
	}
}