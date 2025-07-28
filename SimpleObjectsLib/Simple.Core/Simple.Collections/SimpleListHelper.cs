using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.
    /// To addition to the <see cref="T:System.Collections.Generic.IList`1"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method, AsCustom method custom type casting.
    /// It serve as helper when you need to manipulate with list infrastructure and your class doesn't derive from this class. So this class has all needed properties and method exposed as a public.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    [Serializable]
    public class SimpleListHelper<T> : SimpleList<T>, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListHelper&lt;T&gt;"/> class that is empty and has the default initial capacity.
        /// </summary>
        public SimpleListHelper()
        {
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="Simple.Collections.SimpleListHelper&lt;T&gt;"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleListHelper(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListHelper&lt;T&gt;"/> class that wrap around the specified generic list.
        /// </summary>
        /// <param name="collection">The <see cref="T:System.Collections.Generic.IList`1"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
        public SimpleListHelper(IList<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListHelper&lt;T&gt;"/> class that wrap around the specified non-generic list.
        /// </summary>
        /// <param name="collectionToWrap">The <see cref="T:System.Collections.IList"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collectionToWrap is null.</exception>
        public SimpleListHelper(IList collectionToWrap)
            : base(collectionToWrap)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Properties   |

        public new IList<T> InnerList
        {
            get { return base.InnerList; }
        }
        
        public new MatchItemValue<T> MatchItemValueDelegate 
        {
            get { return base.MatchItemValueDelegate; }
            set { base.MatchItemValueDelegate = value; } 
        }

        public new bool IsReadOnly
        {
            get { return base.IsReadOnly; }
            set { base.IsReadOnly = value; }
        }

        #endregion |   Public Properties   |

        #region |   Public Methods   |

        public new T ListGet(int index)
        {
            return base.ListGet(index);
        }

        public new void ListSet(int index, T value)
        {
            base.ListSet(index, value);
        }

        public new int ListAdd(T item)
        {
            return base.ListAdd(item);
        }

        public new void ListInsert(int index, T value)
        {
            base.ListInsert(index, value);
        }

        public new bool ListRemove(T item)
        {
            return base.ListRemove(item);
        }

        public new bool ListRemoveAt(int index)
        {
            return base.ListRemoveAt(index);
        }

        public new void ListClear()
        {
            base.ListClear();
        }

        #endregion |   Public Methods   |
    }
}
