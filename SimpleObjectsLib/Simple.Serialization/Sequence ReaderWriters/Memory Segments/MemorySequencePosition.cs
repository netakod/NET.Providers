using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct MemorySequencePosition<T>
	{
		public MemorySequencePosition(MemoryElement<T> segment, int position)
		{
			this.Segment = segment;
			this.Position = position;
		}

		public MemoryElement<T> Segment { get; private set; }
		public int Position { get; private set; }
	}
}
