using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderSystem : ClientProviderModule
	{
		public NetworkDeviceClientProviderSystem(INetworkDeviceProviderSystem system) => this.System = system;

		private INetworkDeviceProviderSystem System { get; set; }


		public async ValueTask<TaskInfo<string>> GetName() => await this.SendRequestAsync(this.System.GetName);

		public async ValueTask<TaskInfo> SetName(string name) => await this.SendRequestAsync(async () => await this.System.SetName(name));

		public async ValueTask<TaskInfo> SetCommunty(string communty) => await this.SendRequestAsync(async () => await this.System.SetCommunity(communty));
		
		public async ValueTask<TaskInfo<IEnumerable<ApplyPasswordDestination>>> SetPassword(string password) => await this.SendRequestAsync(async () => await this.System.SetPassword(password));
		
		public async ValueTask<TaskInfo<string>> GetDescription() => await this.SendRequestAsync(this.System.GetDescription);

		public async ValueTask<TaskInfo<string>> GetObjectID() => await this.SendRequestAsync(this.System.GetObjectID);

		public async ValueTask<TaskInfo<string>> GetLocation() => await this.SendRequestAsync(this.System.GetLocation);

		public async ValueTask<TaskInfo> SetLocation(string location) => await this.SendRequestAsync(async () => await this.System.SetLocation(location));

		public async ValueTask<TaskInfo<string>> GetContact() => await this.SendRequestAsync(this.System.GetContact);

		public async ValueTask<TaskInfo> SetContact(string contact) => await this.SendRequestAsync(async () => await this.System.SetContact(contact));

		public async ValueTask<TaskInfo<TimeSpan>> GetUpTime() => await this.SendRequestAsync(this.System.GetUpTime);
	}
}
