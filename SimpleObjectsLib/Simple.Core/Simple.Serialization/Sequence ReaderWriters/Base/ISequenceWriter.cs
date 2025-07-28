using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public interface ISequenceWriter
	{
		object BaseWriter { get; }

		long BytesWritten { get; }
		void WriteByte(byte value);

		[Obsolete("This method is is deprecated since Span concept replaces it.")]
		void WriteByteArray(byte[] buffer, int offset, int count);

		void WriteSpan(ReadOnlySpan<byte> span, int count);
		void WriteSpanSegment(ReadOnlySpanSegment<byte> spanSegment);

		void WriteSpanSequence(ReadOnlySpanSegment<byte> first);
		void WriteSpanSequence(SpanSegment<byte> first);

		//SpanSegment<byte> AsSpanSequence();

  //      [Obsolete("This method is is deprecated since Span concept replaces it.")]
		List<ArraySegment<byte>> ToArraySegmentList();

		void WriteTo(IBufferWriter<byte> writer);

		object GetCurrentPositionToken();

		void WriteAt(object positionToken, byte value);
	}
}
