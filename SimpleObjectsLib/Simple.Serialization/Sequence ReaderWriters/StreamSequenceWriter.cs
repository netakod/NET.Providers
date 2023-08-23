using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class StreamSequenceWriter : SequenceWriterBase, ISequenceWriter
	{
		private Stream stream;
		private long bytesWritten = 0;

		public StreamSequenceWriter(Stream stream) => this.stream = stream;

		public override object BaseWriter => this.stream;
		public override long BytesWritten => this.bytesWritten;

		public override void WriteByte(byte value)
		{
			this.stream.WriteByte(value);
			this.bytesWritten++;
		}

		public override void WriteByteArray(byte[] buffer, int offset, int count)
		{
			this.stream.Write(buffer, 0, buffer.Length);
			this.bytesWritten += buffer.LongLength;
		}

        public override void WriteSpan(ReadOnlySpan<byte> span, int count)
        {
            this.WriteByteArray(span.ToArray(), 0, count);
        }

		public override void WriteSpanSegment(SpanSegment<byte> spanSegment)
		{
			var item = spanSegment;

			while (item != null)
			{
				this.WriteSpan(spanSegment.AsSpan());
				item = item.Next;
			}
		}

		public override object GetCurrentPositionToken()
		{
			return this.stream.Position;
		}

		public override void WriteAt(object positionToken, byte value) => this.WriteAt((int)positionToken, value);

		public void WriteAt(byte value, long position)
		{
			var currentPosition = this.stream.Position;

			stream.Position = position;
			this.stream.WriteByte(value);
			stream.Position = currentPosition;
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

		public override List<ArraySegment<byte>> ToArraySegmentList()
		{
			return new List<ArraySegment<byte>> { new ArraySegment<byte>(this.GetBuffer()) };
		}

		public override SpanSegment<byte> AsSpanSequence() => new SpanSegment<byte>(this.GetBuffer());

		public override void WriteTo(IBufferWriter<byte> writer) => writer.Write(this.GetBuffer());

		public byte[] ToArray()
		{
			byte[] result;

			if (this.stream is MemoryStream memoryStream)
			{
				result = memoryStream.ToArray(); // GetBuffer();
			}
			else
			{
				memoryStream = new MemoryStream();

				stream.CopyTo(memoryStream);

				result = memoryStream.ToArray(); // GetBuffer(); // Avoid using additional copy memoryStream.ToArray()
			}

			return result;
		}
	}
}
