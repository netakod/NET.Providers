
using System.Collections.Generic;

namespace Simple.Collections
{
	public static class CollectionExtensions
	{
		public static IList<T> ToLazyList<T>(this IEnumerable<T> list)
		{
			return new LazyList<T>(list);
		}
	}

}
