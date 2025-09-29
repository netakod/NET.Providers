using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public readonly struct Half
	{
		internal const int SignShift = 15;
		internal const int BiasedExponentShift = 10;

		internal readonly ushort value;

		internal Half(ushort value)
		{
			this.value = value;
		}

		//public static implicit operator ushort(Half value) => value.value;
		public static explicit operator ushort(Half value) => (ushort)(float)value;
		public static explicit operator Half(ushort value) => (Half)(float)value;

		private Half(bool sign, ushort exp, ushort sig) => this.value = (ushort)(((sign ? 1 : 0) << SignShift) + (exp << BiasedExponentShift) + sig);
	}
}
