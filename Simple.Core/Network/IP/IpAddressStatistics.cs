using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Network
{
	internal static class IpAddressParserStatics
	{
		public const int IPv4AddressBytes = 4;
		public const int IPv6AddressBytes = 16;
		public const int IPv6AddressShorts = IPv6AddressBytes / 2;
	}
}
