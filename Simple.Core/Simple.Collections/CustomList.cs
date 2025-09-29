using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Simple.Collections
{
	/// <summary>
	/// Represents a strongly typed custom list of objects that can be accessed by index. Original list can be <see cref="System.Collections.Generic.IList{T}"></see> interface and it is wrapped to T type.
	/// Note that any element of the input list must be custable to the T type. As standard list, it's provides methods to search, sort, and manipulate lists.
	/// To addition to the <see cref="System.Collections.Generic.IList{T}"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method and custom type casting.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the collection.</typeparam>
	[Serializable]
    public class CustomList<T> : CustomCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomList&lt;T&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.IList"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public CustomList(IList collectionToWrap)
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
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.  -or- index is equal to or greater than <see cref="CustomList&lt;T&gt;"/>.Count.</exception>
        /// <exception cref="System.NotSupportedException">the property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public T this[int index]
        {
            get { return this.CollectionGet(index); }
            set { this.CollectionSet(index, value); }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        /// <summary>
        /// Returns the <see cref="T:System.Collections.IList"></see> of the original list.
        /// </summary>
        protected new IList OriginalCollection
        {
            get { return base.OriginalCollection as IList; }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyList&lt;T&gt;"/> wrapper for the current collection.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyList&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IList`1"></see>.</returns>
        public new ReadOnlyList<T> AsReadOnly()
        {
            return base.AsReadOnly() as ReadOnlyList<T>;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="CustomList&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>. The item can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="CustomList&lt;T&gt;"/>, if found; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            return this.OriginalCollection.IndexOf(item);
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected T CollectionGet(int index)
        {
            return (T)this.OriginalCollection[index];
        }

        protected void CollectionSet(int index, T value)
        {
            T oldValue = this[index];
            this.OnSet(index, oldValue, value);
            this.OriginalCollection[index] = value;
        }

        protected void CollectionInsert(int index, T value)
        {
            this.OnInsert(index, value);
            this.OriginalCollection.Insert(index, value);
        }

        #endregion |   Protected Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual void OnSet(int index, T oldValue, T newValue)
        {
        }

        #endregion |   Protected Virtual Methods   |

        #region |   Protected Overrided Methods   |

        protected override object CreateReadOnlyCollection()
        {
            return new ReadOnlyList<T>(this as IList<T>);
        }

        protected override int GetIndexOf(T item)
        {
            return this.OriginalCollection.IndexOf(item);
        }

        protected override void DoRemoveAt(int index, T item)
        {
            this.OriginalCollection.RemoveAt(index);
        }

        #endregion |   Protected Overrided Methods   |

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
            this.CollectionInsert(index, item);
        }

        /// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.Generic.IList`1"></see>.
		/// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        public void RemoveAt(int index)
        {
            T item = this[index];
            this.CollectionRemoveAt(index, item);
        }

        #endregion |   IList<T> Interface   |

        //#region |   ICollection<T> Interface   |

        ///// <summary>
        ///// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///// </summary>
        ///// <typeparam name="T">The type of elements in the list.</typeparam> 
        ///// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        //public void Add(T item)
        //{
        //    this.ListInsert(this.Count, item);
        //}

        ///// <summary>
        ///// Removes all items from the System.Collections.Generic.ICollection<T>.
        ///// </summary>
        ///// <exception cref="System.NotSupportedException">The System.Collections.Generic.ICollection<T> is read-only.</exception>
        //public void Clear()
        //{
        //    this.CollectionClear();
        //}

        ///// <summary>
        ///// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///// </summary>
        ///// <typeparam name="T">The type of elements in the list.</typeparam> 
        ///// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///// <returns>true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. 
        /////   This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        ///// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        //public bool Remove(T item)
        //{
        //    int index = this.List.IndexOf(item);
        //    return this.ListRemoveAt(index); 
        //}

        //#endregion |   ICollection<T> Interface   |

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
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to add to the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList"></see> has a fixed size.</exception>
        int IList.Add(object value)
        {
            return this.CollectionAdd((T)value);
        }

        /// <summary>
        /// Removes all items from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.</exception>
        void IList.Clear()
        {
            this.CollectionClear();
        }

        /// <summary>Determines whether the <see cref="System.Collections.IList"></see> contains a specific value.</summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>true if the System.Object is found in the <see cref="System.Collections.IList"></see>; otherwise, false.</returns>
        bool IList.Contains(object value)
        {
            return this.OriginalCollection.Contains(value);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            return this.OriginalCollection.IndexOf(value);
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
            this.CollectionInsert(index, (T)item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="item">The System.Object to remove from the <see cref="System.Collections.IList"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.Remove(object item)
        {
            this.CollectionRemove((T)item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.RemoveAt(int index)
        {
            T item = this[index];
            this.CollectionRemoveAt(index, item);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.IList"></see> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="System.Collections.IList"></see> has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize
        {
            get { return this.OriginalCollection.IsFixedSize; }
        }

        #endregion |   IList Interface   |

        //#region |   ICollection Interface   |

        ///// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        ///// <returns>true if access to the System.Collections.ICollection is synchronized (thread safe); otherwise, false.</returns>
        //bool ICollection.IsSynchronized
        //{
        //    get
        //    {
        //        return this.List.IsSynchronized;
        //    }
        //}

        ///// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        ///// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        //object ICollection.SyncRoot
        //{
        //    get
        //    {
        //        return this.List.SyncRoot;
        //    }
        //}

        ///// <summary>
        ///// Copies the elements of the <see cref="System.Collections.ICollection"></see> to an <see cref="System.Array"></see>, starting at a particular <see cref="System.Array"></see> index.
        ///// </summary>
        ///// <param name="array">The one-dimensional <see cref="System.Array"></see> that is the destination of the elements copied from <see cref="System.Collections.ICollection"></see>. The <see cref="System.Array"></see> must have zero-based indexing.</param>
        ///// <param name="index">The zero-based index in array at which copying begins.</param>
        ///// <exception cref="System.ArgumentNullException">array is null.</exception>
        ///// <exception cref="System.ArgumentOutOfRangeException">index is less than zero.</exception>
        ///// <exception cref="System.ArgumentException">index is less than zero.</exception>
        ///// <exception cref="System.ArgumentException">The type of the source <see cref="System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array.</exception>
        //void ICollection.CopyTo(Array array, int index)
        //{
        //    this.List.CopyTo(array, index);
        //}

        //#endregion |   ICollection Interface   |

        //#region |   IEnumerable Interface   |

        ///// <summary>
        ///// Returns an enumerator that iterates through a collection.
        ///// </summary>
        ///// <returns>
        ///// An <see cref="System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///// </returns>
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.CustomEnumerator;
        //}

        //#endregion |   IEnumerable Interface   |
    }
}
