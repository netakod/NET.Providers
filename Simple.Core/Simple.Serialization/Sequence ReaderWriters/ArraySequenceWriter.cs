using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class ArraySequenceWriter : SequenceWriterBase, ISequenceWriter
	{
		//private long capacity;
		//private BufferSequence<byte> sequence;
		private ArrayElement<byte>? first = null;
		private ArrayElement<byte>? current = null;
		private byte[] buffer;
		private int position = 0;
		private int bufferSize;
		private int bytesWritten = 0;
		

		public const int DefaultCapacity = ArrayElement<byte>.DefaultArraySize;

		public ArraySequenceWriter()
			: this(DefaultCapacity)
		{
		}

		public ArraySequenceWriter(int capacity)
		{
			this.buffer = new byte[capacity];
			this.bufferSize = capacity;
		}

		public ArraySequenceWriter(ArrayElement<byte> first)
			: this(first, DefaultCapacity)
		{
		}

		public ArraySequenceWriter(ArrayElement<byte> first, int capacity)
			: this(capacity)
		{
			this.bufferSize = capacity;
			this.first = first;
			this.current = first;

			while (this.current.Next != null)
			{
				this.bytesWritten += this.current.Count;
				this.current = this.current.Next;
			}

			this.bytesWritten += this.current.Count;
		}

		public ArrayElement<byte>? First => this.first;
		public override object BaseWriter => this.first;
		public override long BytesWritten => this.bytesWritten;

		public override void WriteByte(byte value)
		{
			// TODO: Check whats wrong with that if we first join new segment, if required
			//
			// SOLVED!!! -> The reason is bitStrem writer that take position token before writing value.
			// 
			//if (this.position == this.buffer.Length)
			//    this.PushBufferToNewArrayElementAndCreateNewBuffer();

			//this.buffer[this.position++] = value;
			//this.bytesWritten++;

			this.buffer[this.position++] = value;
			this.bytesWritten++;
			
			if (this.position == this.buffer.Length)
				this.PushBufferToNewArrayElementAndCreateNewBuffer();
		}

		public override void WriteByteArray(byte[] buffer, int offset, int count)
		{
			//this.JoinSegment(buffer, offset, count);

			//for (long i = 0; i < buffer.LongLength; i++)
			//	this.WriteByte(buffer[i]);

			// TODO: CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

			int currentLeft = this.buffer.Length - this.position;

			if (count <= currentLeft)
			{
				Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.buffer, destinationIndex: this.position, length: count);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, count: buffer.Length);

				this.position += count;
			}
			else // Must split into two pices
			{
				// First part is rest of current
				if (currentLeft > 0)
					Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.buffer, destinationIndex: this.position, length: currentLeft);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, (int)count);

				this.position = this.buffer.Length; 
				
				int rest = count - currentLeft;
				
				this.JoinNewArrayElement(minimumBufferSize: rest);

				Array.Copy(sourceArray: buffer, sourceIndex: offset + currentLeft, destinationArray: this.buffer, destinationIndex: 0, length: rest);
				//Buffer.BlockCopy(src: buffer, srcOffset: (int)count, dst: this.current.Buffer, dstOffset: 0, count: (int)rest);

				this.position = rest;
            }

            this.bytesWritten += count;
            
			if (this.position == this.buffer.Length)
                this.PushBufferToNewArrayElementAndCreateNewBuffer();
		}

        public override void WriteSpan(ReadOnlySpan<byte> span, int count)
        {
            this.WriteByteArray(span.ToArray(), 0, count);
        }

        public override void WriteSpanSegment(SpanSegment<byte> spanSegment)
        {
            this.WriteByteArray(spanSegment.ToArray(), 0, spanSegment.Count);
        }

        public override object GetCurrentPositionToken() => new ArraySequencePosition<byte>(ref this.buffer, this.position);

		public override void WriteAt(object positionToken, byte value)
		{
            ArraySequencePosition<byte> sequencePosition = (ArraySequencePosition<byte>)positionToken;

			sequencePosition.Array[sequencePosition.Position] = value;
		}

		public override List<ArraySegment<byte>> ToArraySegmentList()
		{
			List<ArraySegment<byte>> result = new List<ArraySegment<byte>>();
			var element = this.first;

			while (element != null)
			{
				result.Add(element.ArraySegment);
				element = element.Next;
			}

			return result;
		}

		public override SpanSegment<byte> AsSpanSequence()
		{
			if (this.first is null)
				return SpanSegment<byte>.Empty;

			SpanSegment<byte> first = new SpanSegment<byte>(this.first.ArraySegment);
			SpanSegment<byte> current = first;
			var item = this.first.Next;

			while (item != null)
			{
				current.Next = new SpanSegment<byte>(item.ArraySegment);
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
					writer.Write(new ReadOnlySpan<byte>(segment.ArraySegment.Array, (int)segment.ArraySegment.Offset, (int)segment.Count));
				
				segment = segment.Next;
			}
		}

		// TODO: Try to avoid this method if possible (avoiding coping the data)
		public byte[] ToArray() => ArraySequenceReader.ToArray(this.first); // TODO: Make as extension method. CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

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

		public void PushBuffer() => this.PushBufferToNewArrayElementAndCreateNewBuffer();
		public void JoinSegment(byte[] buffer) => this.JoinSegment(buffer, 0, buffer.Length);
		public void JoinSegment(byte[] buffer, int offset, int count) => this.JoinSegment(new ArrayElement<byte>(new ArraySegment<byte>(buffer, offset, count)));

		public void JoinSegment(ArrayElement<byte> segment)
		{
			// TODO:

			// This cannot be satisfied since when new contains no data.
			// This current should not be empty <- this will be brake chaing. Current must havae any data always!!!!

			if (this.current != null && this.current.Count > 0)
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

		private void JoinNewArrayElement(int minimumBufferSize = DefaultCapacity)
		{
			this.bufferSize *= 2;

			if (this.bufferSize < minimumBufferSize)
				this.bufferSize = minimumBufferSize;

            //var last = BufferSequenceReader.FindLast(this.current);
            var newSegment = new ArrayElement<byte>(new ArraySegment<byte>(new byte[this.bufferSize], 0, this.bufferSize));

			if (this.current is null)
			{
				this.current = newSegment;
				this.first = newSegment;
			}
			else
			{
				this.current.Next = newSegment;
				this.current = newSegment;
			}
		}

		private void PushBufferToNewArrayElementAndCreateNewBuffer(int minimumBufferSize = DefaultCapacity)
		{
			this.PushBufferToNewArrayElement();
			this.CreateNewBuffer(minimumBufferSize);
		}

		private void PushBufferToNewArrayElement()
		{
			if (this.position > 0)
			{
				byte[] array = this.buffer;
				var newElement = new ArrayElement<byte>(new ArraySegment<byte>(array, 0, count: this.position));

				if (this.current is null)
				{
					this.current = newElement;
					this.first = current;
				}
				else
				{
					this.current.Next = newElement;
					this.current = newElement;
				}
			}
		}

		private void CreateNewBuffer(int minimumBufferSize = DefaultCapacity)
		{
			if (this.position == 0)
				return;

			this.bufferSize *= 2;

			if (this.bufferSize < minimumBufferSize)
				this.bufferSize = minimumBufferSize;

			this.buffer = new byte[this.bufferSize];
			this.position = 0;
		}


		private ArrayElement<byte> FindSegment(int position, out int segmentOffset) // position is global position
		{
			if (this.first is null)
			{
				segmentOffset = 0;

				return ArrayElement<byte>.Empty;
			}
			
			var segment = this.first; 
			int currentCount = segment.Count;

			while (currentCount < position && segment.Next != null)
			{
				currentCount += segment.Count;
				segment = segment.Next;
			}

			segmentOffset = currentCount - position;
			
			return segment;
		}
	}
}

