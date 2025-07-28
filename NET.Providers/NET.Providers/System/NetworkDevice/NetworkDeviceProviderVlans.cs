using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderVlans : NetworkDeviceProviderModule, INetworkDeviceProviderVlans
	{
		public abstract ValueTask<IEnumerable<VlanInfo>> GetVlanInfos();
		public abstract bool IsVlanSupported();
		public abstract ValueTask Set(int vlanId, string name);
		public abstract ValueTask Remove(int vlanId);
		public abstract ValueTask<string> GetName(int vlanId);
	}
}
