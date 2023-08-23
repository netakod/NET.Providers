using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    #region DERIVED TO BASE

    #region COLLECTION WRAPPER
    /// <summary>
    /// ICollection wrapper class.
    /// </summary>
    /// <typeparam name="T">Derived type.</typeparam>
    /// <typeparam name="U">Base type.</typeparam>
    public class CollectionWrapperDerived<T, U> : ICollection<U> where T : U
    {
        #region CONSTRUCTOR
        /// <summary>
        /// Constructs a derived type ICollection wrapper.
        /// </summary>
        /// <param name="collection">List of derived objects.</param>
        public CollectionWrapperDerived(ICollection<T> collection)
        {
            this.m_Coll = collection;
        }
        #endregion CONSTRUCTOR

        #region ADD
        /// <summary>
        /// Adds the item to this collection.
        /// </summary>
        /// <param name="item">Item to add. Ignored if null.</param>
        public void Add(U item)
        {
            T derivedItem = default(T);
            if (item != null && item is T)
            {
                derivedItem = (T)item;
                this.m_Coll.Add(derivedItem);
            }
        }
        #endregion ADD

        #region CLEAR
        /// <summary>
        /// Clears this collection.
        /// </summary>
        public void Clear()
        {
            this.m_Coll.Clear();
        }
        #endregion CLEAR

        #region COLLECTION
        /// <summary>
        /// Derived type collection.
        /// </summary>
        private ICollection<T> m_Coll;
        #endregion COLLECTION

        #region CONTAINS
        /// <summary>
        /// Gets whether the specified item is in this collection.
        /// </summary>
        /// <param name="item">Item to search for.</param>
        public bool Contains(U item)
        {
            bool contains = false;
            T derivedItem = default(T);
            if (item != null && item is T)
            {
                derivedItem = (T)item;
                contains = this.m_Coll.Contains(derivedItem);
            }
            return contains;
        }
        #endregion CONTAINS

        #region COPY TO
        /// <summary>
        /// Copies this collection to the specified array.
        /// </summary>
        /// <param name="array">Zero-based array into which to copy the items.</param>
        /// <param name="arrayIndex">Index in the array where to begin the copy.</param>
        public void CopyTo(U[] array, int arrayIndex)
        {
            foreach (T item in this.m_Coll)
            {
                array[arrayIndex++] = item;
            }
        }
        #endregion COPY TO

        #region COUNT
        /// <summary>
        /// Gets the number of items in this collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.m_Coll.Count;
            }
        }
        #endregion COUNT

        #region ENUMERATOR
        #region INTERFACE
        /// <summary>
        /// Used to satisfy the IEnumerable interface,
        /// but is essentially hidden by typesafe method below.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion INTERFACE

        #region TYPESAFE
        /// <summary>
        /// Returns a typesafe enumerator for this collection.
        /// Will not return null.
        /// </summary>
        public IEnumerator<U> GetEnumerator()
        {
            return new EnumeratorWrapper(this.m_Coll.GetEnumerator());
        }
        #endregion TYPESAFE
        #endregion ENUMERATOR

        #region ENUMERATOR WRAPPER
        /// <summary>
        /// Base type enumerator.
        /// </summary>
        private class EnumeratorWrapper : IEnumerator<U>
        {
            #region CONSTRUCTOR
            /// <summary>
            /// Constructs a base type enumerator wrapper.
            /// </summary>
            /// <param name="collection">List of derived objects.</param>
            public EnumeratorWrapper(IEnumerator<T> collection)
            {
                this.m_Coll = collection;
            }
            #endregion CONSTRUCTOR

            #region DISPOSE
            /// <summary>
            /// Disposes the collection.
            /// </summary>
            public void Dispose()
            {
                this.m_Coll.Dispose();
            }
            #endregion DISPOSE

            #region ENUMERATOR
            /// <summary>
            /// List of derived objects.
            /// </summary>
            private IEnumerator<T> m_Coll;
            #endregion ENUMERATOR

            #region CURRENT
            #region INTERFACE
            /// <summary>
            /// Used to satisfy the IEnumerator interface,
            /// but is essentially hidden by typesafe method below.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.m_Coll.Current;
                }
            }
            #endregion INTERFACE

            #region TYPESAFE
            /// <summary>
            /// Gets the current base object referenced by the enumerator.
            /// </summary>
            public U Current
            {
                get
                {
                    return this.m_Coll.Current;
                }
            }
            #endregion TYPESAFE
            #endregion CURRENT

            #region MOVE
            /// <summary>
            /// Advances the enumerator to the next element in the collection.
            /// </summary>
            public bool MoveNext()
            {
                return this.m_Coll.MoveNext();
            }
            #endregion MOVE

            #region RESET
            /// <summary>
            /// Resets the enumerator to its initial position.
            /// </summary>
            public void Reset()
            {
                this.m_Coll.Reset();
            }
            #endregion RESET
        }
        #endregion ENUMERATOR WRAPPER

        #region READ ONLY
        /// <summary>
        /// Gets whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this.m_Coll.IsReadOnly;
            }
        }
        #endregion READ ONLY

        #region REMOVE
        /// <summary>
        /// Removes the specified item from this collection.
        /// </summary>
        /// <param name="item">Item to remove. OK if null.</param>
        public bool Remove(U item)
        {
            bool removed = false;
            T derivedItem = default(T);
            if (item != null && item is T)
            {
                derivedItem = (T)item;
                removed = this.m_Coll.Remove(derivedItem);
            }
            return removed;
        }
        #endregion REMOVE
    }
    #endregion COLLECTION WRAPPER
    #endregion DERIVED TO BASE}
}