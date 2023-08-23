using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Diagnostics;
using System.Security;

namespace Simple
{
    /// <summary>
    /// Encapsulate array sequence segment.
    /// </summary>
    /// <typeparam name="T">Tha buffer elemnt type.</typeparam>
	public class ReadOnlySpanSegment<T>
	{
        public static readonly ReadOnlySpanSegment<T> Empty = new ReadOnlySpanSegment<T>(new T[0], 0, 0);

        protected unsafe T* arrayPtr;
		protected unsafe void* arrayPtr2;

        public ReadOnlySpanSegment(Memory<T> memory)
            : this((ReadOnlyMemory<T>)memory)
        {
        }
        
        public ReadOnlySpanSegment(ReadOnlyMemory<T> memory)
            : this(memory.Span)
        {
        }
        public ReadOnlySpanSegment(Span<T> span)
            : this((ReadOnlySpan<T>)span)
        {
		}

        public unsafe ReadOnlySpanSegment(ArraySegment<T> arraySegment)
        {
            fixed (T* ap = &arraySegment.Array[arraySegment.Offset])
            {
                this.arrayPtr = ap;
				this.arrayPtr2 = ap;
				this.Length = arraySegment.Count;
            }
        }
        public unsafe ReadOnlySpanSegment(ReadOnlySpan<T> span)
        {
            fixed (T* ap = &span.GetPinnableReference())
            {
                this.arrayPtr = ap;
				this.arrayPtr2 = ap;
				this.Length = span.Length;
            }
        }

        public ReadOnlySpanSegment(T[] array)
            : this(array, 0, array.Length)
        {
        }

        public ReadOnlySpanSegment(T[] array, int count)
            : this(array, 0, count)
        {
        }

        public unsafe ReadOnlySpanSegment(T[] array, int offset, int count)
        {
            fixed (T* ap = &array[offset])
            {
                this.arrayPtr = ap;
				this.arrayPtr2 = ap;
				this.Length = count;
            }
        }
        public unsafe ReadOnlySpanSegment(T* arrayPtr, int length)
        {
            this.arrayPtr = arrayPtr;
            this.Length = length;
        }

        /// <summary>
        /// Returns the specified element of the read-only span segment.
        /// </summary>
        /// <param name="index">Array position of the element to get.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public unsafe T this[int index]
		{
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            //[Intrinsic]
            //[NonVersionable]
            get => *(this.arrayPtr + index); // ref Unsafe.Add(ref this.arrayPtr2, (nint)(uint)index);
		}

		public ReadOnlySpanSegment<T>? Next { get; set; } = null;

        public bool IsLast => this.Next == null;
        public int Length { get; private set; }

        public virtual unsafe ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(this.arrayPtr2, this.Length);

        public virtual T[] ToArray() =>this.AsReadOnlySpan().ToArray();
    }
}
