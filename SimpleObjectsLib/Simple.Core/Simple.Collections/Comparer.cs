using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Collections
{
	public class Comparer<T> : System.Collections.Generic.Comparer<T>
	{
		private readonly Comparison<T> _compareFunction;

		public Comparer(Comparison<T> comparison)
		{
			if (comparison == null) throw new ArgumentNullException("comparison");
			_compareFunction = comparison;
		}

		public override int Compare(T arg1, T arg2)
		{
			return _compareFunction(arg1, arg2);
		}
	}
}