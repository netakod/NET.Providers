using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public unsafe class SpanElement
    {
		private unsafe byte* ptr;
		private unsafe void* ptr2;
		private Span<byte>* spanPtr;
		private unsafe byte* bytePtr;

		public SpanElement(Span<byte> span)
        {
			this.spanPtr = &span;
			
			fixed (byte* ap = &span.GetPinnableReference())
			{
				
				this.ptr = ap;
				this.ptr2 = ap;
				this.bytePtr = (byte*)Unsafe.AsPointer(ref span.GetPinnableReference());
		}

			this.Length = span.Length;
		}

		public int Length { get; private set; }

		public Span<byte> Span => (*this.spanPtr);
		public Span<byte> Span2 => new Span<byte>(this.spanPtr, this.Length);

		public SpanElement? Next { get; set; } = null;


		//public void Method(ref MyStruct refStruct)
		//{
		//	fixed (MyStruct* structPointer = &refStruct)
		//	{
		//		// do something with ref struct here
		//	}
		//}
	}
}
