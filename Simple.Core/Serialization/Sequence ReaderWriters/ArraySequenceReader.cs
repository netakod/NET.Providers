using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public class ArraySequenceReader : SequenceReaderBase, ISequenceReader
	{
		private ArrayElement<byte> first;
		private ArrayElement<byte> current;
		private int currentPosition = 0;
		private bool canRead;
		private long bytesConsumed = 0;

		public ArraySequenceReader(ArrayElement<byte> first)
		{
			this.first = first;
			this.current = first;
			this.canRead = first.Count > 0;
		}

		public override bool CanRead => this.canRead;
		public override object BaseReader => this.first;
		public override long BytesConsumed => this.bytesConsumed;

		public long GetLength() => GetLength(this.first);

		public override byte ReadByte() 
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
				offset = this.currentPosition;
				this.currentPosition += count;
				this.bytesConsumed += count;
				this.OnAfterRead();

				return this.current.ArraySegment.Array;
			}
			else
			{
				offset = 0;
				
				long bytesToRead = Math.Min(count, this.GetLength() - this.BytesConsumed);
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

        public byte[] ToArray()
		{
			if (this.first.Next == null) // Single segment
			{
				if (this.first.Count == this.first.ArraySegment.Array.Length)
				{
					return this.first.ArraySegment.Array;
				}
				else
				{
					byte[] result = new byte[this.first.Count];

					Array.Copy(this.first.ArraySegment.Array, this.first.ArraySegment.Offset, result, 0, this.first.Count);

					return result;
				}
			}
			else
			{
				return ToArray(this.first);
			}
		}

        public override void Advance(long count)
        {
			while (count > 0)
            {
                long currentLeft = this.current!.ArraySegment.Array.Length - this.currentPosition;

                if (count < currentLeft)
                {
                    this.currentPosition = this.currentPosition + (int)count;

                    return; // count = 0
                }
                else
                {
                    this.current = this.current.Next!;
                    this.currentPosition = this.current.ArraySegment.Offset;
                    count -= currentLeft;
                }
            }
        }

        public override object GetCurrentPositionToken() => new ArrayElementPosition<byte>(this.current, this.currentPosition);

        public override void SetCurrentPosition(object positionToken)
        {
            ArrayElementPosition<byte> segmentPositionToken = (ArrayElementPosition<byte>)positionToken;

            this.current = segmentPositionToken.Element;
            this.currentPosition = segmentPositionToken.Position;
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

        public override ReadOnlySpanSegment<byte> AsSpanSequence()
		{
			var element = this.first;
			var result = new ReadOnlySpanSegment<byte>(element.ArraySegment);
			var current = result;

			element = element.Next;

			while (element != null)
			{
				if (element.Count > 0)
				{
					current.Next = new ReadOnlySpanSegment<byte>(element.ArraySegment);
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
				if (segment.Count > 0)
					writer.Write(new ReadOnlySpan<byte>(segment.ArraySegment.Array, segment.ArraySegment.Offset, segment.Count));
				
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


		public static T[] ToArray<T>(ArrayElement<T>? startElement)
		{
			if (startElement is null)
				return new T[0];
			
			var result = new T[GetLength(startElement)];
			int index = 0;
			var item = startElement;

			while (item != null)
			{
				Array.Copy(item.ArraySegment.Array, item.ArraySegment.Offset, result, index, item.Count);
				index += item.Count;
				item = item.Next;
			}

			return result;
		}

		public static int GetLength<T>(ArrayElement<T> startSegment)
		{
			int length = 0;
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
			if (this.currentPosition == this.current.Count) // No more data in current buffer segmnent
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
