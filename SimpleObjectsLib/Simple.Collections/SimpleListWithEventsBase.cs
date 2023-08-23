using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.
    /// To addition to the <see cref="T:System.Collections.Generic.IList`1"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method, AsCustom method custom type casting
    /// and collection action events.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    [Serializable]
    public abstract class SimpleListWithEventsBase<T> : SimpleListBase<T>, ICollectionEvents<T>, IEnumerable<T>, IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListWithEventsBase&lt;T&gt;"/> class that is empty and has the default initial capacity.
        /// </summary>
        public SimpleListWithEventsBase()
        {
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="Simple.Collections.SimpleListWithEventsBase&lt;T&gt;"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleListWithEventsBase(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListWithEventsBase&lt;T&gt;"/> class that wrap around the specified generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.Generic.IList`1"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
        public SimpleListWithEventsBase(IList<T> collectionToWrap)
            : base(collectionToWrap)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListWithEventsBase&lt;T&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.IList"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public SimpleListWithEventsBase(IList collectionToWrap)
            : base(collectionToWrap)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Events   |

        public event CollectionActionEventHandler<T> BeforeInsert;
        public event CollectionActionEventHandler<T> AfterInsert;
        public event CollectionActionEventHandler<T> BeforeRemove;
        public event CollectionActionEventHandler<T> AfterRemove;
        public event CollectionActionOldValueEventHandler<T> BeforeSet;
        public event CollectionActionOldValueEventHandler<T> AfterSet;
        public event EventHandler BeforeClear;
        public event EventHandler AfterClear;
        public event CountChangeEventHandler CountChange;

        #endregion |   Public Events   |

        #region |   Protected Overrided Raise Event Methods   |

        protected override void OnBeforeInsert(int index, T value)
        {
            base.OnBeforeInsert(index, value);

            if (this.BeforeInsert != null)
                this.BeforeInsert(this, new CollectionActionEventArgs<T>(index, value));
        }

        protected override void OnAfterInsert(int index, T value)
        {
            base.OnAfterInsert(index, value);

            if (this.AfterInsert != null)
                this.AfterInsert(this, new CollectionActionEventArgs<T>(index, value));
        }

        protected override void OnBeforeRemove(int index, T value)
        {
            base.OnBeforeRemove(index, value);

            if (this.BeforeRemove != null)
                this.BeforeRemove(this, new CollectionActionEventArgs<T>(index, value));
        }

        protected override void OnAfterRemove(int index, T value)
        {
            base.OnAfterRemove(index, value);

            if (this.AfterRemove != null)
                this.AfterRemove(this, new CollectionActionEventArgs<T>(index, value));
        }

        protected override void OnBeforeSet(int index, T value, T oldValue)
        {
            base.OnBeforeSet(index, value, oldValue);

            if (this.BeforeSet != null)
                this.BeforeSet(this, new CollectionActionOldValueEventArgs<T>(index, value, oldValue));
        }

        protected override void OnAfterSet(int index, T value, T oldValue)
        {
            base.OnAfterSet(index, value, oldValue);

            if (this.AfterSet != null)
                this.AfterSet(this, new CollectionActionOldValueEventArgs<T>(index, value, oldValue));
        }

        protected override void OnBeforeClear()
        {
            base.OnBeforeClear();

            if (this.BeforeClear != null)
                this.BeforeClear(this, new EventArgs());
        }

        protected override void OnAfterClear()
        {
            base.OnAfterClear();

            if (this.AfterClear != null)
                this.AfterClear(this, new EventArgs());
        }

        protected override void OnCountChange(int count, int oldCount)
        {
            base.OnCountChange(count, oldCount);

            if (this.CountChange != null)
                this.CountChange(this, new CountChangeEventArgs(count, oldCount));
        }

        #endregion |   Protected Overrided Raise Event Methods   |
    }

    #region |   Interfaces   |
    
    public interface ICollectionEvents<T> 
    {
        event CollectionActionEventHandler<T> BeforeInsert;
        event CollectionActionEventHandler<T> AfterInsert;
        event CollectionActionEventHandler<T> BeforeRemove;
        event CollectionActionEventHandler<T> AfterRemove;
        event CollectionActionOldValueEventHandler<T> BeforeSet;
        event CollectionActionOldValueEventHandler<T> AfterSet;
        event EventHandler BeforeClear;
        event EventHandler AfterClear;
        event CountChangeEventHandler CountChange;
    }

    #endregion |   Interfaces   |
}
