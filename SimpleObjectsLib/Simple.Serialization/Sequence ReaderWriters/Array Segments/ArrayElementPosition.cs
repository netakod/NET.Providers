using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct ArrayElementPosition<T>
	{
		public ArrayElementPosition(ArrayElement<T> element, int position)
		{
			this.Element = element;
			this.Position = position;
		}

		public ArrayElement<T> Element { get; private set; }
		public int Position { get; private set; }
	}
}
