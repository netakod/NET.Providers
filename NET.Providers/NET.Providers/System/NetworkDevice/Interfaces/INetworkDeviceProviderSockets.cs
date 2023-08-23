using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.Sockets)]
	public interface INetworkDeviceProviderSockets : IDisposable
	{
		ValueTask<IEnumerable<TcpConnectionInfo>> GetTcpConnections();
		ValueTask<TcpConnectionState> GetConnectionState(string localAddress, int localPort, string remoteAddress, int remotePort);
		ValueTask<IEnumerable<UdpListeningPortInfo>> GetUdpListeningPorts();
	}
}
