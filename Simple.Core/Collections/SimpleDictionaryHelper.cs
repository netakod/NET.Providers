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
    /// AsCustom method for custom type casting.
	/// It serve as helper when you need to manipulate with dictionary infrastructure and your class doesn't derive from <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> class. So this class has all needed dictionary properties and methods exposed as a public.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public class SimpleDictionaryHelper<TKey, TValue> : SimpleDictionary<TKey, TValue>, IDictionaryWithEvents<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionaryEvents<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SimpleDictionaryHelper()
	    {
	    }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryHelper(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/>
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public SimpleDictionaryHelper(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
        public SimpleDictionaryHelper(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/> 
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
        public SimpleDictionaryHelper(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionaryHelper&lt;TKey, TValue&gt;"/> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2"></see> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleDictionaryHelper(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Properties   |

        public new IDictionary<TKey, TValue> InnerDictionary
        {
            get { return base.InnerDictionary; }
        }

        /// <summary>
        /// Gets a read-only <see cref="SimpleCollection&lt;TKey&gt;"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see> that can be customized.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="SimpleCollection&lt;TKey&gt;"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public new CustomCollection<TKey> CustomKeys
        {
            get { return base.CustomKeys; }
        }

        /// <summary>
        /// Gets an read-only <see cref="SimpleCollection&lt;TKey&gt;"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see> that can be customized.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="SimpleCollection&lt;TKey&gt;"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public new CustomCollection<TValue> CustomValues
        {
            get { return base.CustomValues; }
        }

        #endregion |   Public Properties   |

        #region |   Public Methods   |

        public new void DictionaryAdd(TKey key, TValue value)
        {
            base.DictionaryAdd(key, value);
        }

        public new bool DictionaryRemove(TKey key)
        {
            return base.DictionaryRemove(key);
        }

        public new TValue DictionaryGet(TKey key)
        {
            return base.DictionaryGet(key);
        }

        public new void DictionarySet(TKey key, TValue value)
        {
            base.DictionarySet(key, value);
        }

        public new void DictionaryClear()
        {
            base.DictionaryClear();
        }

        #endregion |   Public Methods   |
    }
}
