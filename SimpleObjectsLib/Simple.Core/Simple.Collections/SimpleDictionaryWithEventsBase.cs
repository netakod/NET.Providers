using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and values. To addition to the <see cref="T:System.Collections.Generic.Dictionary`2"></see> its implements ReadOnly property like readonly IList wrapper, AsReadOnly method, 
    /// AsCustom method custom type casting and dictionary event actions.
    /// It serve as helper when you need to manipulate with dictionary infrastructure and your class doesn't derive from this class. So this class has all needed properties and methods exposed as a public.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public abstract class SimpleDictionaryWithEventsBase<TKey, TValue> : SimpleDictionaryBase<TKey, TValue>, IDictionaryEvents<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SimpleDictionaryWithEventsBase()
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryWithEventsBase(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/>
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public SimpleDictionaryWithEventsBase(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
        public SimpleDictionaryWithEventsBase(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/> 
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> whose elements are
        /// copied to the new <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        /// <exception cref="T:System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
        public SimpleDictionaryWithEventsBase(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryWithEventsBase&lt;TKey, TValue&gt;"/> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2"></see> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryWithEventsBase(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Events   |

        public event DictionaryActionEventHandler<TKey, TValue> BeforeAdd;
        public event DictionaryActionEventHandler<TKey, TValue> AfterAdd;
        public event DictionaryActionEventHandler<TKey, TValue> BeforeRemove;
        public event DictionaryActionEventHandler<TKey, TValue> AfterRemove;
        public event DictionaryActionOldValueEventHandler<TKey, TValue> BeforeSet;
        public event DictionaryActionOldValueEventHandler<TKey, TValue> AfterSet;
        public event EventHandler BeforeClear;
        public event OldCountEventHandler AfterClear;
		public event CountChangeEventHandler CountChange;

		#endregion |   Events   |

		#region |   Protected Overrided Raise Event Methods   |

		protected override void OnBeforeAdd(TKey key, TValue value)
        {
            base.OnBeforeAdd(key, value);
            this.BeforeAdd?.Invoke(this, new DictionaryActionEventArgs<TKey, TValue>(key, value));
        }

        protected override void OnAfterAdd(TKey key, TValue value)
        {
            base.OnAfterAdd(key, value);
            this.AfterAdd?.Invoke(this, new DictionaryActionEventArgs<TKey, TValue>(key, value));
        }

        protected override void OnBeforeRemove(TKey key, TValue value)
        {
            base.OnBeforeRemove(key, value);
            this.BeforeRemove?.Invoke(this, new DictionaryActionEventArgs<TKey, TValue>(key, value));
        }

        protected override void OnAfterRemove(TKey key, TValue value)
        {
            base.OnAfterRemove(key, value);
            this.AfterRemove?.Invoke(this, new DictionaryActionEventArgs<TKey, TValue>(key, value));
        }

        protected override void OnBeforeSet(TKey key, TValue value, TValue oldValue)
        {
 	        base.OnBeforeSet(key, value, oldValue);
            this.BeforeSet?.Invoke(this, new DictionaryActionOldValueEventArgs<TKey, TValue>(key, value, oldValue));
        }

        protected override void OnAfterSet(TKey key, TValue value, TValue oldValue)
        {
            base.OnAfterSet(key, value, oldValue);
            this.AfterSet?.Invoke(this, new DictionaryActionOldValueEventArgs<TKey, TValue>(key, value, oldValue));
        }

        protected override void OnBeforeClear()
        {
            base.OnBeforeClear();
            this.BeforeClear?.Invoke(this, new EventArgs());
        }

        protected override void OnAfterClear(int oldCount)
        {
            base.OnAfterClear(oldCount);
            this.AfterClear?.Invoke(this, new OldCountEventArgs(oldCount));
        }

		protected override void OnCountChange(int count, int oldCount)
		{
			base.OnCountChange(count, oldCount);
			this.CountChange?.Invoke(this, new CountChangeEventArgs(count, oldCount));
		}

		#endregion |   Protected Overrided Raise Event Methods   |
	}

    #region |   Interfaces   |

    public interface IDictionaryEvents<TKey, TValue>
    {
        event DictionaryActionEventHandler<TKey, TValue> BeforeAdd;
        event DictionaryActionEventHandler<TKey, TValue> AfterAdd;
        event DictionaryActionEventHandler<TKey, TValue> BeforeRemove;
        event DictionaryActionEventHandler<TKey, TValue> AfterRemove;
        event DictionaryActionOldValueEventHandler<TKey, TValue> BeforeSet;
        event DictionaryActionOldValueEventHandler<TKey, TValue> AfterSet;
        event EventHandler BeforeClear;
        event OldCountEventHandler AfterClear;
    }

    #endregion |   Interfaces   |
}
