using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public class MemoryElement<T>
	{
		public MemoryElement(Memory<T> memory)
		{
			this.Memory = memory;
		}

		public int Count { get; internal set; }
		public int Length => this.Memory.Length;

		public T this[int index]
		{
			get => this.Memory.Span[index];
			set => this.Memory.Span[index] = value;
		}

		public Memory<T> Memory { get; private set; }

		public MemoryElement<T>? Next { get; internal set; } = null;


		private static int ToBuffer(int value, Span<char> span)
		{
			string strValue = value.ToString();
			int length = strValue.Length;
			
			strValue.AsSpan().CopyTo(span.Slice(0, length));
			
			return length;
		}
	}
}
