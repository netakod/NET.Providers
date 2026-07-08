using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Simple ;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom List interface for the specified T type. Input collectionToWrap elements must be custable to U object type.
    /// </summary>
    /// <typeparam name="U">The type of the elements in the collection.</typeparam>
    /// <typeparam name="U">Generic type U to convert to.</typeparam>
    [Serializable]
    public class CustomList<T, U> : CustomCollection<T, U>, IList<U>, ICollection<U>, IEnumerable<U>, IList, ICollection, IEnumerable
        where U : T
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomList&lt;T, U&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.IList`1"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public CustomList(IList<T> collectionToWrap)
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
        public U this[int index]
        {
            get { return this.CollectionGet(index); }
            set { this.CollectionSet(index, value); }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        /// <summary>
        /// Returns the <see cref="T:System.Collections.IList"></see> of the original list.
        /// </summary>
        protected new IList<T> OriginalCollection
        {
            get { return base.OriginalCollection as IList<T>; }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyList&lt;T&gt;"/> wrapper for the current collection.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyList&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IList`1"></see>.</returns>
        public new ReadOnlyList<U> AsReadOnly()
        {
            return base.AsReadOnly() as ReadOnlyList<U>;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="CustomList&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>. The item can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="CustomList&lt;T&gt;"/>, if found; otherwise, -1.</returns>
        public int IndexOf(U item)
        {
            for (int index = 0; index < this.Count; index++)
            {
                T originalItem = this.OriginalCollection[index];
                
                if (Comparison.IsEqual(originalItem, item))
                {
                    return index;
                }
            }

            return -1;
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected U CollectionGet(int index)
        {
            U value = (U)this.OriginalCollection[index];
            return value;
        }

        protected void CollectionSet(int index, U value)
        {
            U oldValue = this.CollectionGet(index);
            this.OnSet(index, oldValue, value);
            this.OriginalCollection[index] = value;
        }

        protected void CollectionInsert(int index, U value)
        {
            this.OnInsert(index, value);
            this.OriginalCollection.Insert(index, value);
        }

        #endregion |   Protected Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual void OnSet(int index, U oldValue, U newValue)
        {
        }

        #endregion |   Protected Virtual Methods   |

        #region |   Protected Overrided Methods   |

        protected override object CreateReadOnlyCollection()
        {
            return new ReadOnlyList<U>(this);
        }

        protected override int GetIndexOf(U item)
        {
            return this.OriginalCollection.IndexOf(item);
        }

        protected override void DoRemoveAt(int index, U item)
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
        public void Insert(int index, U item)
        {
            this.CollectionInsert(index, item);
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
            U item = this.CollectionGet(index);
            this.CollectionRemoveAt(index, item);
        }

        #endregion |   IList<T> Interface   |

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
            set { this[index] = (U)value; }
        }

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to add to the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList"></see> has a fixed size.</exception>
        int IList.Add(object value)
        {
            return this.CollectionAdd((U)value);
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
            return this.OriginalCollection.Contains((U)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            return this.OriginalCollection.IndexOf((U)value);
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
            this.CollectionInsert(index, (U)item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="item">The System.Object to remove from the <see cref="System.Collections.IList"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.Remove(object item)
        {
            this.CollectionRemove((U)item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.RemoveAt(int index)
        {
            U item = this[index];
            this.CollectionRemoveAt(index, item);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.IList"></see> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="System.Collections.IList"></see> has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        #endregion |   IList Interface   |
    }
}
