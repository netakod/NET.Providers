using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderAcls : NetworkDeviceProviderModule, INetworkDeviceProviderAcls
	{
		public abstract ValueTask SetAclRule(string aclName, string command, AclInfo aclInfo);
		public abstract ValueTask RemoveAclRule(string aclName, string command, AclInfo aclInfo);
		public abstract ValueTask RemoveAcl(string aclName);
	}
}
