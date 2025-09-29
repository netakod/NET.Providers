using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public abstract class SequenceReaderBase : ISequenceReader
	{
		public abstract object BaseReader { get; }
		
		public abstract bool CanRead { get; }

		public abstract long BytesConsumed { get; }

		public abstract byte ReadByte();

		public virtual byte[] ReadByteArray(int count, out int offset)
		{
			offset = 0;
			byte[] buffer = new byte[count];

			this.ReadToBuffer(buffer, offset, count);

			return buffer;
		}

		public abstract int ReadToBuffer(byte[] buffer, int offset, int count);
		
		public abstract void Advance(long count);

        public abstract object GetCurrentPositionToken();
        public abstract void SetCurrentPosition(object positionToken);


        public abstract ReadOnlySpan<byte> ReadSpan(int count);

		public abstract ReadOnlySpanSegment<byte> AsSpanSequence();

		public abstract List<ArraySegment<byte>> ToArraySegmentList();
		public abstract void WriteTo(IBufferWriter<byte> writer);

		//public abstract int ReadToBuffer(byte[] buffer, int offset, int count);

		public abstract void ResetPosition();
	}
}
