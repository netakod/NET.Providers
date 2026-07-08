using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class ArraySegmentListReader : SequenceReaderBase, ISequenceReader
	{
		private List<ArraySegment<byte>> arraySegmentList;
		private ArraySegment<byte> current;
		private int currentPosition;
		private bool canRead;
		private int currentListIndex = 0;
		private long bytesConsumed = 0;

		public ArraySegmentListReader(List<ArraySegment<byte>> arraySegmentList)
		{
			this.arraySegmentList = arraySegmentList;
			this.current = this.arraySegmentList.ElementAt(0);
			this.currentPosition = this.current.Offset;
			this.canRead = this.current.LongCount() > 0;
		}

		public override bool CanRead => this.canRead;
		public override long BytesConsumed => this.bytesConsumed;

		public override object BaseReader => this.arraySegmentList;

		public override byte ReadByte()
		{
			if (!this.CanRead)
				throw new EndOfStreamException();
			
			var result = this.current.Array?[this.currentPosition++] ?? default;

			if (this.currentPosition == this.current.Array.LongCount())
			{
				this.currentListIndex++;

				if (this.currentListIndex < this.arraySegmentList.Count)
				{
					this.current = this.arraySegmentList[this.currentListIndex]; //++this.currentIndex
                    this.currentPosition = this.current.Offset;

					if (this.current.Count == 0)
						this.canRead = false; // Empty segment -> its the end
				}
				else // EOF
				{
					this.canRead = false;
				}
			}

			this.bytesConsumed++;

			return result;
		}

        public override ReadOnlySpan<byte> ReadSpan(int count)
        {
			byte[] array = this.ReadByteArray(count, out int offset);

			return new ReadOnlySpan<byte>(array, offset, count);
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

        public override void Advance(long count)
        {
			while (count > 0)
			{
				long currentLeft = this.current.Array.LongCount() - this.currentPosition;

                if (count < currentLeft)
				{
					this.currentPosition = this.currentPosition + (int)count;

					return; // count = 0
                }
                else
				{
					this.current = this.arraySegmentList[this.currentListIndex++];
					this.currentPosition = this.current.Offset;
					count -= currentLeft;
                }
			}
        }

        public override object GetCurrentPositionToken() => new ArraySegmentListPosition(this.currentPosition, this.currentListIndex);

        public override void SetCurrentPosition(object positionToken)
        {
            ArraySegmentListPosition segmentPositionToken = (ArraySegmentListPosition)positionToken;

			this.current = this.arraySegmentList[segmentPositionToken.ListIndex];
			this.currentPosition = segmentPositionToken.Position;
        }

        public override List<ArraySegment<byte>> ToArraySegmentList() => this.arraySegmentList;

        public override ReadOnlySpanSegment<byte> AsSpanSequence()
		{
			if (this.arraySegmentList.Count == 0)
				return ReadOnlySpanSegment<byte>.Empty;

			var first = new ReadOnlySpanSegment<byte>(this.arraySegmentList[0]);
			var current = first;

			for (int i = 1; i < this.arraySegmentList.Count(); i++)
			{
				current.Next = new ReadOnlySpanSegment<byte>(this.arraySegmentList[i]);
				current = current.Next;
			}

			return first;
		}

		public override void WriteTo(IBufferWriter<byte> writer)
		{
			foreach (var segment in this.arraySegmentList)
				writer.Write(new ReadOnlySpan<byte>(segment.Array, (int)segment.Offset, (int)segment.Count));
		}

		public override void ResetPosition()
		{
			this.current = this.arraySegmentList[0];
			this.currentPosition = this.current.Offset;
			this.bytesConsumed = 0;
			this.canRead = this.current.LongCount() > 0;
		}
	}
}
