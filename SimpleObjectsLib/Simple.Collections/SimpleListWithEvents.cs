using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Simple.Collections
{
	/// <summary>
	/// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.
	/// To addition to the <see cref="T:System.Collections.Generic.IList`1"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method, AsCustom method custom type casting
	/// and collection action events.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the collection.</typeparam>
	[Serializable]
    public class SimpleListWithEvents<T> : SimpleListWithEventsBase<T>, IList<T>, ICollectionEvents<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable

    {
		#region |   Constructor(s) and Initialization   |

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleList{T}"/> class that is empty and has the default initial capacity.
		/// </summary>
		public SimpleListWithEvents()
        {
        }

		/// <summary>
		///    Initializes a new instance of the <see cref="SimpleList{T}"/> class that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
		public SimpleListWithEvents(int capacity)
            : base(capacity)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleList{T}"/> class that wrap around the specified generic list.
		/// </summary>
		/// <param name="collectionToWrap">The <see cref="System.Collections.Generic.IList{T}"></see> to wrap.</param>
		/// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
		public SimpleListWithEvents(IList<T> collectionToWrap)
            : base(collectionToWrap)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleList{T}"/> class that wrap around the specified non-generic list.
		/// </summary>
		/// <param name="collectionToWrap">The <see cref="System.Collections.IList"></see> to wrap.</param>
		/// <exception cref="System.ArgumentNullException">collectionToWrap is null.</exception>
		public SimpleListWithEvents(IList collectionToWrap)
            : base(collectionToWrap)
        {
        }

		#endregion |   Constructor(s) and Initialization   |

		#region |   Public Properties   |

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.  -or- index is equal to or greater than <see cref="CustomList{T}"/>.Count.</exception>
		/// <exception cref="System.NotSupportedException">the property is set and the <see cref="System.Collections.Generic.IList{T}"></see> is read-only.</exception>
		public T this[int index]
        {
            get { return this.ListGet(index); }
            set { this.ListSet(index, value); }
        }

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="Simple.Collections.SimpleList{T}"/> is read-only.
		/// </summary>
		public new bool IsReadOnly
        {
            get { return base.IsReadOnly; }
            set { base.IsReadOnly = value; }
        }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException"><see cref="T:System.Collections.Generic.List`1.Capacity"></see> is set to a value that is less than <see cref="SimpleList&lt;T&gt;.Count"/>.</exception>
        /// <exception cref="System.OutOfMemoryException">the property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public new int Capacity
        {
            get { return base.Capacity; }
            set { base.Capacity = value; }
        }

        #endregion |   Public Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="SimpleList&lt;T&gt;"/> wrapper for the current collection.
        /// </summary>
        /// <returns>A <see cref="SimpleList&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IList`1"></see>.</returns>
        public new ReadOnlyList<T> AsReadOnly()
        {
            return base.AsReadOnly() as ReadOnlyList<T>;
        }

        /// <summary>
        /// Return custom collection that wrap current colection and cast elements to U type parameter. T elements mast be castable to U type.
        /// </summary>
        /// <typeparam name="U">Customized type. </typeparam>
        /// <returns></returns>
        public new CustomList<U> AsCustom<U>()
        {
            return base.AsCustom<U>() as CustomList<U>;
        }
        
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IList`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.IList`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            lock (this.lockObject)
            {
                return this.InnerList.Contains(item);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.IList`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.IList`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.IList`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this.lockObject)
            {
                this.InnerList.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="CustomList&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>. The item can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="SimpleList&lt;T&gt;"/>, if found; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            lock (this.lockObject)
            {
                return this.InnerList.IndexOf(item);
            }
        }

        #endregion |   Public Methods   |

        #region |   Protected Overrided Abstract Methods   |

        protected override ReadOnlyCollection<T> CreateReadOnlyCollection()
        {
            return new ReadOnlyList<T>(this);
        }

        protected override CustomCollection<U> CreateCustomCollection<U>()
        {
            return new CustomList<U>(this);
        }

        #endregion |   Protected Overrided Abstract Methods   |

        #region |   IList<T> Interface   |

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        public void Insert(int index, T item)
        {
            this.ListInsert(index, item);
        }

        /// <summary>
        /// Removes cannot be performed on the read-only collection.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        public void RemoveAt(int index)
        {
            this.ListRemoveAt(index);
        }

        #endregion |   IList<T> Interface   |

        #region |   ICollection<T> Interface   |

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public void Add(T item)
        {
            this.ListInsert(this.Count, item);
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="SimpleList&lt;T&gt;"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="SimpleList&lt;T&gt;"/>. 
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <exception cref="System.ArgumentNullException">collection is null.</exception>
        public void AddRange(IEnumerable<T> collection)
        {
            this.ListAddRange(collection);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. 
        ///   This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public bool Remove(T item)
        {
            return this.ListRemove(item); 
        }

        /// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
		/// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Clear()
        {
            this.ListClear();
        }

        #endregion |   ICollection<T> Interface   |

        #region |   IList Interface   |

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="System.Collections.IList"></see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList"></see> has a fixed size.</exception>
        object IList.this[int index]
        {
            get { return this.ListGet(index); }
            set { this.ListSet(index, (T)value); }
        }

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to add to the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList"></see> has a fixed size.</exception>
        int IList.Add(object value)
        {
            return this.ListAdd((T)value);
        }

        /// <summary>
        /// Removes all items from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.</exception>
        void IList.Clear()
        {
            this.ListClear();
        }

        /// <summary>Determines whether the <see cref="System.Collections.IList"></see> contains a specific value.</summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>true if the System.Object is found in the <see cref="System.Collections.IList"></see>; otherwise, false.</returns>
        bool IList.Contains(object value)
        {
            lock (this.lockObject)
            {
                return this.InnerList.Contains((T)value);
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            lock (this.lockObject)
            {
                return this.InnerList.IndexOf((T)value);
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="System.Collections.Generic.IList"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="item">The System.Object to insert into the <see cref="System.Collections.IList"></see>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="System.Collections.IList"></see>.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        /// <exception cref="System.NullReferenceException">value is null reference in the <see cref="System.Collections.IList"></see>.</exception>
        void IList.Insert(int index, object item)
        {
            this.ListInsert(index, (T)item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="item">The System.Object to remove from the <see cref="System.Collections.IList"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.Remove(object item)
        {
            this.ListRemove((T)item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.RemoveAt(int index)
        {
            this.ListRemoveAt(index);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.IList"></see> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="System.Collections.IList"></see> has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize
        {
            get { return (this.InnerList as IList).IsFixedSize; }
        }

        #endregion |   IList Interface   |

        #region |   ICollection Interface   |

        /// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        /// <returns>true if access to the System.Collections.ICollection is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return (this.InnerList as IList).IsSynchronized; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return (this.InnerList as IList).SyncRoot; }
        }

        /// <summary>
        /// Copies the elements of the <see cref="System.Collections.ICollection"></see> to an <see cref="System.Array"></see>, starting at a particular <see cref="System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="System.Array"></see> that is the destination of the elements copied from <see cref="System.Collections.ICollection"></see>. The <see cref="System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="System.ArgumentNullException">array is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than zero.</exception>
        /// <exception cref="System.ArgumentException">index is less than zero.</exception>
        /// <exception cref="System.ArgumentException">The type of the source <see cref="System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            lock (this.lockObject)
            {
                (this.InnerList as IList).CopyTo(array, index);
            }
        }

        #endregion |   ICollection Interface   |
    }

    #region |   Interfaces   |

    //public interface IListWithEvents<T> : IList<T>
    //{
    //    event CollectionActionEventHandler<T> BeforeInsert;
    //    event CollectionActionEventHandler<T> AfterInsert;
    //    event CollectionActionEventHandler<T> BeforeRemove;
    //    event CollectionActionEventHandler<T> AfterRemove;
    //    event CollectionActionOldValueEventHandler<T> BeforeSet;
    //    event CollectionActionOldValueEventHandler<T> AfterSet;
    //    event EventHandler BeforeClear;
    //    event EventHandler AfterClear;
    //    event CountChangeEventHandler CountChange;
    //}

    #endregion |   Interfaces   |
}
