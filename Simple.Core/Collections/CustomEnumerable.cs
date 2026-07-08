using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom Enumerable interface for the specified T type. Input objectElements mus be custable to T object type.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerator.</typeparam>
    public class CustomEnumerable<T> : IEnumerable<T>, IEnumerable
    {
        protected object lockObject = new object();
        private IEnumerable originalCollection = null;
        //private CustomEnumerator<T> customEnumerator = null;

		/// <summary>
		/// Creates custom IEnumerable interface for the specified T type. Input objectElements mus be custable to T object type.
		/// </summary>
		/// <param name="enumerableToWrap"></param>
		public CustomEnumerable(IEnumerable<T> enumerableToWrap)
		{
			this.originalCollection = enumerableToWrap;
		}

		/// <summary>
		/// Creates custom IEnumerable interface for the specified T type. Input objectElements mus be custable to T object type.
		/// </summary>
		/// <param name="enumerableToWrap"></param>
		public CustomEnumerable(IEnumerable enumerableToWrap)
        {
            this.originalCollection = enumerableToWrap;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator T that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
			foreach (T item in this.originalCollection)
				yield return item;

			//return this.CustomEnumerator; 
        }

        /// <summary>
        /// Gets the origanal enumerator wrapper.
        /// </summary>
        protected IEnumerable OriginalCollection
        {
            get { return this.originalCollection; }
        }

     //   /// <summary>
     //   /// Gets an anumerator wrapper around the IEnumerable input.
     //   /// </summary>
     //   protected IEnumerator<T> CustomEnumerator
     //   {
     //       get
     //       {
     //           lock (lockObject)
     //           {
					//if (this.customEnumerator == null)
     //               {
     //                   IEnumerator enumerator = this.OriginalCollection.GetEnumerator();
     //                   this.customEnumerator = new CustomEnumerator<T>(enumerator);
     //               }
     //               else
     //               {
					//	this.customEnumerator.Reset();
     //               }

     //               return this.customEnumerator;
     //           }
     //       }
     //   }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
			return this.GetEnumerator();

			//foreach (object item in this.originalCollection)
			//	yield return item;

			//return this.CustomEnumerator as IEnumerator;
        }
    }
}
