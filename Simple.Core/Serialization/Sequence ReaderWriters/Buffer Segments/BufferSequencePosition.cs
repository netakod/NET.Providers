using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct BufferSequencePosition<T>
	{
		public BufferSequencePosition(BufferSegment<T> segment, long position)
		{
			this.Segment = segment;
			this.Position = position;
		}

		public BufferSegment<T> Segment { get; private set; }
		public long Position { get; private set; }
	}
}
