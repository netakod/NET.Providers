using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public class SimpleTwoKeyDictionaryBase<TKey1, TKey2, TValue> : IEnumerable<KeyValuePair<TKey1, TValue>>, IEnumerable
    {
        #region |   Private Members   |

        private object lockObject = new object();
        private Hashtable key2ByKey1 = new Hashtable();
        private Hashtable key1ByKey2 = new Hashtable();
        
        #endregion |   Private Members   |

        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTwoKeyDictionaryBase&lt;TKey, TValue&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SimpleTwoKeyDictionaryBase()
        {
            this.InnerDictionary1 = new Dictionary<TKey1, TValue>();
            this.InnerDictionary2 = new Dictionary<TKey2, TValue>();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SimpleTwoKeyDictionaryBase&lt;TKey, TValue&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleTwoKeyDictionaryBase&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public SimpleTwoKeyDictionaryBase(int capacity)
        {
            this.InnerDictionary1 = new Dictionary<TKey1, TValue>(capacity);
            this.InnerDictionary2 = new Dictionary<TKey2, TValue>(capacity);
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets the number of elements contained in the  <see cref="SimpleTwoKeyDictionaryBase&lt;TKey, TValue&gt;"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the  <see cref="SimpleTwoKeyDictionaryBase&lt;TKey, TValue&gt;"/>.</returns>
        public int Count
        {
            get { return this.InnerDictionary1.Count; }
        }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        protected IDictionary<TKey1, TValue> InnerDictionary1 { get; private set; }
        protected IDictionary<TKey2, TValue> InnerDictionary2 { get; private set; }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey1, TValue>> GetEnumerator()
        {
            lock (lockObject)
            {
                return this.InnerDictionary1.GetEnumerator();
            }
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected void DictionaryAdd(TKey1 key1, TKey2 key2, TValue value)
        {
            lock (lockObject)
            {
                this.OnAdd(key1, key2, value);

                this.InnerDictionary1.Add(key1, value);
                this.InnerDictionary2.Add(key2, value);

                this.key2ByKey1.Add(key1, key2);
                this.key1ByKey2.Add(key2, key1);
            }
        }

        protected bool DictionaryRemoveByKey1(TKey1 key1)
        {
            bool isRemoved = false;

            lock (lockObject)
            {
                if (this.InnerDictionary1.ContainsKey(key1))
                {
                    TKey2 key2 = (TKey2)this.key2ByKey1[key1];

                    this.OnRemove(key1, key2);
                    this.DictionaryRemove(key1, key2);

                    isRemoved = true;
                }
            }

            return isRemoved;
        }

        protected bool DictionaryRemoveByKey2(TKey2 key2)
        {
            bool isRemoved = false;

            lock (lockObject)
            {
                if (this.InnerDictionary2.ContainsKey(key2))
                {
                    TKey1 key1 = (TKey1)this.key1ByKey2[key2];

                    this.OnRemove(key1, key2);
                    this.DictionaryRemove(key1, key2);

                    isRemoved = true;
                }
            }

            return isRemoved;
        }

        protected TValue DictionaryGetByKey1(TKey1 key1)
        {
            TValue value;

            lock (lockObject)
            {
                this.InnerDictionary1.TryGetValue(key1, out value);
            }

            return value;
        }

        protected TValue DictionaryGetByKey2(TKey2 key2)
        {
            TValue value;

            lock (lockObject)
            {
                this.InnerDictionary2.TryGetValue(key2, out value);
            }

            return value;
        }

        protected void DictionarySetByKey1(TKey1 key1, TValue value)
        {
            lock (lockObject)
            {
                TKey2 key2 = (TKey2)this.key2ByKey1[key1];
                TValue oldValue = this.DictionaryGetByKey1(key1);

                this.OnSet(key1, key2, oldValue, value);
                this.InnerDictionary1[key1] = value;
                this.InnerDictionary2[key2] = value;
            }
        }

        protected void DictionarySetByKey2(TKey2 key2, TValue value)
        {
            lock (lockObject)
            {
                TKey1 key1 = (TKey1)this.key1ByKey2[key2];
                TValue oldValue = this.DictionaryGetByKey2(key2);

                this.OnSet(key1, key2, oldValue, value);
                this.InnerDictionary1[key1] = value;
                this.InnerDictionary2[key2] = value;
            }
        }

        protected void DictionaryClear()
        {
            lock (lockObject)
            {
                this.OnClear();
                this.InnerDictionary1.Clear();
                this.InnerDictionary2.Clear();
                this.key2ByKey1.Clear();
                this.key1ByKey2.Clear();
            }
        }

        protected virtual void OnAdd(TKey1 key1, TKey2 tkey2, TValue value)
        {
        }

        protected virtual void OnRemove(TKey1 key1, TKey2 key2)
        {
        }

        protected virtual void OnSet(TKey1 key1, TKey2 key2, TValue oldValue, TValue newValue)
        {
        }

        protected virtual void OnClear()
        {
        }

        #endregion |   Protected Methods   |

        #region |   Private Methods   |

        private void DictionaryRemove(TKey1 key1, TKey2 key2)
        {
            lock (lockObject)
            {
                this.key2ByKey1.Remove(key1);
                this.key1ByKey2.Remove(key2);

                this.InnerDictionary1.Remove(key1);
                this.InnerDictionary2.Remove(key2);
            }
        }

        #endregion |   Private Methods   |

        #region |   IEnumerable Interface   |

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerDictionary1.GetEnumerator();
        }

        #endregion |   IEnumerable Interface   |
    }
}
