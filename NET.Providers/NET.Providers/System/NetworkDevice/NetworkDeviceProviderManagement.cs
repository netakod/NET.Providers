using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderManagement : NetworkDeviceProviderModule, INetworkDeviceProviderManagement
	{
		public abstract bool IsWriteConfigSupported();
		public abstract ValueTask WriteConfigToFlash();
		public abstract ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName);
	}
}
