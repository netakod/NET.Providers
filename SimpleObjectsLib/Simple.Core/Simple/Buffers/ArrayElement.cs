using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple
{
    /// <summary>
    /// Encapsulate array sequence segment.
    /// </summary>
    /// <typeparam name="T">Tha buffer elemnt type.</typeparam>
	public class ArrayElement<T>
	{
		public const int DefaultArraySize = 1024;

		public static ArrayElement<T> Empty = new ArrayElement<T>(new ArraySegment<T>(new T[0], 0, 0));

        public ArrayElement(ArraySegment<T> arraySegment)
        { 
            this.ArraySegment = arraySegment;
        }

		public ArraySegment<T> ArraySegment { get; private set; }

        //public int Offset => this.ArraySegment.Offset;
        public int Count => this.ArraySegment.Count;

        public ArrayElement<T>? Next { get; internal set; }

        public bool IsLast => this.Next == null;
        
        public T this[int index] 
        { 
            get => this.ArraySegment[index]; 
            set => this.ArraySegment.Array[this.ArraySegment.Offset + index] = value; 
        }
    }
}
