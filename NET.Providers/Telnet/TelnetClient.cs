using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Telnet
{
	public class TelnetClient : TelnetPipeClient 
	{
		public TelnetClient()
		{
		}

		public TelnetClient(ChannelOptions options)
			: base(options)
		{
		}
	}
}
