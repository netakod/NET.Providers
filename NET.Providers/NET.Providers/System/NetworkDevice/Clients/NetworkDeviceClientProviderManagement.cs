using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderManagement : ClientProviderModule
	{
		public NetworkDeviceClientProviderManagement(INetworkDeviceProviderManagement management) => this.Management = management;

		private INetworkDeviceProviderManagement Management { get; set; }


		public TaskInfo<bool> IsWriteConfigurationSupported() => this.SendRequest(this.Management.IsWriteConfigSupported);

		public async ValueTask<TaskInfo> WriteConfigurationToFlash() => await this.SendRequestAsync(this.Management.WriteConfigToFlash);

		public async ValueTask<TaskInfo> WriteConfigurationToServer(string server, ConfigBackupServerProtocol protocol, string configFileName) => await this.SendRequestAsync(async () => await this.Management.WriteConfigToServer(server, protocol, configFileName));
	}
}
