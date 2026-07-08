using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class StreamSequenceReader : SequenceReaderBase, ISequenceReader
	{
		private Stream stream = null;
		private long byteaConsumed = 0;

		public StreamSequenceReader(Stream stream) => this.stream = stream;

		//public override long Position => this.stream.Position;
		//public override long Length => this.stream.Length;
		public override bool CanRead => this.stream.CanRead;
		public override long BytesConsumed => this.stream.Position;
		public override object BaseReader => this.stream;

		public override byte ReadByte()
		{
			this.byteaConsumed++;
			
			return unchecked((byte)this.stream.ReadByte());
		}

		public override int ReadToBuffer(byte[] buffer, int offset, int count)
		{
			int currentPosition = 0;
			int readLength;
			int bytesRead = 0;

			do
			{
				readLength = count - currentPosition;
				int n = this.stream.Read(buffer, currentPosition, readLength);

				if (n == 0)
					throw new EndOfStreamException("Stream reach the end but not all data has been read");

				currentPosition += n;
				bytesRead += n;
			}
			while (currentPosition < count);

			this.byteaConsumed += bytesRead;

			return bytesRead;
		}

        public override ReadOnlySpan<byte> ReadSpan(int count)
        {
            byte[] array = this.ReadByteArray(count, out int offset);

            return new ReadOnlySpan<byte>(array, offset, count);
        }

        //public override object GetCurrentPositionToken() => this.Position;

        //public override byte ReadAt(object positionToken)
        //{
        //	var currentPosition = this.Position;

        //	this.stream.Position = (int)positionToken;

        //	var result = this.ReadByte();

        //	this.stream.Position = currentPosition;

        //	return result;
        //}

        public override void ResetPosition()
		{
			this.stream.Position = 0;
			this.byteaConsumed = 0;
		}

		public byte[] GetBuffer()
		{
			byte[] result;

			if (this.stream is MemoryStream memoryStream)
			{
				result = memoryStream.GetBuffer();
			}
			else
			{
				memoryStream = new MemoryStream();

				stream.CopyTo(memoryStream);

				result = memoryStream.GetBuffer(); // Avoid using additional copy memoryStream.ToArray()
			}

			return result;
		}

		public override void Advance(long count) => this.stream.Position += count;

		public override object GetCurrentPositionToken() => this.stream.Position;

		public override void SetCurrentPosition(object positionToken) => this.stream.Position = (long)positionToken;

        public override List<ArraySegment<byte>> ToArraySegmentList() => new List<ArraySegment<byte>> { new ArraySegment<byte>(this.GetBuffer()) };

        public override ReadOnlySpanSegment<byte> AsSpanSequence() => new ReadOnlySpanSegment<byte>(this.GetBuffer());

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			writer.Write(this.GetBuffer());
		}

		public byte[] ToArray()
		{
			byte[] result;

			if (this.stream is MemoryStream memoryStream)
			{
				result = memoryStream.ToArray(); 
			}
			else
			{
				memoryStream = new MemoryStream();

				stream.CopyTo(memoryStream);

				result = memoryStream.ToArray(); 
			}

			return result;
		}
	}
}

