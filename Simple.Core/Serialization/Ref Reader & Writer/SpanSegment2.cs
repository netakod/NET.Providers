using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public unsafe ref struct SpanSegment2
    {
		private unsafe SpanSegment2* nextPtr;
		private unsafe byte* nextPtr2;
		private unsafe void* nextPtr3;


		public unsafe SpanSegment2(Span<byte> span)
        {
			this.Span = span;
			
			//fixed (byte* ap = &span.GetPinnableReference())
			//{
				
			//	this.ptr = ap;
			//	this.ptr2 = ap;
			//	this.bytePtr = (byte*)Unsafe.AsPointer(ref span.GetPinnableReference());
			//}
		}

		public int Count { get; internal set; }

		public int Length => this.Span.Length;

		public Span<byte> Span { get; private set; }

		public SpanSegment2 Next
		{
			get => (*this.nextPtr);
			set
			{
				this.nextPtr = &value; // Unsafe.AsPointer(ref value);
				//this.nextPtr2 = (byte*)Unsafe.AsPointer(ref value.GetPinnableReference());
			}
		}


		//public void Method(ref MyStruct refStruct)
		//{
		//	fixed (MyStruct* structPointer = &refStruct)
		//	{
		//		// do something with ref struct here
		//	}
		//}
	}
}
