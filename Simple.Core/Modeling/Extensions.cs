using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
    public static class Extensions
    {

		public static bool TryGetPropertyValue(this IEnumerable<PropertyIndexValuePair> collection, int propertyIndex, out object? propertyValue)
		{
			int index = collection.IndexOf(propertyIndex);

			if (index >= 0)
			{
				propertyValue = collection.ElementAt(index).PropertyValue;

				return true;
			}

			propertyValue = null;

			return false;
		}

		public static bool ContainsPropertyValue(this IEnumerable<PropertyIndexValuePair> collection, int propertyIndex)
		{
			return collection.IndexOf(propertyIndex) >= 0;
		}

		public static int IndexOf(this IEnumerable<PropertyIndexValuePair> collection, int propertyIndex)
		{
			for (int i = 0; i < collection.Count(); i++)
				if (collection.ElementAt(i).PropertyIndex == propertyIndex)
					return i;

			return -1;
		}
	}
}
