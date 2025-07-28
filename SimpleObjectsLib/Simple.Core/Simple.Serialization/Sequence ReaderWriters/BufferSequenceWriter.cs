using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class BufferSequenceWriter : SequenceWriterBase, ISequenceWriter
	{
		//private long capacity;
		//private BufferSequence<byte> sequence;
		private BufferSegment<byte> first;
		private BufferSegment<byte> current;
		private long bufferSize;
		private long bytesWritten = 0;
		//private int currentPosition = 0;

		public const long DefaultCapacity = BufferSegment<byte>.DefaultBufferSize;

		public BufferSequenceWriter()
			: this(DefaultCapacity)
		{
		}

		public BufferSequenceWriter(long capacity)
			: this(new BufferSegment<byte>(capacity))
		{
		}

		public BufferSequenceWriter(BufferSegment<byte> first)
			: this(first, DefaultCapacity)
		{
		}

		public BufferSequenceWriter(BufferSegment<byte> first, long capacity)
		{
			//this.capacity = capacity;
			this.first = first;
			this.current = first;
			this.bufferSize = capacity;
		}

		//public long Capacity { get => this.capacity; set => this.capacity = value; }
		public BufferSegment<byte> First => this.first;
		public override object BaseWriter => this.first;
		public override long BytesWritten => this.bytesWritten;

		public override void WriteByte(byte value)
		{
			// TODO: Check whats wrong with that if we first join new segment, if required
			//
			// SOLVED!!! -> The reason is bitStrem writer that take position token before writing value.
			// 
			//if (this.current.Count == this.current.Buffer.Length)
			//	this.JoinNewSegment();

			//this.current[this.current.Position] = value;
			//this.current.Count++;
			//this.bytesWritten++;

			this.current[this.current.Position] = value;
			this.current.Count++;
			this.bytesWritten++;

			if (this.current.Position == this.current.Buffer.Length)
				this.JoinNewSegment();
		}

		public override void WriteByteArray(byte[] buffer, int offset, int count)
		{
			//this.JoinSegment(buffer, offset, count);

			//for (long i = 0; i < buffer.LongLength; i++)
			//	this.WriteByte(buffer[i]);

			// TODO: CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

			long currentLeft = this.current.Length - this.current.Position;

			if (count <= currentLeft)
			{
				Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.current.Buffer, destinationIndex: this.current.Position, length: count);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, count: buffer.Length);

				this.current.Count += count;
			}
			else // Must split into two pices
			{
				// First part is rest of current
				if (currentLeft > 0)
					Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.current.Buffer, destinationIndex: this.current.Position, length: currentLeft);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, (int)count);

				this.current.Count = this.current.Length; // this.current.Count = this.current.Length

				long rest = count - currentLeft;
				
				this.JoinNewSegment(minimumBufferSize: rest);

				Array.Copy(sourceArray: buffer, sourceIndex: offset + currentLeft, destinationArray: this.current.Buffer, destinationIndex: 0, length: rest);
				//Buffer.BlockCopy(src: buffer, srcOffset: (int)count, dst: this.current.Buffer, dstOffset: 0, count: (int)rest);

				this.current.Count = rest;
			}

			this.bytesWritten += count;
        }

        public override void WriteSpan(ReadOnlySpan<byte> span, int count)
        {
			this.WriteByteArray(span.ToArray(), 0, count);
		}

        public override void WriteSpanSegment(SpanSegment<byte> spanSegment)
        {
            this.WriteByteArray(spanSegment.ToArray(), 0, spanSegment.Count);
        }

        public override object GetCurrentPositionToken() => new BufferSequencePosition<byte>(this.current, this.current.Offset + this.current.Count);

		public override void WriteAt(object positionToken, byte value)
		{
			BufferSequencePosition<byte> sequencePosition = (BufferSequencePosition<byte>)positionToken;

			sequencePosition.Segment.Buffer[sequencePosition.Position] = value;
		}

		public override List<ArraySegment<byte>> ToArraySegmentList()
		{
			List<ArraySegment<byte>> result = new List<ArraySegment<byte>>();
			var segment = this.first;

			while (segment != null)
			{
				result.Add(new ArraySegment<byte>(segment.Buffer, (int)segment.Offset, (int)segment.Count));
				segment = segment.Next;
			}

			return result;
		}

		public override SpanSegment<byte> AsSpanSequence()
		{
			if (this.first.Count == 0)
				return SpanSegment<byte>.Empty;

			SpanSegment<byte> first = new SpanSegment<byte>(this.first.Buffer, (int)this.first.Offset, (int)this.first.Count);
			SpanSegment<byte> current = first;
			var item = this.first.Next;

			while (item != null)
			{
				current.Next = new SpanSegment<byte>(item.Buffer, (int)item.Offset, (int)item.Count);
				current = current.Next;
				item = item.Next;
			}

			return first;
		}

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			var segment = this.first;

			while (segment != null)
			{
				if (segment.Count > 0)
					writer.Write(new ReadOnlySpan<byte>(segment.Buffer, (int)segment.Offset, (int)segment.Count));
				
				segment = segment.Next;
			}
		}

		// TODO: Try to avoid this method if possible (avoiding coping the data)
		public byte[] ToArray() => BufferSequenceReader.ToArray(this.first); // TODO: Make as extension method. CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

		//public override byte[] ToArray()
		//{
		//	var result = new byte[this.Length];
		//	var item = this.Sequence.First;
		//	int offset = 0;

		//	while (item != null)
		//	{
		//		int itemCount = item.IsLast ? (int)this.Sequence.CountOfLast : item.Buffer.Count();

		//		Array.Copy(sourceArray: item.Buffer, sourceIndex: 0, destinationArray: result, destinationIndex: offset, length: itemCount);

		//		offset += itemCount;
		//		item = item.Next;
		//	}

		//	return result;
		//}

		//private void CurrentSegmentIsFullFilled() => this.JoinNewSegment();

		public void JoinSegment(byte[] buffer) => this.JoinSegment(buffer, 0, buffer.Length);
		public void JoinSegment(byte[] buffer, int offset, int count) => this.JoinSegment(new BufferSegment<byte>(buffer, offset, count));

		public void JoinSegment(BufferSegment<byte> segment)
		{
			// TODO:

			// This cannot be satisfied since when new contains no data.
			// This current should not be empty <- this will be brake chaing. Current must havae any data always!!!!

			if (this.current.Count > 0)
				this.current.Next = segment;
			else
				this.current = segment;

			var item = segment;

			while (item.Next != null)
			{
                this.bytesWritten += item.Count;
				item = item.Next;
            }

            this.current = item;
			this.bytesWritten += item.Count;
		}

		private void JoinNewSegment(long minimumBufferSize = DefaultCapacity)
		{
			this.bufferSize *= 2;

			if (this.bufferSize < minimumBufferSize)
				this.bufferSize = minimumBufferSize;

			//var last = BufferSequenceReader.FindLast(this.current);

            var newSegment = new BufferSegment<byte>(this.bufferSize);

			this.current.Next = newSegment;
			this.current = newSegment;
		}

		private BufferSegment<byte> FindSegment(long position, out long segmentOffset) // position is global position
		{
			var segment = this.first; 
			long currentLength = segment.Buffer.Length;

			while (currentLength < position)
			{
				segment = segment.Next;
				currentLength += segment.Buffer.Length;
			}

			segmentOffset = currentLength - position;
			
			return segment;
		}
	}
}

