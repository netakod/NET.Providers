using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Terminal
{
	public static class Extensions
	{
		public static void AddRange<T>(this List<T> list, params T[] items)
		{
			if (items.Length > 0)
				list.AddRange(items);
		}
	}
}
