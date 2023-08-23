using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.CiscoIOS)]
    public class NetworkDeviceProviderSystemCiscoIOS : NetworkDeviceProviderSystemGeneric, INetworkDeviceProviderSystem
    {
		public override async ValueTask SetName(string name)
        {
            string newName = String.IsNullOrEmpty(name) ? " " : name.Trim().Replace(' ', '_');
            string result = String.Empty;
            
            await this.Provider.Terminal.EnterConfigModeAsync();
            result = await this.Provider.Terminal.SendAsync(String.Format("hostname {0}", newName));

            if (!result.IsNullOrEmpty())
                throw new ProviderInfoException(result);
        }
    }
}
