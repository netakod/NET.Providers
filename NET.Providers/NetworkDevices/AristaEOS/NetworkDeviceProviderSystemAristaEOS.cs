using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.AristaEOS)]
    public class NetworkDeviceProviderSystemAristaEOS: NetworkDeviceProviderSystemCiscoIOS, INetworkDeviceProviderSystem
    {
    }
}
