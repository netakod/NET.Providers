using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public ref struct SpanSegment
    {
		private unsafe SpanSegment* nextPtr;

		public unsafe SpanSegment(Span<byte> span)
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

		public byte this[int index]
		{
			get => this.Span[index];
			set => this.Span[index] = value;
		}

		public unsafe SpanSegment Next
		{
			get => (*this.nextPtr);
			//set
			//{
			//		this.SetNext(value);
			//		//this.nextPtr = &value; // Unsafe.AsPointer(ref value);
			//}
		}

		public bool HasNex { get; private set; } = false;

		internal unsafe void SetNext(SpanSegment* nextPtr)
		{
			//fixed (SpanSegment* nextPtr = &value)
			//{
			//	this.nextPtr = nextPtr;
			//}

			this.nextPtr = nextPtr;
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
