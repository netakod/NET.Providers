using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikSwOS)]
    public class NetworkDeviceProviderSystemMikroTikSwOS : NetworkDeviceProviderSystemGeneric, INetworkDeviceProviderSystem
    {
		public WebClientMikroTikSwOS WebClient => this.Provider.Web as WebClientMikroTikSwOS;

		public override async ValueTask SetName(string name)
        {
			var systemDictionary = await this.WebClient.GetSystemDictionary();

			systemDictionary["id"] = this.WebClient.ConvertStringToAscii(name);
		}

		public override async ValueTask SetContact(string contact)
		{
			var snmpDictionary = await this.WebClient.GetSnmpDictionary();

			snmpDictionary["ci"] = this.WebClient.ConvertStringToAscii(contact);
		}

		public override async ValueTask SetLocation(string location)
		{
			var snmpDictionary = await this.WebClient.GetSnmpDictionary();

			snmpDictionary["loc"] = this.WebClient.ConvertStringToAscii(location);
		}
	}
}
