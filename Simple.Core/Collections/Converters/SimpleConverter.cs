using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public static class SimpleConverter
    {
        #region CONVERT
        /// <summary>
        /// Converts between generic ICollection interfaces
        /// from a base generic type to a derived type.
        /// Note that some items in the collection may not be of the derived type.
        /// </summary>
        /// <typeparam name="T">Derived type.</typeparam>
        /// <typeparam name="U">Base type.</typeparam>
        /// <param name="coll">Collection of objects.</param>
        public static ICollection<U> ConvertBaseToDerived<T, U>(ICollection<T> coll) where U : T
        {
            return new CollectionWrapperBase<T, U>(coll);
        }
        #endregion CONVERT

        /// <summary>
        /// Converts between generic ICollection interfaces
        /// from a derived generic type to a base type.
        /// </summary>
        /// <typeparam name="T">Derived type.</typeparam>
        /// <typeparam name="U">Base type.</typeparam>
        /// <param name="coll">Collection of objects.</param>
        public static ICollection<U> ConvertDerivedToBase<T, U>(ICollection<T> coll) where T : U
        {
            return new CollectionWrapperDerived<T, U>(coll);
        }
    }
}
