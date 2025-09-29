using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct ArraySequencePosition<T>
	{
		public ArraySequencePosition(ref T[] array, long position)
		{
			this.Array = array;
			this.Position = position;
		}

		public T[] Array { get; private set; }
		public long Position { get; private set; }
	}
}
