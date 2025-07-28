using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderInterfaces : NetworkDeviceProviderModule, INetworkDeviceProviderInterfaces
	{
		public abstract ValueTask<IEnumerable<string>> GetInterfaceNames();
		//public abstract ValueTask<bool> Contains(string interfaceName);
		public abstract ValueTask<string> GetName(int interfaceIndex);
		public abstract string GetStandardizedName(string interfaceName);
		public abstract ValueTask<int> GetIndex(string interfaceName);
		public abstract ValueTask<string> GetShortName(string interfaceName);
		public abstract ValueTask<string> GetDescription(string interfaceName);
		public abstract ValueTask SetDescription(string interfaceName, string description);
		public abstract ValueTask<InterfaceSnmpType> GetInterfaceType(string interfaceName);
		public abstract ValueTask<uint> GetMtu(string interfaceName);
		public abstract ValueTask<uint> GetSpeed(string interfaceName);
		public abstract ValueTask<string> GetPhysicalAddress(string interfaceName);
		public abstract ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName);
		public abstract ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus);
		public abstract ValueTask<InterfaceOperationalStatus> GetOperationalStatus(string interfaceName);
		public abstract ValueTask<Dictionary<string, InterfaceOperationalStatus>> GetBulkOperationalStatus();
		public abstract ValueTask<TimeSpan> GetLastChange(string interfaceName);
		public abstract ValueTask<uint> GetInOctets(string interfaceName);
		public abstract ValueTask<uint> GetInUnicastPackets(string interfaceName);
		public abstract ValueTask<uint> GetInNUnicastPackets(string interfaceName);
		public abstract ValueTask<uint> GetInDiscards(string interfaceName);
		public abstract ValueTask<uint> GetInErrors(string interfaceName);
		public abstract ValueTask<uint> GetInUnknownProtocols(string interfaceName);
		public abstract ValueTask<uint> GetOutOctets(string interfaceName);
		public abstract ValueTask<uint> GetOutUnicastPackets(string interfaceName);
		public abstract ValueTask<uint> GetOutNUnicastPackets(string interfaceName);
		public abstract ValueTask<uint> GetOutDiscards(string interfaceName);
		public abstract ValueTask<uint> GetOutErrors(string interfaceName);
		public abstract ValueTask<uint> GetOutQLen(string interfaceName);
		public abstract ValueTask<uint> GetSpecific(string interfaceName);
		public abstract ValueTask<uint> GetInMulticastPackets(string interfaceName);
		public abstract ValueTask<uint> GetInBrotcastPackets(string interfaceName);
		public abstract ValueTask<uint> GetOutMulticastPackets(string interfaceName);
		public abstract ValueTask<uint> GetOutBrotcastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCInOctets(string interfaceName);
		public abstract ValueTask<ulong> GetHCInUnicastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCInMulticastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCInBrotcastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCOutOctets(string interfaceName);
		public abstract ValueTask<ulong> GetHCOutUnicastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCOutMulticastPackets(string interfaceName);
		public abstract ValueTask<ulong> GetHCOutBrotcastPackets(string interfaceName);
		public abstract ValueTask<InterfaceSnmpTrapUpDownEnable> GetLinkUpDownTrapEnable(string interfaceName);
		public abstract ValueTask SetLinkUpDownTrapEnable(string interfaceName, InterfaceSnmpTrapUpDownEnable linkUpDownTrapEnable);
		public abstract ValueTask<uint> GetHighSpeed(string interfaceName);
		public abstract ValueTask<InterfacePromiscuousMode> GetPromiscuousMode(string interfaceName);
		public abstract ValueTask SetPromiscuousMode(string interfaceName, InterfacePromiscuousMode promiscuousMode);
		public abstract ValueTask<InterfaceConnectionPresent> GetConnectorPresent(string interfaceName);
		public abstract ValueTask<TimeSpan> GetCounterDiscontinuityTime(string interfaceName);

		public abstract ValueTask GenerateIpAddressDictionary();
		public abstract bool IsAddRemoveSupported();
		public abstract ValueTask Add(string interfaceName);
		public abstract ValueTask Remove(string interfaceName);

		public abstract ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName);
		public abstract ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId);

		public abstract ValueTask<bool> IsIpAddressSupported(string interfaceName);
		public abstract ValueTask<bool> IsWriteIpAddressSupported(string interfaceName);
		public abstract ValueTask<NetworkInfo> GetIpAddress(string interfaceName);
		public abstract ValueTask SetIpAddress(string interfaceName, IpAddress? ipAddress, int subnetMaskPrefix);
		public abstract ValueTask SetDhcpServer(string interfaceName, IpAddress? startIpAddress, IpAddress? endIpAddress, int subnetMaskPrefix, IpAddress? defaultGateway, IEnumerable<IpAddress> dnsServers, string domainName);
		public abstract ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName);
		public abstract ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName);
		public abstract ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName);
		public abstract ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix);
		public abstract ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix);
		public abstract ValueTask<bool> IsL3InterfaceBasedOnVlan(string interfaceName);
		public abstract ValueTask<int> GetL3InterfaceBasedVlanId(string interfaceName);

		public abstract ValueTask AttachAcl(string interfaceName, string aclName, AclDirection aclDirection);
		public abstract ValueTask DetachAcl(string interfaceName, string aclName, AclDirection aclDirection);
		//public abstract void SetAclRule(string aclName, AclRuleInfo aclInfo); // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging);
		//public abstract void RemoveAclRule(string aclName, AclRuleInfo aclInfo); // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging);
		//public abstract string CreateAclRule(AclRuleInfo aclInfo); // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging);
		
	}
}
