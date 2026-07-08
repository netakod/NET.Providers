using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.Drawing;

namespace Simple.Serialization
{
	public class ArraySegmentListWriter : SequenceWriterBase, ISequenceWriter
	{
		//private int capacity;
		//private BufferSequence<byte> sequence;
		private List<ArraySegment<byte>> arraySegmentList;
		//private ArraySegment<byte> current;
        private byte[] array;
		private int position = 0;
		//private int currentListIndex = 0;
		private long arraySize;
		private long bytesWritten = 0;

		//private int currentPosition = 0;

		public const long DefaultCapacity = BufferSegment<byte>.DefaultBufferSize;

		public ArraySegmentListWriter()
			: this(DefaultCapacity)
		{
		}

		public ArraySegmentListWriter(long capacity)
			: this(new List<ArraySegment<byte>>())
		{
		}

		public ArraySegmentListWriter(List<ArraySegment<byte>> arraySegmentList)
			: this(arraySegmentList, (int)DefaultCapacity)
		{
		}

		public ArraySegmentListWriter(List<ArraySegment<byte>> arraySegmentList, int capacity)
		{
			this.arraySegmentList = arraySegmentList;
			//this.capacity = capacity;
			this.arraySize = capacity;
			this.array = new byte[capacity];

			foreach (var segment in arraySegmentList)
				this.bytesWritten += segment.Count;
		}

		//public int Capacity { get => this.capacity; set => this.capacity = value; }
		public override object BaseWriter => this.arraySegmentList;
		public override long BytesWritten => this.bytesWritten;

		public override void WriteByte(byte value)
		{
			// TODO: Check whats wrong with that if we first join new segment, if required
			//
			// SOLVED!!! -> The reason is bitStrem writer that take position token before writing value.
			// 
			//if (this.currentPosition == this.current.Array.Length)
			//	this.JoinNewSegment();

			//this.current[this.currentPosition++] = value;
			//this.bytesWritten++;

			this.array[this.position++] = value;
			this.bytesWritten++;
			
			if (this.position == this.array.Length)
				this.PushBufferToArraySegmentListAndCreateNewSegment();


		}

		public override void WriteByteArray(byte[] buffer, int offset, int count)
		{
			//this.JoinSegment(buffer, offset, count);

			//for (long i = 0; i < buffer.LongLength; i++)
			//	this.WriteByte(buffer[i]);

			// TODO: CHECK: Is Array.Copy faster than manualy iterate and copy byte by byte?

			int currentLeft = this.array.Length - this.position;

			if (count <= currentLeft)
			{
				Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.array, destinationIndex: this.position, length: count);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, count: buffer.Length);

				this.position += count;
			}
			else // Must split into two pices
			{
				// First part is rest of current
				if (currentLeft > 0)
					Array.Copy(sourceArray: buffer, sourceIndex: offset, destinationArray: this.array, destinationIndex: this.position, length: currentLeft);
				//Buffer.BlockCopy(src: buffer, srcOffset: 0, dst: this.current.Buffer, dstOffset: (int)this.current.Count, (int)count);

				this.position = this.array.Length; // += currentLeft;

				int rest = count - currentLeft;

				this.PushBufferToArraySegmentListAndCreateNewSegment(minimumBufferSize: rest);

				Array.Copy(sourceArray: buffer, sourceIndex: offset + currentLeft, destinationArray: this.array, destinationIndex: 0, length: rest);
				//Buffer.BlockCopy(src: buffer, srcOffset: (int)count, dst: this.current.Buffer, dstOffset: 0, count: (int)rest);

				this.position = rest;
			}

			this.bytesWritten += count;
		}

        public override void WriteSpan(ReadOnlySpan<byte> span, int count)
        {
			this.WriteByteArray(span.ToArray(), 0, count);
        }

        public override void WriteSpanSegment(SpanSegment<byte> spanSegment)
		{
            this.WriteByteArray(spanSegment.ToArray(), 0, spanSegment.Length);
        }

        public override object GetCurrentPositionToken() => new ArraySegmentListPosition(this.position, this.arraySegmentList.Count);

		public override void WriteAt(object positionToken, byte value)
		{
			ArraySegmentListPosition segmentPositionToken = (ArraySegmentListPosition)positionToken;

			if (segmentPositionToken.ListIndex < this.arraySegmentList.Count)
				this.arraySegmentList[segmentPositionToken.ListIndex].Array![segmentPositionToken.Position] = value;
			else
				this.array[segmentPositionToken.Position] = value;
		}

		public override List<ArraySegment<byte>> ToArraySegmentList()
		{
			if (this.position > 0)
				this.PushBufferToArraySegmentListAndCreateNewSegment();

			return this.arraySegmentList;
		}

		public override SpanSegment<byte> AsSpanSequence()
		{
			this.PushBufferToArraySegmentListAndCreateNewSegment();

			if (this.arraySegmentList.Count == 0)
				return SpanSegment<byte>.Empty;

			SpanSegment<byte> first = new SpanSegment<byte>(this.arraySegmentList[0].Array, this.arraySegmentList[0].Count); ;
			SpanSegment<byte> current = first;

			for (int i = 1; i < this.arraySegmentList.Count; i++)
			{
				var item = this.arraySegmentList[i];

				current.Next = new SpanSegment<byte>(item.Array, item.Count);
				current = current.Next;
			}

			return first;
		}

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			foreach (ArraySegment<byte> segment in this.arraySegmentList)
				writer.Write(new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count));

			if (this.position > 0)
				writer.Write(new ReadOnlySpan<byte>(this.array, 0, this.position));
		}

		public void AddSegment(ArraySegment<byte> segment)
		{
			if (this.position > 0)
				this.PushBufferToArraySegmentList();

			this.arraySegmentList.Add(segment);
			this.CreateNewBuffer();
			this.bytesWritten += segment.Count;
		}

		public void AddSegmentRange(IEnumerable<ArraySegment<byte>> segments)
		{
			foreach (var segment in segments)
				this.AddSegment(segment);
		}

		private void PushBufferToArraySegmentList()
		{
			if (this.position > 0)
				this.arraySegmentList.Add(new ArraySegment<byte>(this.array, 0, this.position));
		}

        private void PushBufferToArraySegmentListAndCreateNewSegment(int minimumBufferSize = (int)DefaultCapacity)
        {
			this.PushBufferToArraySegmentList();
			this.CreateNewBuffer(minimumBufferSize);
        }
        
		private void CreateNewBuffer(int minimumBufferSize = (int)DefaultCapacity)
		{
			if (this.position == 0)
				return;
			
			this.arraySize *= 2;

            if (this.arraySize < minimumBufferSize)
                this.arraySize = minimumBufferSize;

            this.array = new byte[this.arraySize];
            this.position = 0;
        }
    }
}

