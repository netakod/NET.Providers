using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.Management)]
	public interface INetworkDeviceProviderManagement : IDisposable
	{
		bool IsWriteConfigSupported();
		ValueTask WriteConfigToFlash();
		ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName);
	}
}
