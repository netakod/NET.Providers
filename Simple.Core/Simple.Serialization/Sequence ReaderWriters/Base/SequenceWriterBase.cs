using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public abstract class SequenceWriterBase : ISequenceWriter
	{
		public abstract object BaseWriter { get; }

		public abstract long BytesWritten { get; }
		
		public abstract void WriteByte(byte value);
		
		public abstract void WriteByteArray(byte[] buffer, int offset, int count);
		public void WriteSpan(ReadOnlySpan<byte> span) => this.WriteSpan(span, span.Length);
        public abstract void WriteSpan(ReadOnlySpan<byte> span, int count);

		public void WriteSpanSegment(ReadOnlySpanSegment<byte> spanSegment) => this.WriteSpan(spanSegment.AsReadOnlySpan());

		public abstract void WriteSpanSegment(SpanSegment<byte> spanSegment);

		public void WriteSpanSequence(ReadOnlySpanSegment<byte> first)
		{
			var item = first;

			while (item != null)
			{
				this.WriteSpanSegment(item);
				item = item.Next;
			}
		}

		public void WriteSpanSequence(SpanSegment<byte> first)
		{
			var item = first;

			while (item != null)
			{
				this.WriteSpanSegment(item);
				item = item.Next;
			}
		}

		public abstract SpanSegment<byte> AsSpanSequence();


		//public abstract void WriteFromBuffer(byte[] buffer, long offset, long count);
		public abstract List<ArraySegment<byte>> ToArraySegmentList();

		public abstract void WriteTo(IBufferWriter<byte> writer);
		public abstract object GetCurrentPositionToken();

		public abstract void WriteAt(object positionToken, byte value);
	}
}
