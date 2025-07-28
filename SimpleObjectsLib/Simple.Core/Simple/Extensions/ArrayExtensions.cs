using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class ArrayExtensions
	{
		/// <summary>
		/// Create a copy of the ArraySegment
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="segment">ArraySegment that is abbout to be copied.</param>
		/// <returns>An ArraySegment copy</returns>
		public static ArraySegment<T> CreateCopy<T>(this ArraySegment<T> segment)
		{
			return new ArraySegment<T>(segment.ToArray(), segment.Offset, segment.Count);
		}

		public static bool IsEqual(this byte[]? buffer, byte[]? comparand)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(buffer, comparand))
				return true;

			// If one is null, but not both, return false.
			if ((object?)buffer == null ^ (object?)comparand == null)
				return false;

			if (buffer.Length != comparand.Length)
				return false;

			for (int i = 0; i < buffer.Length; i++)
				if (buffer[i] != comparand[i])
					return false;

			return true;
		}

		public static T? FindFirst<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
			where T : class
		{
			foreach (T item in collection)
				if (predicate(item))
					return item;

			return default;
		}

		//public static bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> predicate) where T : class => FindFirst(collection, predicate) != null;
	}
}
