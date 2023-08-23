using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom Collection interface for the specified T type. Input object elements must be custable to T object type.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public class CustomCollection<T> : CustomEnumerable<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        #region |   Private Members   |

        private ReadOnlyCollection<T> readOnlyCollection = null;

		#endregion |   Private Members   |

		#region |   Constructor(s) and Initialization   |

		public CustomCollection(ICollection<T> collectionToWrap)
			: base(collectionToWrap)
		{
            bool isReadOnly = this.IsReadOnly;
		}

		public CustomCollection(ICollection collectionToWrap)
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
            get 
            {
                lock (lockObject)
                {
                    return this.OriginalCollection.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get
            {
                bool value = true;

                PropertyInfo propertyInfo = this.OriginalCollection.GetType().GetProperty("IsReadOnly");

                if (propertyInfo != null)
                    value = (bool)propertyInfo.GetValue(this.OriginalCollection, null);

                return value;
            }
        }

		#endregion |   Public Properties   |

		//#region |   Protected Properties   |

		protected new ICollection OriginalCollection
		{
			get { return base.OriginalCollection as ICollection; }
		}

		//#endregion |   Protected Properties   |

		#region |   Public Methods   |

		/// <summary>
		/// Returns a read-only <see cref="ReadOnlyCollection&lt;T&gt;"/> wrapper for the current collection.
		/// </summary>
		/// <returns>A <see cref="ReadOnlyCollection&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.ICollection`1"/>.</returns>
		public ReadOnlyCollection<T> AsReadOnly()
        {
            if (this.readOnlyCollection == null)
            {
                this.readOnlyCollection = this.CreateReadOnlyCollection() as ReadOnlyCollection<T>;
            }

            return this.readOnlyCollection;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns> true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. </returns>
        public bool Contains(T item)
        {
            bool result = false;

            lock (lockObject)
            {
                if (this.OriginalCollection is ICollection<T>)
                {
                    result = (this.OriginalCollection as ICollection<T>).Contains(item);
                }
                else
                {
                    foreach (T collectionElement in this)
                    {
                        if (collectionElement.Equals(item))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            
            return result;
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
            lock (lockObject)
            {
                this.OriginalCollection.CopyTo(array, arrayIndex);
            }
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected int CollectionAdd(T item)
        {
            if (this.OriginalCollection is ICollection<T>)
            {
                lock (lockObject)
                {
                    this.OnInsert(this.Count, item);
                    (this.OriginalCollection as ICollection<T>).Add(item);

                    return this.Count - 1;
                }
            }
            else
            {
                throw new NotSupportedException("Input wraped collection does not support insertion.");
            }
        }

        protected bool CollectionRemove(T item)
        {
            bool canRemove = false;

            if (this.OriginalCollection is ICollection<T>)
            {
                lock (lockObject)
                {
                    int index = this.GetIndexOf(item);
                    canRemove = this.CollectionRemoveAt(index, item);
                }
            }
            else
            {
                throw new NotSupportedException("Input wraped collection does not support removing.");
            }

            return canRemove;
        }

        protected bool CollectionRemoveAt(int index, T item)
        {
            lock (lockObject)
            {
                bool itemExists = index >= 0 && index < this.OriginalCollection.Count;

                if (itemExists)
                {
                    this.OnRemove(index, item);
                    this.DoRemoveAt(index, item);
                }

                return itemExists;
            }
        }


        protected void CollectionClear()
        {
            if (this.OriginalCollection is ICollection<T>)
            {
                lock (lockObject)
                {
                    this.OnClear();
                    (this.OriginalCollection as ICollection<T>).Clear();
                }
            }
            else
            {
                throw new NotSupportedException("Input wraped collection does not support removing.");
            }
        }

        #endregion |   Protected Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual object CreateReadOnlyCollection()
        {
            return new ReadOnlyCollection<T>(this as ICollection<T>);
        }

        protected virtual int GetIndexOf(T item)
        {
            lock (lockObject)
            {
                if (this.OriginalCollection is ICollection<T>)
                {
                    for (int index = 0; index < this.Count; index++)
                    {
                        T element = (this.OriginalCollection as ICollection<T>).ElementAt(index);

                        if (Comparison.IsEqual(element, item))
                        {
                            return index;
                        }
                    }
                }
                else
                {
                    int index = 0;

                    foreach (object element in this.OriginalCollection)
                    {
                        if (Comparison.IsEqual(element, item))
                        {
                            return index;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }

                return -1;
            }
        }

        protected virtual void DoRemoveAt(int index, T item)
        {
            if (this.OriginalCollection is ICollection<T>)
            {
                lock (lockObject)
                {
                    (this.OriginalCollection as ICollection<T>).Remove(item);
                }
            }
            else
            {
                throw new NotSupportedException("Input wraped collection does not support removing.");
            }
        }

        protected virtual void OnInsert(int index, T item)
        {
        }

        protected virtual void OnRemove(int index, T item)
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
        public void Add(T item)
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
        public bool Remove(T item)
        {
            return this.CollectionRemove(item);
        }

        #endregion |   ICollection<T> Interface   |

        #region |   ICollection Interface   |

        /// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        /// <returns>true if access to the System.Collections.ICollection is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return this.OriginalCollection.IsSynchronized; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return this.OriginalCollection.SyncRoot; }
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
            lock (lockObject)
            {
                this.OriginalCollection.CopyTo(array, index);
            }
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
        //    lock (lockObject)
        //    {
        //        return this.CustomEnumerator;
        //    }
        //}

        //#endregion |   IEnumerable Interface   |
    }


    #region |   Delegates   |

    public delegate void CollectionActionEventHandler<T>(object sender, CollectionActionEventArgs<T> e);
    public delegate void CollectionActionOldValueEventHandler<T>(object sender, CollectionActionOldValueEventArgs<T> e);
    public delegate void CountChangeEventHandler(object sender, CountChangeEventArgs e);
    public delegate T MatchItemValue<T>(T item);

    #endregion |   Delegates   |

    #region |   EventArgs Classes   |

    public class CollectionActionEventArgs<T> : EventArgs
    {
        public CollectionActionEventArgs(int index, T value)
        {
            this.Index = index;
            this.Value = value;
        }

        public int Index { get; private set; }
        public T Value { get; private set; }
    }

    public class CollectionActionOldValueEventArgs<T> : CollectionActionEventArgs<T>
    {
        public CollectionActionOldValueEventArgs(int index, T value, T oldValue)
            : base(index, value)
        {
            this.OldValue = oldValue;
        }

        public T OldValue { get; private set; }
    }

    public class CountEventArgs : EventArgs
    {
        public CountEventArgs(int count)
        {
            this.Count = count;
        }

        public int Count { get; private set; }
    }

    public class OldCountEventArgs : EventArgs
    {
        public OldCountEventArgs(int oldCount)
        {
            this.OldCount = oldCount;
        }

        public int OldCount { get; private set; }
    }

    public class CountChangeEventArgs : CountEventArgs
    {
        public CountChangeEventArgs(int count, int oldCount)
            : base(count)
        {
            this.OldCount = oldCount;
        }

        public int OldCount { get; private set; }
    }

    #endregion |   EventArgs Classes   |

}
