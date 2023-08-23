using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Collections;

namespace Simple.Modeling
{
    public class ModelCollection<TModel> : IList<TModel>, ICollection<TModel>, ICollectionEvents<TModel>, IEnumerable<TModel>, IList, ICollection, IEnumerable
		where TModel : ModelElement
    {
        #region |   Constructor(s) and Initialization   |

        public ModelCollection()
        {
            this.InnerList = new SimpleListHelper<TModel>();
            //this.Initialize();
        }

        public ModelCollection(IList<TModel> collection)
        {
            if (collection != null)
            {
                this.InnerList = new SimpleListHelper<TModel>(collection);
            }
            else
            {
                throw new ArgumentNullException("collection is null");
            }

            //this.Initialize();
        }

        #endregion |   Constructor(s) and Initialization   |

        #region |   Public Events   |

        public event CollectionActionEventHandler<TModel> BeforeInsert;
        public event CollectionActionEventHandler<TModel> AfterInsert;
        public event CollectionActionEventHandler<TModel> BeforeRemove;
        public event CollectionActionEventHandler<TModel> AfterRemove;
        public event CollectionActionOldValueEventHandler<TModel> BeforeSet;
        public event CollectionActionOldValueEventHandler<TModel> AfterSet;
        public event EventHandler BeforeClear;
        public event EventHandler AfterClear;
        public event CountChangeEventHandler CountChange;

        #endregion |   Public Events   |

        #region |   Public Properties   |

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.  -or- index is equal to or greater than <see cref="ModelCollection&lt;T&gt;"/>.Count.</exception>
        /// <exception cref="System.NotSupportedException">the property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public TModel this[int index]
        {
            get { return this.InnerList[index]; }
            set { this.InnerList[index] = value; }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the collection.</returns>
        public int Count
        {
            get { return this.InnerList.Count; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ModelCollection&lt;T&gt;"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.InnerList.IsReadOnly; }
            set { this.InnerList.IsReadOnly = value; }
        }

        public object Owner { get; set; }

        #endregion |   Public Properties   |

        #region |   Protected Properties   |

        protected SimpleListHelper<TModel> InnerList { get; private set; }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="SimpleList&lt;T&gt;"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="SimpleList&lt;T&gt;"/>. 
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <exception cref="System.ArgumentNullException">collection is null.</exception>
        public void AddRange(IEnumerable<TModel> collection)
        {
            this.InnerList.AddRange(collection);
        }
        
        /// <summary>
        /// Returns a read-only <see cref="ModelCollection&lt;T&gt;"/> wrapper for the current collection.
        /// </summary>
        /// <returns>A <see cref="ModelCollection&lt;T&gt;"/> that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.IList`1"></see>.</returns>
        public ReadOnlyList<TModel> AsReadOnly()
        {
            return this.InnerList.AsReadOnly();
        }

        /// <summary>
        /// Return custom collection that wrap current colection and cast elements to U type parameter. T elements mast be castable to U type.
        /// </summary>
        /// <typeparam name="U">Customized type. </typeparam>
        /// <returns></returns>
        public CustomList<U> AsCustom<U>() where U : IModelElement
        {
            return this.InnerList.AsCustom<U>();
        }

        /// <summary>
        /// Determines whether the <see cref="ModelCollection&lt;T&gt;"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ModelCollection&lt;T&gt;"/>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.IList`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(TModel item)
        {
            return this.InnerList.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ModelCollection&lt;T&gt;"/> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="ModelCollection&lt;T&gt;"/>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of 
        /// elements in the source <see cref="ModelCollection&lt;T&gt;"/> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T 
        /// cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(TModel[] array, int arrayIndex)
        {
            this.InnerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="CustomList&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to locate in the <see cref="ModelCollection&lt;T&gt;"/>. The item can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="ModelCollection&lt;T&gt;"/>, if found; otherwise, -1.</returns>
        public int IndexOf(TModel item)
        {
            return this.InnerList.IndexOf(item);

        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
        public IEnumerator<TModel> GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        #endregion |   Public Methods   |

        //#region |   Protected Overrided Methods   |

        //protected override void OnSetOwner()
        //{
        //    base.OnSetOwner();

        //    foreach (Model model in this)
        //        model.Owner = this;
        //}

        //#endregion |   Protected Overrided Methods   |

        #region |   Protected Virtual Methods   |

        protected virtual void InnerList_BeforeInsert(object sender, CollectionActionEventArgs<TModel> e)
        {
            //this.SetOwnerToModel(e.Value);

            if (this.BeforeInsert != null)
                this.BeforeInsert(this, new CollectionActionEventArgs<TModel>(e.Index, e.Value));
        }

        protected virtual void InnerList_AfterInsert(object sender, CollectionActionEventArgs<TModel> e)
        {
            if (this.AfterInsert != null)
                this.AfterInsert(this, new CollectionActionEventArgs<TModel>(e.Index, e.Value));
        }

        protected virtual void InnerList_BeforeRemove(object sender, CollectionActionEventArgs<TModel> e)
        {
            if (this.BeforeRemove != null)
                this.BeforeRemove(this, new CollectionActionEventArgs<TModel>(e.Index, e.Value));

        }

        protected virtual void InnerList_AfterRemove(object sender, CollectionActionEventArgs<TModel> e)
        {
            if (this.AfterRemove != null)
                this.AfterRemove(this, new CollectionActionEventArgs<TModel>(e.Index, e.Value));
        }

        protected virtual void InnerList_BeforeSet(object sender, CollectionActionOldValueEventArgs<TModel> e)
        {
            //this.SetOwnerToModel(e.Value);

            if (this.BeforeSet != null)
                this.BeforeSet(this, new CollectionActionOldValueEventArgs<TModel>(e.Index, e.Value, e.OldValue));
        }

        protected virtual void InnerList_AfterSet(object sender, CollectionActionOldValueEventArgs<TModel> e)
        {
            if (this.AfterSet != null)
                this.AfterSet(this, new CollectionActionOldValueEventArgs<TModel>(e.Index, e.Value, e.OldValue));
        }

        protected virtual void InnerList_BeforeClear(object sender, EventArgs e)
        {
            if (this.BeforeClear != null)
                this.BeforeClear(this, new EventArgs());
        }

        protected virtual void InnerList_AfterClear(object sender, EventArgs e)
        {
            if (this.AfterClear != null)
                this.AfterClear(this, new EventArgs());
        }

        protected virtual void InnerList_CountChange(object sender, CountChangeEventArgs e)
        {
            if (this.CountChange != null)
                this.CountChange(this, new CountChangeEventArgs(e.Count, e.OldCount));
        }

        #endregion |   Protected Virtual Methods   |

        //#region |   Private Methods   |

        //private void Initialize()
        //{
        //    this.InnerList.BeforeSet += new CollectionActionOldValueEventHandler<TModel>(InnerList_BeforeSet);
        //    this.InnerList.AfterSet += new CollectionActionOldValueEventHandler<TModel>(InnerList_AfterSet);
        //    this.InnerList.BeforeInsert += new CollectionActionEventHandler<TModel>(InnerList_BeforeInsert);
        //    this.InnerList.AfterInsert += new CollectionActionEventHandler<TModel>(InnerList_AfterInsert);
        //    this.InnerList.BeforeRemove += new CollectionActionEventHandler<TModel>(InnerList_BeforeRemove);
        //    this.InnerList.AfterRemove += new CollectionActionEventHandler<TModel>(InnerList_AfterRemove);
        //    this.InnerList.BeforeClear += new EventHandler(InnerList_BeforeClear);
        //    this.InnerList.AfterClear += new EventHandler(InnerList_AfterClear);
        //    this.InnerList.CountChange += new CountChangeEventHandler(InnerList_CountChange);
        //}

        //#endregion |   Private Methods   |

        #region |   IList<T> Interface   |

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList`1"></see> has a fixed size.</exception>
        public void Insert(int index, TModel item)
        {
            this.InnerList.ListInsert(index, item);
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
            this.InnerList.ListRemoveAt(index);
        }

        #endregion |   IList<T> Interface   |

        #region |   ICollection<T> Interface   |

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public void Add(TModel item)
        {
            this.InnerList.Add(item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam> 
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. 
        ///   This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.ICollection`1"></see> has a fixed size.</exception>
        public bool Remove(TModel item)
        {
            return this.InnerList.Remove(item);
        }

        /// <summary>
        /// Removes all items from the System.Collections.Generic.ICollection<T>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.ICollection<T> is read-only.</exception>
        public void Clear()
        {
            this.InnerList.Clear();
        }

        #endregion |   ICollection<T> Interface   |

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
            get { return this.InnerList[index]; }
            set { (this.InnerList as IList)[index] = value; }
        }

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to add to the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="T:System.Collections.Generic.IList"></see> has a fixed size.</exception>
        int IList.Add(object value)
        {
            return (this.InnerList as IList).Add(value);
        }

        /// <summary>
        /// Removes all items from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.</exception>
        void IList.Clear()
        {
            this.InnerList.Clear();
        }

        /// <summary>Determines whether the <see cref="System.Collections.IList"></see> contains a specific value.</summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>true if the System.Object is found in the <see cref="System.Collections.IList"></see>; otherwise, false.</returns>
        bool IList.Contains(object value)
        {
            return (this.InnerList as IList).Contains(value);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="value">The System.Object to locate in the <see cref="System.Collections.IList"></see>.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            return (this.InnerList as IList).IndexOf(value);
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
            (this.InnerList as IList).Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.IList"></see>.
        /// </summary>
        /// <param name="item">The System.Object to remove from the <see cref="System.Collections.IList"></see>.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.Remove(object item)
        {
            (this.InnerList as IList).Remove(item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"></see> is read-only.-or- The <see cref="System.Collections.IList"></see> has a fixed size.</exception>
        void IList.RemoveAt(int index)
        {
            this.InnerList.RemoveAt(index);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.IList"></see> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="System.Collections.IList"></see> has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize
        {
            get { return (this.InnerList as IList).IsFixedSize; }
        }

        #endregion |   IList Interface   |

        #region |   ICollection Interface   |

        /// <summary>Gets a value indicating whether access to the <see cref="System.Collections.ICollection"></see> is synchronized (thread safe).</summary>
        /// <returns>true if access to the System.Collections.ICollection is synchronized (thread safe); otherwise, false.</returns>
        bool ICollection.IsSynchronized
        {
            get { return (this.InnerList as ICollection).IsSynchronized; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"></see>.</returns>
        object ICollection.SyncRoot
        {
            get { return (this.InnerList as ICollection).SyncRoot; }
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
            (this.InnerList as ICollection).CopyTo(array, index);
        }

        #endregion |   ICollection Interface   |
        
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
}