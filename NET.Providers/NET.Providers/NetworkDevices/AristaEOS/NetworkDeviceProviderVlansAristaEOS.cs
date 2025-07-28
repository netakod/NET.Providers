using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NET.Tools.Telnet_DevelopOld;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.AristaEOS)]
    public class NetworkDeviceProviderVlansAristaEOS : NetworkDeviceProviderVlansCiscoIOS, INetworkDeviceProviderVlans
    {
    }
}
