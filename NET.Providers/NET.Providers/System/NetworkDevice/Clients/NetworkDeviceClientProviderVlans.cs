using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderVlans : ClientProviderModule
	{
		public NetworkDeviceClientProviderVlans(INetworkDeviceProviderVlans vlans) => this.Vlans = vlans;

		private INetworkDeviceProviderVlans Vlans { get; set; }


		public async ValueTask<TaskInfo<IEnumerable<VlanInfo>>> GetVlans() => await this.SendRequestAsync(this.Vlans.GetVlans);

		public TaskInfo<bool> IsVlanSupported() => this.SendRequest(this.Vlans.IsVlanSupported);

		public async ValueTask<TaskInfo> Add(int vlanId, string name) => await this.SendRequestAsync(async () => await this.Vlans.Add(vlanId, name));

		public async ValueTask<TaskInfo> Remove(int vlanId) => await this.SendRequestAsync(async () => await this.Vlans.Remove(vlanId));

		public async ValueTask<TaskInfo<string>> GetName(int vlanId) => await this.SendRequestAsync(async () => await this.Vlans.GetName(vlanId));

		public async ValueTask<TaskInfo> SetName(int vlanId, string vlanName) => await this.SendRequestAsync(async () => await this.Vlans.SetName(vlanId, vlanName));
	}
}
