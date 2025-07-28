using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Simple.Collections
{
	public abstract class SimpleCollectionBase<T> : IEnumerable<T>, IEnumerable
	{
		#region |   Private Members   |

		protected readonly object lockObject = new object();
		private ReadOnlyCollection<T> readOnlyCollection = null;
		private bool isReadOnly = false;
		Hashtable customCollectionsByCustomType = null;

		#endregion |   Private Members   |

		#region |   Constructor(s) and Initialization   |

		/// <summary>
		/// Initializes a new instance of the <see cref="Simple.Collections.SimpleCollectionBase&lt;T&gt;"/> class that is empty and has the default initial capacity.
		/// </summary>
		public SimpleCollectionBase()
		{
			this.InnerCollection = new List<T>();
			this.Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <<see cref="Simple.Collections.SimpleCollectionBase&lt;T&gt;"/> class that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
		public SimpleCollectionBase(int capacity)
		{
			this.InnerCollection = new List<T>(capacity);
			this.Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Simple.Collections.SimpleCollectionBase&lt;T&gt;"/> class that wrap around the specified generic list.
		/// </summary>
		/// <param name="collectionToWrap">The <see cref="T:System.Collections.Generic.ICollection`1"></see> to wrap.</param>
		/// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
		public SimpleCollectionBase(ICollection<T> collectionToWrap)
		{
			if (collectionToWrap != null)
			{
				this.InnerCollection = collectionToWrap; //  new CustomCollection<T>(collectionToWrap); // new CustomList<T>(collectionToWrap);
				this.Initialize();
				this.IsReadOnly = true;
			}
			else
			{
				throw new ArgumentNullException("collection is null.");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleListBase&lt;T&gt;"/> class that wrap around the specified non-generic list.
		/// </summary>
		/// <param name="collectionToWrap">The <see cref="T:System.Collections.IList"></see> to wrap.</param>
		/// <param name="isReadOnly">Indicating if collection will act as read-only.</param>
		/// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
		public SimpleCollectionBase(ICollection collectionToWrap)
		{
			if (collectionToWrap != null)
			{
				this.InnerCollection = new CustomCollection<T>(collectionToWrap);
				this.Initialize();
				this.IsReadOnly = true;
			}
			else
			{
				throw new ArgumentNullException("collection is null.");
			}
		}

		#endregion |   Constructor(s) and Initialization   |

		#region |   Private Properties   |

		private Hashtable CustomCollectionsByCustomType
		{
			get
			{
				if (this.customCollectionsByCustomType == null)
					this.customCollectionsByCustomType = new Hashtable();

				return this.customCollectionsByCustomType;
			}
		}

		#endregion |   Private Properties   |

		#region |   Public Properties   |

		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the collection.</returns>
		public int Count
		{
			get { return this.InnerCollection.Count; }
		}

		#endregion |   Public Properties   |

		#region |   Protected Properties   |

		protected ICollection<T> InnerCollection { get; set; }
		protected MatchItemValue<T> MatchItemValueDelegate { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="Simple.Collections.SimpleCollectionBase&lt;T&gt;"/> is read-only.
		/// </summary>
		protected bool IsReadOnly
		{
			get { return this.isReadOnly; }
			set { this.isReadOnly = value; }
		}

		#endregion |   Protected Properties   |

		#region |   Public Methods   |

		/// <summary>
		/// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>
		/// default(T) if no element passes the test specified by predicate; 
		/// otherwise, the first element in collection that passes the test specified by predicate.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">predicate is null.</exception>
		public T FindFirst(Func<T, bool> predicate)
		{
			lock (this.lockObject)
			{
				if (predicate == null)
					throw new ArgumentNullException();

				for (int i = 0; i < this.Count; i++)
				{
					T item = this.ElementAt(i);

					if (predicate(item))
						return item;
				}

				return default(T);
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return this.InnerCollection.GetEnumerator();
		}

		#endregion |   Public Methods   |

		#region |   Protected Abstract Methods   |

		protected abstract ReadOnlyCollection<T> CreateReadOnlyCollection();
		protected abstract CustomCollection<U> CreateCustomCollection<U>();

		#endregion |   Protected Abstract Methods   |

		#region |   Protected Methods   |

		/// <summary>
		/// Returns a read-only <see cref="SimpleCollectionBase&lt;T&gt;"/> wrapper for the current collection.
		/// </summary>
		/// <returns>A <see cref="SimpleCollectionBase&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IList`1"></see>.</returns>
		protected ReadOnlyCollection<T> AsReadOnly()
		{
			if (this.readOnlyCollection == null)
			{
				lock (this.lockObject)
				{
					this.readOnlyCollection = this.CreateReadOnlyCollection();
				}
			}

			return this.readOnlyCollection;
		}

		/// <summary>
		/// Returns a <see cref="CustomCollection&lt;U&gt;"/> type collection object that is wrapper for the current T collection type.
		/// </summary>
		/// <typeparam name="U">Type parameter U to convert from type T.</typeparam>
		/// <returns>Customized colection with values converted from T to U type.</returns>
		protected CustomCollection<U> AsCustom<U>()
		{
			CustomCollection<U> customCollection = null;

			customCollection = this.CustomCollectionsByCustomType[typeof(U)] as CustomCollection<U>;

			if (customCollection == null)
			{
				lock (this.lockObject)
				{
					customCollection = this.CreateCustomCollection<U>();

					if (!this.CustomCollectionsByCustomType.ContainsKey(typeof(U)))
						this.CustomCollectionsByCustomType.Add(typeof(U), customCollection);
				}
			}

			return customCollection;
		}

		/// <summary>
		/// Gets or sets the total number of elements the internal data structure can hold without resizing.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException"><see cref="T:System.Collections.Generic.List`1.Capacity"></see> is set to a value that is less than <see cref="SimpleList&lt;T&gt;.Count"/>.</exception>
		/// <exception cref="System.OutOfMemoryException">the property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
		protected int Capacity
		{
			get
			{
				if (this.InnerCollection is List<T>)
				{
					lock (this.lockObject)
					{
						return (this.InnerCollection as List<T>).Capacity;
					}
				}
				else
				{
					lock (this.lockObject)
					{
						return this.Count;
					}
				}
			}

			set
			{
				if (this.InnerCollection is List<T>)
				{
					lock (this.lockObject)
					{
						(this.InnerCollection as List<T>).Capacity = value;
					}
				}
			}
		}

		protected virtual T ListGet(int index)
		{
			lock (this.lockObject)
			{
				return this.InnerCollection.ElementAt(index);
			}
		}

		protected int ListAdd(T value)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("collection is read-only.");

			return this.InternalListAdd(value);
		}

		protected void ListAddRange(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection is null.");

			this.InternalListAddRange(collection);
		}

		protected bool ListRemove(T item)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("collection is read-only.");

			return this.InternalListRemove(item);
		}

		protected bool ListRemoveAt(int index)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("collection is read-only.");

			return this.InternalListRemoveAt(index);
		}

		protected void ListClear()
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("collection is read-only.");

			this.InternalListClear();
		}

		protected virtual int InternalListAdd(T value)
		{
			lock (this.lockObject)
			{
				int index = this.Count;

				this.OnBeforeInsert(index, value);

				this.InnerCollection.Add(value);

				this.OnAfterInsert(index, value);
				this.OnCountChange(this.Count, this.Count - 1);

				return index;
			}
		}

		protected void InternalListAddRange(IEnumerable<T> collection)
		{
			lock (this.lockObject)
			{
				foreach (T item in collection)
					this.ListAdd(item);
			}
		}

		protected bool InternalListRemove(T item)
		{
			lock (this.lockObject)
			{
				bool result = false;
				T itemToRemove = this.MatchItemValueDelegate(item);

				if (itemToRemove != null)
				{
					int index = this.GetIndexOf(itemToRemove);

					result = this.ListRemoveAt(index);
				}

				return result;
			}
		}

		protected virtual bool InternalListRemoveAt(int index)
		{
			lock (this.lockObject)
			{
				bool itemExists = index >= 0 && index < this.InnerCollection.Count;

				if (itemExists)
				{
					T value = this.ListGet(index);

					this.OnBeforeRemove(index, value);
					this.DoRemoveAt(index, value);
					this.OnAfterRemove(index, value);
					this.OnCountChange(this.Count, this.Count + 1);
				}

				return itemExists;
			}
		}

		protected virtual void InternalListClear()
		{
			lock (this.lockObject)
			{
				int oldCount = this.Count;

				this.OnBeforeClear();
				this.InnerCollection.Clear();
				this.OnAfterClear();
				this.OnCountChange(this.Count, oldCount);
			}
		}

		protected virtual T MatchItemValue(T value)
		{
			lock (this.lockObject)
			{
				T result = this.InnerCollection.Contains(value) ? value : default(T);
				return result;
			}
		}

		protected virtual int GetIndexOf(T item)
		{
			lock (this.lockObject)
			{
				for (int index = 0; index < this.Count; index++)
				{
					T element = this.ListGet(index);

					if (Comparison.IsEqual(element, item))
						return index;
				}
			}

			return -1;
		}

		protected virtual void DoRemoveAt(int index, T value)
		{
			lock (this.lockObject)
			{
				this.InnerCollection.Remove(value);
			}
		}

		#endregion |   Protected Methods   |

		#region |   Protected Virtual Methods   |

		protected virtual void OnBeforeSet(int index, T value, T oldValue)
		{
		}

		protected virtual void OnAfterSet(int index, T value, T oldValue)
		{
		}

		protected virtual void OnBeforeInsert(int index, T value)
		{
		}

		protected virtual void OnAfterInsert(int index, T value)
		{
		}

		protected virtual void OnBeforeRemove(int index, T value)
		{
		}

		protected virtual void OnAfterRemove(int index, T value)
		{
		}

		protected virtual void OnBeforeClear()
		{
		}

		protected virtual void OnAfterClear()
		{
		}

		protected virtual void OnCountChange(int count, int oldCount)
		{
		}

		#endregion |   Protected Virtual Methods   |

		#region |   Private Methods   |

		private void Initialize()
		{
			this.MatchItemValueDelegate = this.MatchItemValue;
		}

		#endregion |   Private Methods   |

		#region |   IEnumerable Interface   |

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.InnerCollection.GetEnumerator();
		}

		#endregion |   IEnumerable Interface   |
	}
}
