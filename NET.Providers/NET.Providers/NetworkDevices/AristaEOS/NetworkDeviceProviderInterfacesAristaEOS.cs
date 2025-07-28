using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.AristaEOS)]
    public class NetworkDeviceProviderInterfacesAristaEOS : NetworkDeviceProviderInterfacesCiscoIOS, INetworkDeviceProviderInterfaces
    {
    }
}
