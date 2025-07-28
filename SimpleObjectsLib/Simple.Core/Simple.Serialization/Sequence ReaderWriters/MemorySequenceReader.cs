using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace Simple.Serialization
{
	public class MemorySequenceReader : SequenceReaderBase, ISequenceReader
	{
		private MemoryElement<byte> first;
		private MemoryElement<byte> current;
		private int currentPosition = 0;
		private int bytesConsumed = 0;
		private bool canRead;

		public MemorySequenceReader(MemoryElement<byte> first)
		{
			this.first = first;
			this.current = first;
			this.canRead = first.Count > 0;
		}

		public override bool CanRead => this.canRead;
		public override object BaseReader => this.first;
		public override long BytesConsumed => this.bytesConsumed;

		public override byte ReadByte() // What if current.Count is zero
		{
			var result = this.current[this.currentPosition++];

			this.bytesConsumed++;
			this.OnAfterRead();
			
			return result;
		}

		public override byte[] ReadByteArray(int count, out int offset)
		{
			if ((this.current.Count - this.currentPosition) <= count)
			{
				offset = (int)this.currentPosition;
				this.currentPosition += count;
				this.bytesConsumed += count;
				this.OnAfterRead();

				return this.current.Memory.Slice(this.currentPosition, offset).ToArray();
			}
			else
			{
				offset = 0;
				long bytesToRead = Math.Min((long)count, this.GetLength() - this.BytesConsumed);
				byte[] buffer = new byte[bytesToRead];

				this.ReadToBuffer(buffer, offset, (int)bytesToRead);

				return buffer;
			}
		}

		public override int ReadToBuffer(byte[] buffer, int offset, int count)
		{
			int position = offset;
			int bytesRead = 0;

			while (this.CanRead && bytesRead < count)
			{
				buffer[position++] = this.ReadByte();
				bytesRead++;
			}

			this.bytesConsumed += count;

			return bytesRead;
		}

        public override ReadOnlySpan<byte> ReadSpan(int count)
        {
            byte[] array = this.ReadByteArray(count, out int offset);

            return new ReadOnlySpan<byte>(array, offset, count);
        }


        public long GetLength() => GetLength(this.first);

		public byte[] ToArray()
		{
			if (this.first.Next == null) // Single segment
				return this.first.Memory.Slice(0, this.first.Count).ToArray();
			else
				return ToArray(this.first);
		}

        public override void Advance(long count)
        {
            while (count > 0)
            {
                int currentLeft = this.current.Count - this.currentPosition;

                if (count < currentLeft)
                {
                    this.currentPosition = this.currentPosition + (int)count;

                    return; // count = 0
                }
                else
                {
                    this.current = this.current.Next!;
                    this.currentPosition = 0;
                    count -= currentLeft;
                }
            }
        }

        public override object GetCurrentPositionToken() => new MemorySequencePosition<byte>(this.current, this.currentPosition);

        public override void SetCurrentPosition(object positionToken)
        {
			MemorySequencePosition<byte> segmentPositionToken = (MemorySequencePosition<byte>)positionToken;

            this.current = segmentPositionToken.Segment;
            this.currentPosition = segmentPositionToken.Position;
        }


        public override List<ArraySegment<byte>> ToArraySegmentList()
        {
            List<ArraySegment<byte>> result = new List<ArraySegment<byte>>();
            var segment = this.first;

            while (segment != null)
            {
                result.Add(new ArraySegment<byte>(segment.Memory.ToArray(), 0, (int)segment.Count));
                segment = segment.Next;
            }

            return result;
        }

        public override ReadOnlySpanSegment<byte> AsSpanSequence()
		{
			var element = this.first;
			var result = new ReadOnlySpanSegment<byte>(element.Memory.ToArray(), 0, (int)element.Count);
			var current = result;

			element = element.Next;

			while (element != null)
			{
				if (element.Count > 0)
				{
					current.Next = new ReadOnlySpanSegment<byte>(element.Memory.ToArray(), 0, (int)element.Count);
					current = current.Next;
				}

				element = element.Next;
			}

			return result;
		}

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			var segment = this.first;

			while (segment != null)
			{
				writer.Write(new ReadOnlySpan<byte>(segment.Memory.ToArray(), 0, (int)segment.Count));
				segment = segment.Next;
			}
		}

		public override void ResetPosition()
		{
			this.current = first;
			this.currentPosition = 0;
			this.canRead = this.current.Count > 0;
			this.bytesConsumed = 0;
		}


		public static BufferSegment<T> FindLast<T>(BufferSegment<T> startSegment)
		{
			var item = startSegment;

			while (!item.IsLast)
				item = item.Next;

			return item;
		}

		public static T[] ToArray<T>(MemoryElement<T> startSegment)
		{
			var result = new T[GetLength(startSegment)];
			int index = 0;
			var item = startSegment;

			while (item != null)
			{
				for (int i = 0; i < item.Count; i++)
					result[index++] = item[i];

				item = item.Next;
			}

			//var result = new byte[this.Length];
			//var item = this.Sequence.First;
			//int offset = 0;

			//while (item != null)
			//{
			//	int itemCount = item.IsLast ? (int)this.Sequence.CountOfLast : item.Buffer.Count();

			//	Array.Copy(sourceArray: item.Buffer, sourceIndex: 0, destinationArray: result, destinationIndex: offset, length: itemCount);

			//	offset += itemCount;
			//	item = item.Next;
			//}

			return result;
		}

		public static long GetLength<T>(MemoryElement<T> startSegment)
		{
			long length = 0;
			var item = startSegment;

			while (item != null)
			{
				length += item.Count;
				item = item.Next;
			}

			return length;
		}

		private void OnAfterRead()
		{
			if (this.currentPosition == (this.current.Count)) // No more data in current buffer segmnent
			{
				if (this.current.Next == null) // EOF
				{
					this.canRead = false;
				}
				else
				{
					this.current = this.current.Next;
					this.currentPosition = 0;
				}
			}
		}
	}
}
