﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public class SpanSequenceReader : SequenceReaderBase, ISequenceReader
	{
		private ReadOnlySpanSegment<byte> first;
		private ReadOnlySpanSegment<byte> current;
		private int currentPosition = 0;
		private bool canRead = false;
		private long bytesConsumed = 0;

		public SpanSequenceReader(ReadOnlySpanSegment<byte> first)
		{
			this.first = first;
			this.current = first;
			this.canRead = first.Length > 0;
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
			if ((this.current.Length - this.currentPosition) <= count)
			{
				offset = this.currentPosition;
				this.currentPosition += count;
				this.bytesConsumed += count;
				this.OnAfterRead();

				return this.current.ToArray();
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

		//public override byte[] ReadBytes(int count)
		//{
		//	byte[] result = new byte[count];

		//	for (int i = 0; i < count; i++)
		//		result[i] = this.ReadByte();

		//	this.bytesConsumed += count;

		//	return result;
		//}

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
			ReadOnlySpan<byte> result;

			if ((this.current.Length - this.currentPosition) <= count)
			{
				result = this.current.AsReadOnlySpan().Slice(this.currentPosition, count);
				this.currentPosition += count;
			}
			else
			{
				byte[] array = this.ReadByteArray(count, out int offset);

				result = new ReadOnlySpan<byte>(array, offset, count);
			}

			this.bytesConsumed += count;

			return result;

        }

        public long GetLength() => GetLength(this.first);

		public byte[] ToArray()
		{
			if (this.first.Next == null) // Single segment
				return this.first.ToArray();
			else
				return ToArray(this.first);
		}

        public override void Advance(long count)
        {
            while (count > 0)
            {
                long currentLeft = this.current!.Length - this.currentPosition;

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

        public override object GetCurrentPositionToken() => new SpanSegmentPosition<byte>(this.current, this.currentPosition);

        public override void SetCurrentPosition(object positionToken)
        {
            SpanSegmentPosition<byte> segmentPositionToken = (SpanSegmentPosition<byte>)positionToken;

            this.current = segmentPositionToken.Segment;
            this.currentPosition = segmentPositionToken.Position;
        }


        public override List<ArraySegment<byte>> ToArraySegmentList()
        {
            List<ArraySegment<byte>> result = new List<ArraySegment<byte>>();
            var segment = this.first;

            while (segment != null)
            {
                result.Add(new ArraySegment<byte>(segment.ToArray(), 0, segment.Length));
                segment = segment.Next;
            }

            return result;
        }

        public override ReadOnlySpanSegment<byte> AsSpanSequence() => this.first;

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			var segment = this.first;

			while (segment != null)
			{
				writer.Write(segment.AsReadOnlySpan());
				segment = segment.Next;
			}
		}

		public override void ResetPosition()
		{
			this.current = first;
			this.currentPosition = 0;
			this.canRead = this.current.Length > 0;
			this.bytesConsumed = 0;
		}


		public static ReadOnlySpanSegment<T> FindLast<T>(ReadOnlySpanSegment<T> startSegment)
		{
			var item = startSegment;

			while (item.Next != null)
				item = item.Next;

			return item;
		}

		public static T[] ToArray<T>(ReadOnlySpanSegment<T> startSegment)
		{
			var result = new T[GetLength(startSegment)];
			int index = 0;
			var item = startSegment;

			while (item != null)
			{
				for (int i = 0; i < item.Length; i++)
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

		public static long GetLength<T>(ReadOnlySpanSegment<T> startSegment)
		{
			long length = 0;
			var item = startSegment;

			while (item != null)
			{
				length += item.Length;
				item = item.Next;
			}

			return length;
		}

		private void OnAfterRead()
		{
			if (this.currentPosition == (this.current.Length)) // No more data in current buffer segmnent
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
