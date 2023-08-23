using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXELWebManaged)]
    public class ProviderManagementZyXELWebManaged : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true; // Each set actions automatically writes configuration to the system flash.
        }

        public override ValueTask WriteConfigToFlash()
        {
            return new ValueTask();
        }

        public override ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            throw new ProviderInfoException("Writing config to " + protocol + " server is not supported.");
        }
    }
}
