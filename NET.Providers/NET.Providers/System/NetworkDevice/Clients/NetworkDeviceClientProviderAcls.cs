using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderAcls : ClientProviderModule
	{
		public NetworkDeviceClientProviderAcls(INetworkDeviceProviderAcls acls)
		{
			this.Acls = acls;
		}

		private INetworkDeviceProviderAcls Acls { get; set; }

		public async ValueTask<TaskInfo> SetAclRule(string aclName, string command, AclInfo aclInfo) //L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
		{
			return await this.SendRequestAsync(async () => await this.Acls.SetAclRule(aclName, command, aclInfo)); // permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, sourcePort2, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort, destinationPort2, dscp, established, logging));
		}

		public async ValueTask<TaskInfo> RemoveAclRule(string aclName, string command, AclInfo aclInfo) // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
		{
			return await this.SendRequestAsync(async () => await this.Acls.RemoveAclRule(aclName, command, aclInfo)); // permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, sourcePort2, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort, destinationPort2, dscp, established, logging));
		}

		public async ValueTask<TaskInfo> RemoveAcl(string aclName) // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
		{
			return await this.SendRequestAsync(async () => await this.Acls.RemoveAcl(aclName)); // permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, sourcePort2, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort, destinationPort2, dscp, established, logging));
		}
	}
}
