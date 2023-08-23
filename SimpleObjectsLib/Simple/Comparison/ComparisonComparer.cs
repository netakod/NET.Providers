using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	/// <summary>
	/// Wraps a generic Comparison<T> delegate in an IComparer to make it easy to use a lambda expression for methods that take an <see cref="IComparer"/> or <see cref="IComparer{T}"/>
	/// </summary>
	/// <typeparam name="T">Comparison element type.</typeparam>
	public class ComparisonComparer<T> : IComparer<T>, IComparer
	{
		private readonly Comparison<T> comparison;

		public ComparisonComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		public int Compare(object o1, object o2)
		{
			return comparison((T)o1, (T)o2);
		}
	}
}
