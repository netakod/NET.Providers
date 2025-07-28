using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.Interfaces)]
	public interface INetworkDeviceProviderInterfaces : IDisposable
	{
		ValueTask<IEnumerable<string>> GetInterfaceNames();
		//bool Contains(string interfaceName);
		ValueTask<string> GetName(int interfaceIndex);
		string GetStandardizedName(string interfaceName);
		ValueTask<int> GetIndex(string interfaceName);
		ValueTask<string> GetShortName(string interfaceName);
		ValueTask<string> GetDescription(string interfaceName);
		ValueTask SetDescription(string interfaceName, string description);
		ValueTask<InterfaceSnmpType> GetInterfaceType(string interfaceName);
		ValueTask<uint> GetMtu(string interfaceName);
		ValueTask<uint> GetSpeed(string interfaceName);
		ValueTask<string> GetPhysicalAddress(string interfaceName);
		ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName);
		ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus);
		ValueTask<InterfaceOperationalStatus> GetOperationalStatus(string interfaceName);
		ValueTask<Dictionary<string, InterfaceOperationalStatus>> GetBulkOperationalStatus();
		ValueTask<TimeSpan> GetLastChange(string interfaceName);
		ValueTask<uint> GetInOctets(string interfaceName);
		ValueTask<uint> GetInUnicastPackets(string interfaceName);
		ValueTask<uint> GetInNUnicastPackets(string interfaceName);
		ValueTask<uint> GetInDiscards(string interfaceName);
		ValueTask<uint> GetInErrors(string interfaceName);
		ValueTask<uint> GetInUnknownProtocols(string interfaceName);
		ValueTask<uint> GetOutOctets(string interfaceName);
		ValueTask<uint> GetOutUnicastPackets(string interfaceName);
		ValueTask<uint> GetOutNUnicastPackets(string interfaceName);
		ValueTask<uint> GetOutDiscards(string interfaceName);
		ValueTask<uint> GetOutErrors(string interfaceName);
		ValueTask<uint> GetOutQLen(string interfaceName);
		ValueTask<uint> GetSpecific(string interfaceName);
		ValueTask<uint> GetInMulticastPackets(string interfaceName);
		ValueTask<uint> GetInBrotcastPackets(string interfaceName);
		ValueTask<uint> GetOutMulticastPackets(string interfaceName);
		ValueTask<uint> GetOutBrotcastPackets(string interfaceName);
		ValueTask<ulong> GetHCInOctets(string interfaceName);
		ValueTask<ulong> GetHCInUnicastPackets(string interfaceName);
		ValueTask<ulong> GetHCInMulticastPackets(string interfaceName);
		ValueTask<ulong> GetHCInBrotcastPackets(string interfaceName);
		ValueTask<ulong> GetHCOutOctets(string interfaceName);
		ValueTask<ulong> GetHCOutUnicastPackets(string interfaceName);
		ValueTask<ulong> GetHCOutMulticastPackets(string interfaceName);
		ValueTask<ulong> GetHCOutBrotcastPackets(string interfaceName);
		ValueTask<InterfaceSnmpTrapUpDownEnable> GetLinkUpDownTrapEnable(string interfaceName);
		ValueTask SetLinkUpDownTrapEnable(string interfaceName, InterfaceSnmpTrapUpDownEnable linkUpDownTrapEnable);
		ValueTask<uint> GetHighSpeed(string interfaceName);
		ValueTask<InterfacePromiscuousMode> GetPromiscuousMode(string interfaceName);
		ValueTask SetPromiscuousMode(string interfaceName, InterfacePromiscuousMode promiscuousMode);
		ValueTask<InterfaceConnectionPresent> GetConnectorPresent(string interfaceName);
		ValueTask<TimeSpan> GetCounterDiscontinuityTime(string interfaceName);

		ValueTask GenerateIpAddressDictionary();
		bool IsAddRemoveSupported();
		ValueTask Add(string interfaceName);
		ValueTask Remove(string interfaceName);

		//bool IsVlanSupported(string interfaceName);
		ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName);
		ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId);

		ValueTask<bool> IsIpAddressSupported(string interfaceName);
		ValueTask<bool> IsWriteIpAddressSupported(string interfaceName);
		ValueTask<NetworkInfo> GetIpAddress(string interfaceName);
		ValueTask SetIpAddress(string interfaceName, IpAddress? ipAddress, int subnetMaskPrefix);
		ValueTask SetDhcpServer(string interfaceName, IpAddress? startIpAddress, IpAddress? endIpAddress, int subnetMaskPrefix, IpAddress? defaultGateway, IEnumerable<IpAddress> dnsServers, string domainName);
		ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName);
		ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName);
		ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName);
		ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix);
		ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix);
		ValueTask<bool> IsL3InterfaceBasedOnVlan(string interfaceName);
		ValueTask<int> GetL3InterfaceBasedVlanId(string interfaceName);

		ValueTask AttachAcl(string interfaceName, string aclName, AclDirection aclDirection);
		ValueTask DetachAcl(string interfaceName, string aclName, AclDirection aclDirection);
		//string CreateAclRule(AclRuleInfo aclInfo); // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging);
	}
}
