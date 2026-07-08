using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public abstract class SimpleListBase<T> : SimpleCollectionBase<T>, IEnumerable<T>, IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListBase&lt;T&gt;"/> class that is empty and has the default initial capacity.
        /// </summary>
        public SimpleListBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <<see cref="Simple.Collections.SimpleListBase&lt;T&gt;"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleListBase(int capacity)
            : base(capacity)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListBase&lt;T&gt;"/> class that wrap around the specified generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.Generic.ICollection`1"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">list is null.</exception>
        public SimpleListBase(ICollection<T> collectionToWrap)
            : base(collectionToWrap)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleListBase&lt;T&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.ICollection"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public SimpleListBase(ICollection collectionToWrap)
            : base(collectionToWrap)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Protected Properties   |

        protected IList<T> InnerList
        {
            get { return base.InnerCollection as IList<T>; }
			set { base.InnerCollection = value; }
        }

        #endregion |   Protected Properties   |

        #region |   Protected Methods   |

        protected virtual void ListSet(int index, T item)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("collection is read-only.");

            this.InternalListSet(index, item);
        }

        protected void ListInsert(int index, T item)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("collection is read-only.");

            this.InternalListInsert(index, item);
        }

        protected void InternalListSet(int index, T item)
        {
            lock (this.lockObject)
            {
                T oldValue = this.ListGet(index);

                this.OnBeforeSet(index, item, oldValue);
                this.InnerList[index] = item;
                this.OnAfterSet(index, item, oldValue);
            }
        }

        protected void InternalListInsert(int index, T item)
        {
            lock (this.lockObject)
            {
                this.OnBeforeInsert(index, item);
                this.InnerList.Insert(index, item);
                this.OnAfterInsert(index, item);
                this.OnCountChange(this.Count, this.Count - 1);
            }
        }

        protected override int GetIndexOf(T item)
        {
            lock (this.lockObject)
            {
                return this.InnerList.IndexOf(item);
            }
        }

        protected override void DoRemoveAt(int index, T value)
        {
            lock (this.lockObject)
            {
                this.InnerList.RemoveAt(index);
            }
        }

        //protected bool ListRemove(T item)
        //{
        //    bool result = false;
        //    T itemToRemove = this.MatchItemValueDelegate(item);

        //    if (itemToRemove != null)
        //    {
        //        int index = this.InnerList.IndexOf(itemToRemove);
        //        result = this.ListRemoveAt(index);
        //    }

        //    return result;
        //}

        //protected bool ListRemoveAt(int index)
        //{
        //    if (!this.IsReadOnly)
        //    {
        //        bool itemExists = index >= 0 && index < this.InnerList.Count;

        //        if (itemExists)
        //        {
        //            T value = this.InnerList[index];

        //            this.OnBeforeRemove(index, value);
        //            this.RaiseBeforeRemove(index, value);

        //            this.InnerList.RemoveAt(index);

        //            this.OnAfterRemove(index, value);
        //            this.RaiseAfterRemove(index, value);
        //        }

        //        return itemExists;
        //    }
        //    else
        //    {
        //        throw new NotSupportedException("Collection is read-only.");
        //    }
        //}

        //protected void ListClear()
        //{
        //    if (!this.IsReadOnly)
        //    {
        //        this.OnBeforeClear();
        //        this.RaiseBeforeClear();

        //        this.InnerList.Clear();

        //        this.OnAfterClear();
        //        this.RaiseAfterClear();
        //    }
        //}

        //protected virtual T MatchItemValue(T value)
        //{
        //    T result = default(T);

        //    if (this.InnerList.Contains(value))
        //    {
        //        result = value;
        //    }

        //    return result;
        //}

        //protected virtual T InnerListGet(int index)
        //{
        //    return this.InnerList[index];
        //}

        #endregion |   Protected Methods   |
    }
}
