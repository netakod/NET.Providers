using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Linq;

namespace Simple.Collections
{
    /// <summary>
    /// Provides the base class for a generic read-only collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    [Serializable]
    public class ReadOnlyCollection<T> : IReadOnlyCollection<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
		/// <summary>
		/// The original collection field.
		/// </summary>
		//private ICollection<T> originalCollection;
		private IEnumerable<T> originalCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollection&lt;T&gt;"/> class that is a read-only wrapper around the specified collection.
        /// </summary>
        /// <param name="collectionToWrap">collection to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
        public ReadOnlyCollection(IEnumerable<T> originalCollection)
        {
            if (originalCollection != null)
            {
                this.originalCollection = originalCollection;
            }
            else
            {
                throw new System.ArgumentNullException("Collection is null");
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return this.OriginalCollection.Count(); }
        }

        /// <summary>
        /// Returns the <see cref="T:System.Collections.Generic.ICollection`1"></see> that the <see cref="ReadOnlyCollection&lt;T&gt;"></see> wraps.
        /// </summary>
        protected IEnumerable<T> OriginalCollection
        {
            get { return this.originalCollection; }
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
            return this.OriginalCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source 
        /// <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast 
        /// automatically to the type of the destination array.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
			Array.Copy(this.originalCollection.ToArray(), 0, array, arrayIndex, this.Count);
        }

        ///// <summary>
        ///// Add does not change a ReadOnlyICollection
        ///// </summary>
        ///// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        ///// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        //public void Add(T item)
        //{
        //    throw new NotSupportedException("Collection is read-only");
        //}



        ///// <summary>
        ///// Clear does not change a ReadOnlyICollection
        ///// </summary>
        ///// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only. </exception>
        //public void Clear()
        //{
        //    throw new NotSupportedException("Collection is read-only");
        //}

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        ///// <summary>
        ///// Remove does not change a ReadOnlyICollection
        ///// </summary>
        ///// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        ///// <returns>
        ///// true if item was successfully removed from the <see cref="T:System.Collections.Generic.IList`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///// </returns>
        ///// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        //public bool Remove(T item)
        //{
        //    throw new NotSupportedException("Collection is read-only");
        //}

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns> A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection. </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.OriginalCollection.GetEnumerator();
        }

        //#region |   IList<T> interface   |

        ///// <summary>
        ///// Gets the element at the specified index. Sets throw an System.NotSupportedException as it is read-only collection.
        ///// </summary>
        ///// <typeparam name="T">The type of elements in the list.</typeparam> 
        ///// <param name="index">The zero-based index of the element to get or set.</param>
        ///// <returns>The element at the specified index.</returns>
        ///// <exception cref="System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        //T IList<T>.this[int index]
        //{
        //    get
        //    {
        //        return this[index];
        //    }

        //    set
        //    {
        //        throw new NotSupportedException("Collection is read-only.");
        //    }
        //}

        /////// <summary>Determines the index of a specific item in the System.Collections.Generic.IList<T>.</summary>
        /////// <typeparam name="T">The type of elements in the list.</typeparam> 
        /////// <param name="item">The object to locate in the System.Collections.Generic.IList<T>.</param>
        /////// <returns>The index of item if found in the list; otherwise, -1.</returns>
        ////public int IndexOf(T item)
        ////{
        ////    return this.iList.IndexOf(item);
        ////}

        ///// <summary>
        ///// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        ///// </summary>
        ///// <typeparam name="T">The type of elements in the list.</typeparam> 
        ///// <param name="index">The zero-based index at which item should be inserted.</param>
        ///// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        ///// <returns>The index of item if found in the list; otherwise, -1.</returns>
        ///// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        //void IList<T>.Insert(int index, T item)
        //{
        //    throw new NotSupportedException("Collection is read-only.");
        //}

        ///// <summary>
        ///// Removes cannot be performed on the read-only collection.
        ///// </summary>
        ///// <param name="index">The zero-based index of the item to remove.</param>
        ///// <returns>The index of item if found in the list; otherwise, -1.</returns>
        ///// <exception cref="System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        ///// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        //void IList<T>.RemoveAt(int index)
        //{
        //    throw new NotSupportedException("Collection is read-only.");
        //}

        //#endregion |   IList<T> interface   |

        #region |   ICollection<T> interface   |

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam> 
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException("Collection is read-only.");
        }


        ///// <summary>Copies the elements of the System.Collections.Generic.ICollection<T> to an System.Array, starting at a particular System.Array index.</summary>
        ///// <typeparam name="T">The type of elements in the list.</typeparam> 
        ///// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.Generic.ICollection<T>. 
        /////          The System.Array must have zero-based indexing.</param>
        ///// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ///// <returns>true if item is found in the System.Collections.Generic.ICollection<T>; otherwise, false.</returns>
        ///// <exception cref="System.ArgumentNullException">array is null.</exception>
        ///// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ///// <exception cref="System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of 
        /////     array.-or-The number of elements in the source System.Collections.Generic.ICollection<T> is greater than the available space from 
        /////     arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        //public void CopyTo(T[] array, int arrayIndex)
        //{
        //    this.list.CopyTo(array, arrayIndex);
        //}

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam> 
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. 
        ///   This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public bool Remove(T item)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        #endregion |   ICollection<T> interface   |

        #region |   ICollection interface   |

        /// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        /// <returns>true if access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return (this.OriginalCollection as IList).IsSynchronized; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return (this.OriginalCollection as IList).SyncRoot; }
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
            (this.OriginalCollection as ICollection).CopyTo(array, index);
        }

        #endregion |   ICollection interface   |

        #region |   IEnumerable   |

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.OriginalCollection.GetEnumerator();
        }

        #endregion |   IEnumerable   |
    }
}
