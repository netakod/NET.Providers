using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Buffers;

namespace Simple
{
	public static class MemoryMarshalExtension
	{
        public static ArraySegment<T> ToArraySegment<T>(this Memory<T> memory) => ((ReadOnlyMemory<T>)memory).ToArraySegment();

        public static ArraySegment<T> ToArraySegment<T>(this ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
                throw new InvalidOperationException("Buffer backed by array was expected");

			//if (result.Count == 0)
			//	return result;
			
			return result;
        }

        public static List<ArraySegment<T>> ToArraySegmentList<T>(this ReadOnlySequence<T> sequence)
		{
            var result = new List<ArraySegment<T>>();
            
            foreach (var piece in sequence)
                result.Add(piece.ToArraySegment());

            return result;
        }

        public static BufferSegment<T> ToBufferSegments<T>(this ReadOnlySequence<T> sequence)
        {
            BufferSegment<T>? first = null;
            BufferSegment<T>? current = null;
			
            foreach (var piece in sequence)
            {
                var arraySegment = piece.ToArraySegment();
                var bufferSegment = new BufferSegment<T>(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

				if (current is null)
                {
                    first = bufferSegment;
                    current = bufferSegment;
                }
                else
                {
					current.Next = bufferSegment;
                    current = current.Next;
				}
            }

            if (first != null)
                return first;
            else
                return BufferSegment<T>.Empty;
		}

		public static ArrayElement<T> ToArraySequence<T>(this ReadOnlySequence<T> sequence)
		{
			ArrayElement<T>? first = null;
			ArrayElement<T>? current = null;

			foreach (var piece in sequence)
			{
				var item = new ArrayElement<T>(piece.ToArraySegment());

				if (current is null)
				{
					first = item;
					current = item;
				}
				else
				{
					current.Next = item;
					current = current.Next;
				}
			}

			if (first != null)
				return first;
			else
				return ArrayElement<T>.Empty;
		}

		public static ReadOnlySpanSegment<T> ToSpanSequence<T>(this ReadOnlySequence<T> sequence)
		{
			ReadOnlySpanSegment<T>? first = null;
			ReadOnlySpanSegment<T>? current = null;

			foreach (var piece in sequence)
			{
				var item = new ReadOnlySpanSegment<T>(piece.Span);

				if (current is null)
				{
					first = item;
					current = item;
				}
				else
				{
					current.Next = item;
					current = current.Next;
				}
			}

			if (first != null)
				return first;
			else
				return ReadOnlySpanSegment<T>.Empty;
		}

	}
}
