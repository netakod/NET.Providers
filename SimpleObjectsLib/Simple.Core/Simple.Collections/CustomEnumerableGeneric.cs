using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom Enumerable interface for the specified T type. Input enumerableToWrap elements must be custable to U object type.
    /// </summary>
    /// <typeparam name="U">The type of the elements in the enumerator.</typeparam>
    /// <typeparam name="U">Generic type U to convert to.</typeparam>
    public class CustomEnumerable<T, U> : IEnumerable<U>, IEnumerable
        where U : T
    {
        private IEnumerable<T> originalCollection = null;
        private CustomEnumerator<T, U> customEnumerator = null;

        /// <summary>
        /// Creates custom IEnumerable interface for the specified T type. Input objectElements mus be custable to U object type.
        /// </summary>
        /// <param name="enumerableToWrap"></param>
        public CustomEnumerable(IEnumerable<T> enumerableToWrap)
        {
            this.originalCollection = enumerableToWrap;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator T that can be used to iterate through the collection.</returns>
        public IEnumerator<U> GetEnumerator()
        {
            return this.CustomEnumerator;
        }

        /// <summary>
        /// Gets the origanal enumerator wrapper.
        /// </summary>
        protected IEnumerable<T> OriginalCollection
        {
            get { return this.originalCollection; }
        }

        /// <summary>
        /// Gets an anumerator wrapper around the IEnumerable input.
        /// </summary>
        private IEnumerator<U> CustomEnumerator
        {
            get
            {
                if (this.customEnumerator == null)
                {
                    IEnumerator<T> enumerator = this.OriginalCollection.GetEnumerator();
                    this.customEnumerator = new CustomEnumerator<T, U>(enumerator);
                }
                else
                {
                    this.customEnumerator.Reset();
                }

                return this.customEnumerator;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.CustomEnumerator as IEnumerator;
        }
    }
}
