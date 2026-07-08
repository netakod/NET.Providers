using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public interface ISequenceReader
	{
		object BaseReader { get; }
		bool CanRead { get; }
		long BytesConsumed { get; }

		byte ReadByte();

		[Obsolete("This method is is deprecated since Span concept replaces it.")]
		byte[] ReadByteArray(int count, out int offset);

		[Obsolete("This method is is deprecated since Span concept replaces it.")]
		int ReadToBuffer(byte[] buffer, int offset, int count);

		ReadOnlySpan<byte> ReadSpan(int count);

		void Advance(long count);

		ReadOnlySpanSegment<byte> AsSpanSequence();

  //      [Obsolete("This method is is deprecated since Span concept replaces it.")]
		List<ArraySegment<byte>> ToArraySegmentList();
		
		void WriteTo(IBufferWriter<byte> writer);
		object GetCurrentPositionToken();
		void SetCurrentPosition(object positionToken);


		//byte ReadAt(object positionToken);

		void ResetPosition();
	}
}
