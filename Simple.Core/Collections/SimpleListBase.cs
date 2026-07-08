using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public abstract class SimpleListBase : IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListBase"/> class that is empty and has the default initial capacity.
        /// </summary>
        public SimpleListBase()
        {
            this.InnerList = new List<object>();
            this.MatchItemValueDelegate = this.MatchItemValueDelegate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple.Collections.SimpleListBase"/> class that wrap around the specified list.
        /// </summary>
        /// <param name="collection">The <see cref="T:System.Collections.IList"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
        public SimpleListBase(IList collection)
        {
            if (collection != null)
            {
                this.InnerList = collection;
            }
            else
            {
                throw new ArgumentNullException("collection is null.");
            }
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Protected Events   |

        protected event CollectionActionEventHandler BeforeInsert;
        protected event CollectionActionEventHandler AfterInsert;
        protected event CollectionActionEventHandler BeforeRemove;
        protected event CollectionActionEventHandler AfterRemove;
        protected event CollectionActionOldValueEventHandler BeforeSet;
        protected event CollectionActionOldValueEventHandler AfterSet;
        protected event EventHandler BeforeClear;
        protected event EventHandler AfterClear;

        #endregion |   Protected Events   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the collection.</returns>
        public int Count
        {
            get { return this.InnerList.Count; }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        protected IList InnerList { get; set; }
        protected MatchItemValue MatchItemValueDelegate { get; set; }

        #endregion |   Protected Properties   |

        #region |   Protected Methods   |

        protected object ListGet(int index)
        {
            object value = this.InnerList[index];
            return value;
        }

        protected void ListSet(int index, object value)
        {
            object oldValue = this.ListGet(index);

            this.OnBeforeSet(index, value, oldValue);
            this.RaiseBeforeSet(index, value, oldValue);

            this.InnerList[index] = value;

            this.OnAfterSet(index, value, oldValue);
            this.RaiseAfterSet(index, value, oldValue);
        }

        protected void ListInsert(int index, object value)
        {
            this.OnBeforeInsert(index, value);
            this.RaiseBeforeInsert(index, value);

            this.InnerList.Insert(index, value);

            this.OnAfterInsert(index, value);
            this.RaiseAfterInsert(index, value);
        }

        protected bool ListRemove(object item)
        {
            bool result = false;
            object itemToRemove = this.MatchItemValueDelegate(item);

            if (itemToRemove != null)
            {
                int index = this.InnerList.IndexOf(itemToRemove);
                result = this.ListRemoveAt(index);
            }

            return result;
        }

        protected bool ListRemoveAt(int index)
        {
            bool exists = index >= 0 && index < this.InnerList.Count;

            if (exists)
            {
                object value = this.InnerList[index];

                this.OnBeforeRemove(index, value);
                this.RaiseBeforeRemove(index, value);

                this.InnerList.RemoveAt(index);

                this.OnAfterRemove(index, value);
                this.RaiseAfterRemove(index, value);
            }

            return exists;
        }

        protected void ListClear()
        {
            this.OnBeforeClear();
            this.RaiseBeforeClear();

            this.InnerList.Clear();

            this.OnAfterClear();
            this.RaiseAfterClear();
        }

        protected virtual object MatchItemValue(object value)
        {
            object result = default(object);

            if (this.InnerList.Contains(value))
            {
                result = value;
            }

            return result;
        }

        #endregion |   Protected Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual void OnBeforeSet(int index, object value, object oldValue)
        {
        }

        protected virtual void OnAfterSet(int index, object value, object oldValue)
        {
        }

        protected virtual void OnBeforeInsert(int index, object value)
        {
        }

        protected virtual void OnAfterInsert(int index, object value)
        {
        }

        protected virtual void OnBeforeRemove(int index, object value)
        {
        }

        protected virtual void OnAfterRemove(int index, object value)
        {
        }

        protected virtual void OnBeforeClear()
        {
        }

        protected virtual void OnAfterClear()
        {
        }

        #endregion |   Protected Virtual Methods   |

        #region |   Private Raise Event Methods   |

        private void RaiseBeforeInsert(int index, object value)
        {
            if (this.BeforeInsert != null)
            {
                this.BeforeInsert(this, new CollectionActionEventArgs(index, value));
            }
        }

        private void RaiseAfterInsert(int index, object value)
        {
            if (this.AfterInsert != null)
            {
                this.AfterInsert(this, new CollectionActionEventArgs(index, value));
            }
        }

        private void RaiseBeforeRemove(int index, object value)
        {
            if (this.BeforeRemove != null)
            {
                this.BeforeRemove(this, new CollectionActionEventArgs(index, value));
            }
        }

        private void RaiseAfterRemove(int index, object value)
        {
            if (this.AfterRemove != null)
            {
                this.AfterRemove(this, new CollectionActionEventArgs(index, value));
            }
        }

        private void RaiseBeforeSet(int index, object value, object oldValue)
        {
            if (this.BeforeSet != null)
            {
                this.BeforeSet(this, new CollectionActionOldValueEventArgs(index, value, oldValue));
            }
        }

        private void RaiseAfterSet(int index, object value, object oldValue)
        {
            if (this.AfterSet != null)
            {
                this.AfterSet(this, new CollectionActionOldValueEventArgs(index, value, oldValue));
            }
        }

        private void RaiseBeforeClear()
        {
            if (this.BeforeClear != null)
            {
                this.BeforeClear(this, new EventArgs());
            }
        }

        private void RaiseAfterClear()
        {
            if (this.AfterClear != null)
            {
                this.AfterClear(this, new EventArgs());
            }
        }

        #endregion |   Private Raise Event Methods   |

        #region |   IEnumerable Interface   |

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        #endregion |   IEnumerable Interface   |
    }

    #region |   Delegates   |

    public delegate void CollectionActionEventHandler(object sender, CollectionActionEventArgs e);
    public delegate void CollectionActionOldValueEventHandler(object sender, CollectionActionOldValueEventArgs e);
    public delegate object MatchItemValue(object item);

    #endregion |   Delegates   |

    #region |   EventArgs Classes   |

    public class CollectionActionEventArgs : EventArgs
    {
        public CollectionActionEventArgs(int index, object value)
        {
            this.Index = index;
            this.Value = value;
        }

        public int Index { get; private set; }
        public object Value { get; private set; }
    }

    public class CollectionActionOldValueEventArgs : CollectionActionEventArgs
    {
        public CollectionActionOldValueEventArgs(int index, object value, object oldValue)
            : base(index, value)
        {
            this.OldValue = oldValue;
        }

        public object OldValue { get; private set; }
    }

    #endregion |   EventArgs Classes   |

}
