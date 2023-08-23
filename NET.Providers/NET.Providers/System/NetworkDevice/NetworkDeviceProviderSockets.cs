using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderSockets : NetworkDeviceProviderModule, INetworkDeviceProviderSockets
	{
		public abstract ValueTask<IEnumerable<TcpConnectionInfo>> GetTcpConnections();
		public abstract ValueTask<TcpConnectionState> GetConnectionState(string localAddress, int localPort, string remoteAddress, int remotePort);
		public abstract ValueTask<IEnumerable<UdpListeningPortInfo>> GetUdpListeningPorts();
	}
}
