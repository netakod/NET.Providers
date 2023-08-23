using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.Drawing;

namespace Simple.Serialization
{
	public class SpanSequenceWriter : SequenceWriterBase, ISequenceWriter
	{
		//private long capacity;
		//private BufferSequence<byte> sequence;
		private SpanSegment<byte> first;
		private SpanSegment<byte> current;
		private int bufferSize;
		private int bytesWritten = 0;

		//private int currentPosition = 0;

		public const int DefaultCapacity = 1024;

		public SpanSequenceWriter()
			: this(DefaultCapacity)
		{
		}

		public SpanSequenceWriter(int capacity)
			: this(new SpanSegment<byte>(new byte[capacity]))
		{
		}

		public SpanSequenceWriter(SpanSegment<byte> first)
			: this(first, DefaultCapacity)
		{
		}

		public SpanSequenceWriter(SpanSegment<byte> first, int capacity)
		{
			//this.capacity = capacity;
			this.first = first;
			this.current = first;
			this.bufferSize = capacity;
		}

		//public long Capacity { get => this.capacity; set => this.capacity = value; }
		public SpanSegment<byte> First => this.first;
		public override object BaseWriter => this.first;
		public override long BytesWritten => this.bytesWritten;

		public override void WriteByte(byte value)
		{
			// TODO: Check whats wrong with that if we first join new segment, if required
			//
			// SOLVED!!! -> The reason is bitStrem writer that take position token before writing value.
			// 

			this.current[this.current.Count++] = value;
			this.bytesWritten++;

            if (this.current.Count == this.current.Length)
                this.JoinNewSegment();

            //this.current[this.current.Position] = value;
            //this.current.Count++;
            //this.bytesWritten++;

            //if (this.current.Position == this.current.Buffer.Length)
            //	this.JoinNewSegment();
        }

        public override void WriteByteArray(byte[] buffer, int offset, int count)
		{
			this.WriteSpan(new Span<byte>(buffer, offset, count));
        }

		public override void WriteSpan(ReadOnlySpan<byte> span, int count)
        {
			long currentLeft = this.current.Length - this.current.Count;

			if (span.Length <= currentLeft)
			{
				for (int i = 0; i < count; i++)
					this.current[this.current.Count++] = span[i];
			}
			else
			{
				this.current.Next = new SpanSegment<byte>(span);
				this.current = this.current.Next;
			}

			this.bytesWritten += count;
		}

		public override void WriteSpanSegment(SpanSegment<byte> spanSegment)
		{
			long currentLeft = this.current.Length - this.current.Count;

			if (spanSegment.Count <= currentLeft)
			{
				for (int i = 0; i < spanSegment.Count; i++)
					this.current[this.current.Count++] = spanSegment[i];
			}
			else
			{
				this.current.Next = spanSegment;
				this.current = spanSegment;
			}

			this.bytesWritten += spanSegment.Count;
		}

		public override object GetCurrentPositionToken() => new SpanSegmentPosition<byte>(this.current, this.current.Count);

		public override void WriteAt(object positionToken, byte value)
		{
			SpanSegmentPosition<byte> sequencePosition = (SpanSegmentPosition<byte>)positionToken;

			((SpanSegment<byte>)sequencePosition.Segment)[sequencePosition.Position] = value;
		}

		public override List<ArraySegment<byte>> ToArraySegmentList()
		{
			List<ArraySegment<byte>> result = new List<ArraySegment<byte>>();
			var segment = this.first;

			while (segment != null)
			{
				result.Add(new ArraySegment<byte>(segment.AsSpan().ToArray(), 0, (int)segment.Count));
				segment = segment.Next;
			}

			return result;
		}

		public override SpanSegment<byte> AsSpanSequence() => this.first;

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			var segment = this.first;

			while (segment != null)
			{
				if (segment.Count > 0)
					writer.Write(segment.AsReadOnlySpan());
				
				segment = segment.Next;
			}
		}

		// TODO: Try to avoid this method if possible (avoiding coping the data)
		//public byte[] ToArray() => BufferSequenceReader.ToArray(this.first); // TODO: Make as extension method. CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

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
		public void JoinSegment(byte[] buffer, int offset, int count) => this.JoinSegment(new SpanSegment<byte>(buffer, offset, count));

		public void JoinSegment(SpanSegment<byte> segment)
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

		private void JoinNewSegment(int minimumArraySize = DefaultCapacity)
		{
			this.bufferSize *= 2;

			if (this.bufferSize < minimumArraySize)
				this.bufferSize = minimumArraySize;

            //var last = BufferSequenceReader.FindLast(this.current);
            var newSegment = new SpanSegment<byte>(new byte[this.bufferSize]);

			this.current.Next = newSegment;
			this.current = newSegment;
		}

		//private SpanSegment<byte> FindSegment(long position, out long segmentOffset) // position is global position
		//{
		//	var segment = this.first; 
		//	long currentLength = segment.Length;

		//	while (currentLength < position)
		//	{
		//		segment = segment.Next;
		//		currentLength += segment.Length;
		//	}

		//	segmentOffset = currentLength - position;
			
		//	return segment;
		//}
	}
}

