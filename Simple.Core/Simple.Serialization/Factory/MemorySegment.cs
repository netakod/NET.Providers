using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class MemorySegment<T> : ReadOnlySequenceSegment<T>
	{
		public MemorySegment(T[] array)
			: this(new Memory<T>(array))
        {
        }

		public MemorySegment(T[] array, int start, int length)
			: this(new Memory<T>(array, start, length))
		{
		}

		public MemorySegment(ReadOnlyMemory<T> memory)
		{
			this.Memory = memory;
		}

		public new MemorySegment<T>? Next
		{ 
			get => base.Next as MemorySegment<T>;
			set => base.Next = value;
		}

		public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
		{
			var segment = new MemorySegment<T>(memory) { RunningIndex = RunningIndex + Memory.Length };

			this.Next = segment;

			return segment;
		}
	}
}
