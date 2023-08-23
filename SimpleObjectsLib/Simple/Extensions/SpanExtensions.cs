using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class SpanExtensions
	{

		public unsafe static Span<T> AsSpan<T>(this ReadOnlySpan<T> value)
		{
			void* ptr;
			
			fixed (void* ap = &value.GetPinnableReference())
			{
				ptr = ap;
			}

			return new Span<T>(ptr, value.Length);  
		}
	}
}
