//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Simple.Serialization
//{
//	public struct BufferSequence<T>
//	{
//		public BufferSequence(BufferSegment<T> first)
//			: this(first, 0, 0)
//		{
//		}

//		public BufferSequence(BufferSegment<T> first, long countOfLast, long length)
//		{
//			this.First = first;
//			this.CountOfLast = countOfLast;
//			this.Length = length;
//		}

//		public BufferSegment<T> First { get; set; }

//		//public long CountOfLast { get; set; }
//		//public long Length { get; set; }
//	}
//}
