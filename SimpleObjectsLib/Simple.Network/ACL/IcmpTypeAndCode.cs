using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Network
{
	public struct IcmpTypeAndCode
	{
		public IcmpTypeAndCode(byte icmpType, byte? icmpCode)
		{
			this.IcmpType = icmpType;
			this.IcmpCode = icmpCode;
		}

		public byte IcmpType { get; private set; }
		public byte? IcmpCode { get; private set; }
	}
}
