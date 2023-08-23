using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple
{
    /// <summary>
    /// Holds the buffer sequence segment.
    /// </summary>
    /// <typeparam name="T">Tha buffer elemnt type.</typeparam>
	public class BufferSegment<T>
	{
        public const long DefaultBufferSize = 1024;

        public static BufferSegment<T> Empty = new BufferSegment<T>(new T[0], 0, 0);

		public BufferSegment() : this(DefaultBufferSize) { }
        public BufferSegment(long bufferSize) : this(new T[bufferSize], offset: 0, count: 0) { }
        public BufferSegment(T[] buffer) : this(buffer, offset: 0, count: buffer.LongLength) { }

        public BufferSegment(T[] buffer, long offset, long count)
        { 
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
        }

        //public ReadOnlyMemory<T> Memory { get; private set; }
        public T[] Buffer { get; private set; }
        public long Offset { get; private set; }
        public long Count { get; internal set; } // It could be set only from BufferSequenceWriter
        public long Position => this.Offset + this.Count;
        public long Length => this.Buffer.LongLength;
        public BufferSegment<T>? Next { get; internal set; } = null;

        public bool IsLast => this.Next == null;
        
        public T this[long index] 
        { 
            get => this.Buffer[index]; 
            set => this.Buffer[index] = value; 
        }

        //public BufferSegment<T> Join(T[] buffer)
        //{
        //    var item = new BufferSegment<T>(buffer);

        //    this.Next = item;

        //    return item;
        //}
    }
}
