using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization.Factory
{
	public static class ReadOnlySequenceFactory
	{
		public static ReadOnlySequence<T> Create<T>(MemorySegment<T> first)
		{
			MemorySegment<T> last = FindLast(first);

			return new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);
		}

		private static MemorySegment<T> FindLast<T>(MemorySegment<T> segment)
		{
			MemorySegment<T> current = segment;

			while (current.Next != null)
				current = current.Next;

			return current;
		}
	}
}
