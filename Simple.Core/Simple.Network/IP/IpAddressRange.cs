using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Network
{
	public class IpAddressRange
	{

		public IpAddressRange(IpAddress startAddress, IpAddress endAddress) 
		{ 
			this.StartAddres = startAddress;
			this.EndAddresa = endAddress;
		}

		public IpAddress StartAddres { get; private set; }
		public IpAddress EndAddresa { get; private set; }
	}
}
