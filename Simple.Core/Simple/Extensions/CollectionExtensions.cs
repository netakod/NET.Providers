using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simple
{
    public static class CollectionExtensions
    {

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (T item in @this)
                action(item);
        }

        /// <summary>
        /// Sort a generic collection in a place. 
        /// Usage example: 
        /// 
        /// IList<string> iList = new [] { "Carlton", "Alison", "Bob", "Eric", "David" };
        /// iList.Sort((s1, s2) => s1.Length.CompareTo(s2.Length)); // Sort in-place, by string length
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        public static void Sort<T>(this IList<T> list)
        {
            list.OrderBy(i => i);
        }

        /// <summary>
        /// Sort a generic collection in a place. 
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        /// <param name="comparison">The comparison method.</param>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
		}

		// Convenience method on IEnumerable<T> to allow passing of a
		// Comparison<T> delegate to the OrderBy method.
		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
		{
			return list.OrderBy(t => t, new ComparisonComparer<T>(comparison));
		}

		public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
		{
			T temp = list[newIndex];

			list[newIndex] = list[oldIndex];
			list[oldIndex] = temp;
		}

        public static T? FindFirst<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            where T : struct
        {
            foreach (T item in collection)
                if (predicate(item))
                    return item;

            return default;
        }

        //public static bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> predicate) where T : struct => FindFirst(collection, predicate) != null;

		/// <summary>
		/// Checks whether a collection is the same as another collection
		/// </summary>
		/// <param name="value">The current instance object</param>
		/// <param name="compareList">The collection to compare with</param>
		/// <param name="comparer">The comparer object to use to compare each item in the collection.  If null uses EqualityComparer(T).Default</param>
		/// <returns>True if the two collections contain all the same items in the same order</returns>
		internal static bool IsEqualTo<TSource>(this IEnumerable<TSource>? value, IEnumerable<TSource>? compareList, IEqualityComparer<TSource>? comparer = null)
        {
            if (value == compareList)
                return true;
            
            if (value == null || compareList == null)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            var enumerator1 = value.GetEnumerator();
            var enumerator2 = compareList.GetEnumerator();

            var enum1HasValue = enumerator1.MoveNext();
            var enum2HasValue = enumerator2.MoveNext();

            try
            {
                while (enum1HasValue && enum2HasValue)
                {
                    if (!comparer.Equals(enumerator1.Current, enumerator2.Current))
                        return false;

                    enum1HasValue = enumerator1.MoveNext();
                    enum2HasValue = enumerator2.MoveNext();
                }

                return !(enum1HasValue || enum2HasValue);
            }
            finally
            {
                enumerator1.Dispose();
                enumerator2.Dispose();
            }
        }
    }
}
