using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Simple;

namespace Simple.Collections
{
    /// <summary>
    /// Represents a collection of keys and object values and is usefull to store property object values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public class PropertyValueDictionary<TKey, TValue> : SimpleDictionary<TKey, TValue>, IDictionaryWithEvents<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionaryEvents<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private bool removeItemIfValueIsNull = true;
        
        #region |   Constructor(s) and Initialization   |

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public PropertyValueDictionary()
	    {
	    }

        /// <summary>
        ///  Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/> 
        ///  class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="SimpleDictionary&lt;TKey, TValue&gt;"/> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public PropertyValueDictionary(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/>
        /// class that wraps elements from the specified <see cref="T:System.Collections.Generic.Dictionary`2"></see>
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.IDictionary"></see> to wrap.</param>
        /// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
        public PropertyValueDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/>
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> for the type of the key.</param>
        public PropertyValueDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/> 
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
        public PropertyValueDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueDictionary&lt;TKey&gt;"/> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2"></see> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use
        /// when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see>
        /// for the type of the key.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public PropertyValueDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Properties   |

        //public bool RemoveItemIfValueIsNull
        //{
        //    get { return this.removeItemIfValueIsNull; }
        //    set { this.removeItemIfValueIsNull = value; }
        //}

        #endregion |   Public Properties   |

        #region |   Events   |

        public event RequesterPropertyValueChangeEventHandler<TKey, TValue> PropertyValueChange;

        #endregion |   Events   |

        #region |   Public Methods   |

        public TValue GetValue(TKey key)
        {
            return this.GetValue(key, default(TValue));
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            TValue value;
            
            if (!this.TryGetValue(key, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        public T GetValue<T>(TKey key)
        {
            return this.GetValue<T>(key, default(T));
        }

        public T GetValue<T>(TKey key, T defaultValue)
        {
            return this.GetValue<T>(key, () => defaultValue);
        }

        public T GetValue<T>(TKey key, Func<T> getDefaultValue)
        {
            if (this.ContainsKey(key))
            {
                object valueObject = this[key];
                T value = Conversion.TryChangeType<T>(valueObject, getDefaultValue());
                return value;
            }
            else
            {
                return getDefaultValue();
            }
        }

        public void SetValue(TKey key, TValue value)
        {
            this.SetValue(key, value, requester: null);
        }

        public void SetValue(TKey key, TValue value, object requester)
        {
            TValue oldValue = default(TValue);

            if (this.ContainsKey(key))
            {
                oldValue = this[key];
                this[key] = value;
            }
            else
            {
                this.Add(key, value);
            }

            if (!Comparison.IsEqual(value, oldValue))
            {
                this.OnPropertyValueChange(key, value, oldValue, requester);
                this.RaisePropertyValueChange(key, value, oldValue, requester);
			}
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected new void InternalDictionaryAdd(TKey key, TValue value)
        {
            if (value == null && this.removeItemIfValueIsNull)
                return;

            base.InternalDictionaryAdd(key, value);
        }

        protected new void InternalDictionarySet(TKey key, TValue value)
        {
            if (value == null && this.removeItemIfValueIsNull)
            {
                this.Remove(key);
            }
            else
            {
                base.InternalDictionarySet(key, value);
            }
        }

        protected virtual void OnPropertyValueChange(TKey key, TValue value, TValue oldValue, object requester)
        {
        }

        #endregion |   Protected Methods   |

        #region |   Private Methods   |

        private void RaisePropertyValueChange(TKey key, TValue value, TValue oldValue, object requester)
        {
            this.PropertyValueChange?.Invoke(this, new RequesterPropertyValueChangeEventArgs<TKey, TValue>(key, value, oldValue, requester));
        }

        #endregion |   Private Methods   |
}

    #region |   Delegates   |

    public delegate void RequesterPropertyValueChangeEventHandler<TKey, TValue>(object sender, RequesterPropertyValueChangeEventArgs<TKey, TValue> e);

    #endregion |   Delegates   |

    #region |   EventArgs Classes   |

    public class RequesterPropertyValueChangeEventArgs<TKey, TValue> : PropertyValueChangeEventArgs<TKey, TValue>
    {
        public RequesterPropertyValueChangeEventArgs(TKey key, TValue value, TValue oldValue, object requester)
            : base(key, value, oldValue)
        {
            this.Requester = requester;
        }
        public object Requester { get; private set; }
    }

    public class PropertyValueChangeEventArgs<TKey, TValue> : EventArgs
    {
        public PropertyValueChangeEventArgs(TKey key, TValue value, TValue oldValue)
        {
            this.Key = key;
            this.Value = value;
            this.OldValue = oldValue;
        }

        public TKey Key { get; private set; }
        public TValue Value { get; private set; }
        public TValue OldValue { get; private set; }
    }

    #endregion |   EventArgs Classes   |
}
