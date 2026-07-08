using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public unsafe struct SpanElement2
    {
		private unsafe byte* spanPtr;
		private unsafe void* spanPtr2;
		private readonly byte spanReference;
		private unsafe SpanElement2* nextPtr;

		public SpanElement2(Span<byte> span)
        {
			fixed (byte* ap = &span.GetPinnableReference())
			{
				this.spanPtr = ap;
				this.spanPtr2 = ap;
			}

			this.Length = span.Length;
		}

		public int Length { get; private set; }

		public Span<byte> Span
		{
			get => new Span<byte>(this.spanPtr2, this.Length);
		}

		public SpanElement2 Next
		{
			get => (*this.nextPtr); 
			set => this.nextPtr = &value; // Unsafe.AsPointer(ref value);
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
