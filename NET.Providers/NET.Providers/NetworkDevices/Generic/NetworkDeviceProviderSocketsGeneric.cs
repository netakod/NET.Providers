using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;
using NET.Tools.Snmp;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Generic)]
    public class NetworkDeviceProviderSocketsGeneric : NetworkDeviceProviderSockets, INetworkDeviceProviderSockets
    {
		public override async ValueTask<IEnumerable<TcpConnectionInfo>> GetTcpConnections()
        {
            List<TcpConnectionInfo> result = new List<TcpConnectionInfo>();
            SnmpData[,] table = await this.Provider.Snmp.GetTableAsync(SnmpOIDs.Tcp.tcpConnTable);

            for (int i = 0; i < table.GetLength(0); i++)
            {
                string localAddress  = table[i, 1].Value;
                int localPort        = table[i, 2].ToInt32();
                string remoteAddress = table[i, 3].Value;
                int remotePort       = table[i, 4].ToInt32();

                result.Add(new TcpConnectionInfo(localAddress, localPort, remoteAddress, remotePort));
            }

            return result;
        }

        public override async ValueTask<TcpConnectionState> GetConnectionState(string localAddress, int localPort, string remoteAddress, int remotePort)
        {
            return (await this.Provider.Snmp.GetAsync(SnmpOIDs.Tcp.tcpConnState + "." + localAddress + "." + localPort + "." + remoteAddress + "." + remotePort)).ToCustom<TcpConnectionState>();
        }

		public override async ValueTask<IEnumerable<UdpListeningPortInfo>> GetUdpListeningPorts()
		{
			List<UdpListeningPortInfo> result = new List<UdpListeningPortInfo>();
			SnmpData[,] table = await this.Provider.Snmp.GetTableAsync(SnmpOIDs.Udp.udpTable);

			for (int i = 0; i < table.GetLength(0); i++)
			{
				string localAddress = table[i, 1].Value;
				int localPort = table[i, 2].ToInt32();

				result.Add(new UdpListeningPortInfo(localAddress, localPort));
			}

			return result;
		}
	}
}