using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.Acls)]
	public interface INetworkDeviceProviderAcls : IDisposable
	{
		ValueTask SetAclRule(string aclName, string command, AclInfo aclInfo);
		ValueTask RemoveAclRule(string aclName, string command, AclInfo aclInfo);
		ValueTask RemoveAcl(string aclName);
	}
}
