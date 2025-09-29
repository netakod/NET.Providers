using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class SpanSegment<T> : ReadOnlySpanSegment<T>
	{
		public static readonly new SpanSegment<T> Empty = new SpanSegment<T>(new T[0], 0, 0);
		public const int DefaultCapacity = 1024;

		public SpanSegment()
			: this(DefaultCapacity)
		{
		}

		public SpanSegment(int capacity)
			: this(new Span<T>(new T[capacity], 0, capacity))
		{
		}

		public SpanSegment(Memory<T> memory)
			: base(memory)
		{
		}

		public SpanSegment(ReadOnlyMemory<T> memory)
			: base(memory)
		{
		}
		
		public SpanSegment(Span<T> span)
			: base(span)
		{
		}
		
		public SpanSegment(ArraySegment<T> arraySegment)
			: base(arraySegment)
		{
		}

		public SpanSegment(ReadOnlySpan<T> span)
			: base(span)
		{
		}

		public SpanSegment(T[] array)
			: base(array)
		{
		}

		public SpanSegment(T[] array, int count)
			: base(array, 0, count)
		{
		}

		public SpanSegment(T[] array, int offset, int count)
			: base(array, offset, count)
		{
		}
		
		public unsafe SpanSegment(T* arrayPtr, int length)
			: base(arrayPtr, length)
		{
		}

		public new unsafe T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => base[index]; 
			set => *(this.arrayPtr + index) = value;
		}

		public int Count { get; internal set; } = 0;

		public new SpanSegment<T>? Next 
		{	
			get => base.Next as SpanSegment<T>; 
			set => base.Next = value; 
		}

		public unsafe Span<T> AsSpan() => new Span<T>(this.arrayPtr2, this.Count);

		public unsafe override ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(this.arrayPtr2, this.Count);
		
		public override T[] ToArray() => this.AsSpan().ToArray();
    }
}
