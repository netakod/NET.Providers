using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct SpanSegmentPosition<T>
	{
		public SpanSegmentPosition(ReadOnlySpanSegment<T> segment, int position)
		{
			this.Segment = segment;
			this.Position = position;
		}

		public ReadOnlySpanSegment<T> Segment { get; private set; }
		public int Position { get; private set; }
	}
}
