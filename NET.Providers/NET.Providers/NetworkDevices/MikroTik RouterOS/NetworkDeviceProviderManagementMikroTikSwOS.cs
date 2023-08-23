using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType( DeviceProviderType.MikroTikRouterOS)]
    public class NetworkDeviceProviderManagementMikroTikRouterOS : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true; // Each set actions automatically writes configuration to the system flash.
        }

        public override ValueTask WriteConfigToFlash()
        {
            return new ValueTask();
            // Each set actions automatically writes configuration to the system flash - No further action needed.
		}

		public override ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            throw new ProviderInfoException("Writing config to TFTP Server is not supported.");
        }
    }
}
