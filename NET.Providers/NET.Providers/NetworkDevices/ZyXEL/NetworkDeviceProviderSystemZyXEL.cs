using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXEL)]
    public class NetworkDeviceProviderSystemZyXEL : NetworkDeviceProviderSystemGeneric, INetworkDeviceProviderSystem
    {
		public override async ValueTask SetName(string name)
        {
            string newName = String.IsNullOrEmpty(name) ? " " : name.Trim().Replace(' ', '_');
            
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync(String.Format("hostname {0}", newName));
        }

        public override async ValueTask SetContact(string contact)
        {
            string newContact = String.IsNullOrEmpty(contact) ? " " : contact.Trim().Replace(' ', '_');

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync(String.Format("snmp-server contact {0}", newContact));
        }

        public override async ValueTask SetLocation(string location)
        {
            string newLocation = String.IsNullOrEmpty(location) ? " " : location.Trim().Replace(' ', '_');

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync(String.Format("snmp-server location {0}", newLocation));
        }
    }
}
