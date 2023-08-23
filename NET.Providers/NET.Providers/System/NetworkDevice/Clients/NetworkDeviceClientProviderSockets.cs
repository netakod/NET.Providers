using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderSockets : ClientProviderModule
	{
		public NetworkDeviceClientProviderSockets(INetworkDeviceProviderSockets sockets) => this.Sockets = sockets;

		private INetworkDeviceProviderSockets Sockets { get; set; }

		public async ValueTask<TaskInfo<IEnumerable<TcpConnectionInfo>>> GetTcpConnections() => await this.SendRequestAsync(this.Sockets.GetTcpConnections);

		public async ValueTask<TaskInfo<TcpConnectionState>> GetConnectionState(string localAddress, int localPort, string remoteAddress, int remotePort)
		{
			return await this.SendRequestAsync(async () => await this.Sockets.GetConnectionState(localAddress, localPort, remoteAddress, remotePort));
		}

		public async ValueTask<TaskInfo<IEnumerable<UdpListeningPortInfo>>> GetUdpListeningPorts() => await this.SendRequestAsync(this.Sockets.GetUdpListeningPorts);
	}
}
