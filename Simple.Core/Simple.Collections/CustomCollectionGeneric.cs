using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom Collection interface for the specified T type. Input collectionToWrap elements must be custable to U object type.
    /// </summary>
    /// <typeparam name="U">The type of the elements in the collection.</typeparam>
    /// <typeparam name="U">Generic type U to convert to.</typeparam>
    public class CustomCollection<T, U> : CustomEnumerable<T, U>, ICollection<U>, IEnumerable<U>, ICollection, IEnumerable
        where U : T
    {
        #region |   Private Members   |

        private ReadOnlyCollection<U> readOnlyCollection = null;

        #endregion |   Private Members   |

        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCollection&lt;T, U&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.IList`1"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public CustomCollection(ICollection<T> collectionToWrap)
            : base(collectionToWrap)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return this.OriginalCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return this.OriginalCollection.IsReadOnly; }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        /// <summary>
        /// Gets the origanal input collection.
        /// </summary>
        protected new ICollection<T> OriginalCollection
        {
            get { return base.OriginalCollection as ICollection<T>; }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyCollection&lt;T&gt;"/> wrapper for the current collection.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public ReadOnlyCollection<U> AsReadOnly()
        {
            if (this.readOnlyCollection == null)
            {
                this.readOnlyCollection = this.CreateReadOnlyCollection() as ReadOnlyCollection<U>;
            }

            return this.readOnlyCollection;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns> true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. </returns>
        public bool Contains(U item)
        {
            foreach (U collectionElement in this)
            {
                if (Comparison.IsEqual(collectionElement, item))
                {
                    return true;
                }
            }

            return false;
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
        public void CopyTo(U[] array, int arrayIndex)
        {
            this.OriginalCollection.ToArray().CopyTo(array, arrayIndex);
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected int CollectionAdd(U item)
        {
            this.OnInsert(this.Count, item);
            this.OriginalCollection.Add((T)item);
            return this.Count - 1;
        }

        protected bool CollectionRemove(U item)
        {
            int index = this.GetIndexOf(item);
            return this.CollectionRemoveAt(index, item);
        }

        protected bool CollectionRemoveAt(int index, U item)
        {
            bool itemExists = index >= 0 && index < this.OriginalCollection.Count;

            if (itemExists)
            {
                this.OnRemove(index, item);
                this.DoRemoveAt(index, item);
            }

            return itemExists;
        }

        protected void CollectionClear()
        {
            this.OnClear();
            this.OriginalCollection.Clear();
        }

        #endregion |   Protected Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual object CreateReadOnlyCollection()
        {
            return new ReadOnlyCollection<U>(this);
        }

        protected virtual int GetIndexOf(U item)
        {
            for (int index = 0; index < this.Count; index++)
            {
                T element = this.OriginalCollection.ElementAt(index);

                if (Comparison.IsEqual(element, item))
                {
                    return index;
                }
            }

            return -1;
        }

        protected virtual void DoRemoveAt(int index, U item)
        {
            this.OriginalCollection.Remove(item);
        }

        protected virtual void OnInsert(int index, U item)
        {
        }

        protected virtual void OnRemove(int index, U item)
        {
        }

        protected virtual void OnClear()
        {
        }

        #endregion |   Protected Virtual Methods   |

        #region |   ICollection<T> Interface   |

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public void Add(U item)
        {
            this.CollectionAdd(item);
        }

        /// <summary>
        /// Removes all items from the System.Collections.Generic.ICollection<T>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.ICollection<T> is read-only.</exception>
        public void Clear()
        {
            this.CollectionClear();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. 
        ///   This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public bool Remove(U item)
        {
            return this.CollectionRemove(item);
        }

        #endregion |   ICollection<T> Interface   |

        #region |   ICollection Interface   |

        /// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        /// <returns>true if access to the System.Collections.ICollection is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return null; }
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
            this.OriginalCollection.ToArray().CopyTo(array, index);
        }

        #endregion |   ICollection Interface   |

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
