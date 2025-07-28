using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
//using Internal.Runtime.CompilerServices;


namespace Simple.Serialization
{
	public static class SerializationExtensions
	{
		public static T CreateFromReader<T>(this T serializationObject, SerialReader reader, object? context = null) where T : ISerializable
		{
			serializationObject.ReadFrom(reader, context);

			return serializationObject;
		}

		public static T CreateFromReader<T>(this T serializationObject, ref SequenceReader reader, object? context = null) where T : ISequenceSerializable
		{
			serializationObject.ReadFrom(ref reader, context);

			return serializationObject;
		}


		public static BufferSegment<T> ToBufferSequence<T>(this IEnumerable<ArraySegment<T>> arraySegmentList)
		{
			BufferSegment<T> first;

			if (arraySegmentList.Count() == 0)
			{
				first = new BufferSegment<T>(new T[0], 0, 0);
			}
			else
			{
				BufferSegment<T> current = new BufferSegment<T>(arraySegmentList.ElementAt(0).Array, arraySegmentList.ElementAt(0).Offset, arraySegmentList.ElementAt(0).Count);
				int index = 1;

				first = current;

				while (index < arraySegmentList.Count()) 
				{
					current.Next = new BufferSegment<T>(arraySegmentList.ElementAt(0).Array, arraySegmentList.ElementAt(0).Offset, arraySegmentList.ElementAt(0).Count);
					current = current.Next;
					index++;
				}
			}

			return first;
		}

		public static ArrayElement<T> FindLast<T>(this ArrayElement<T> startElement)
		{
			var item = startElement;

			while (item.Next != null)
				item = item.Next;

			return item;
		}

		/// <summary>
		/// Try to read data with given <paramref name="count"/>.
		/// </summary>
		/// <param name="count">Read count.</param>
		/// <param name="sequence">The read data, if successfully read requested <paramref name="count"/> data.</param>
		/// <returns><c>true</c> if remaining items in current <see cref="SequenceReader{T}" /> is enough for <paramref name="count"/>.</returns>
		public static bool TryReadExact<T>(ref this SequenceReader<T> reader, int count, out ReadOnlySequence<T> sequence)
			where T : unmanaged, IEquatable<T>
		{
			//if (count < 0)
			//	ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);

			//if (count > reader.Remaining)
			//{
			//	sequence = default;
			//	return false;
			//}

			sequence = reader.Sequence.Slice(reader.Position, count);
			
			//if (count != 0)
				reader.Advance(count);
			
			return true;
		}

	}
}
