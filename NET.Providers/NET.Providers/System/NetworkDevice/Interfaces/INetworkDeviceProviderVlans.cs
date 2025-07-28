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
		ValueTask<IEnumerable<VlanInfo>> GetVlanInfos();
		bool IsVlanSupported();
		ValueTask Set(int vlanId, string name);
		ValueTask Remove(int vlanId);
		ValueTask<string> GetName(int vlanId);
	}
}
