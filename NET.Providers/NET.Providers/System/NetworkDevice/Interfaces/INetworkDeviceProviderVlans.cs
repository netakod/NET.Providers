using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.Vlans)]
	public interface INetworkDeviceProviderVlans : IDisposable
	{
		ValueTask<IEnumerable<VlanInfo>> GetVlans();
		bool IsVlanSupported();
		ValueTask Add(int vlanId, string name);
		ValueTask Remove(int vlanId);
		ValueTask<string> GetName(int vlanId);
		ValueTask SetName(int vlanId, string vlanName);
	}
}
