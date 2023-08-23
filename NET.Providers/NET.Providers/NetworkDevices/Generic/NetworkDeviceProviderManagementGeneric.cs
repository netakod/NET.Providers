using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Generic)]
    public class NetworkDeviceProviderManagementGeneric : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return false;
        }

        public override ValueTask WriteConfigToFlash()
        {
            throw new ProviderInfoException("Write config to flash is not supported");
        }

        public override ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            throw new ProviderInfoException("Write config to TFTP Server is not supported");
        }
    }
}
