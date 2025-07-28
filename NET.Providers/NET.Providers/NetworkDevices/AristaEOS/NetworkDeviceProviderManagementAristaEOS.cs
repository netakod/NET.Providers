using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
    [NetworkDeviceProviderType(DeviceProviderType.AristaEOS)]
    public class NetworkDeviceProviderManagementAristaEOS : NetworkDeviceProviderManagementCiscoIOS, INetworkDeviceProviderManagement
    {
    }
}
