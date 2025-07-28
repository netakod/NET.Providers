using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public class NetworkDeviceClientProviderInterfaces : ClientProviderModule
	{
		public NetworkDeviceClientProviderInterfaces(INetworkDeviceProviderInterfaces interfaces) => this.Interfaces = interfaces;

		private INetworkDeviceProviderInterfaces Interfaces { get; set; }


		public async ValueTask<TaskInfo<IEnumerable<string>>> GetInterfaceNames() => await this.SendRequestAsync(this.Interfaces.GetInterfaceNames);

		public async ValueTask<TaskInfo<string>> GetName(int interfaceIndex) => await this.SendRequestAsync(async () => await this.Interfaces.GetName(interfaceIndex));

		public TaskInfo<string> GetStandardizedName(string interfaceName) => this.SendRequest(() => this.Interfaces.GetStandardizedName(interfaceName));

		public async ValueTask<TaskInfo<int>> GetIndex(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetIndex(interfaceName));

		public async ValueTask<TaskInfo<string>> GetShortName(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetShortName(interfaceName));

		public async ValueTask<TaskInfo<string>> GetDescription(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetDescription(interfaceName));
		
		public async ValueTask<TaskInfo> SetDescription(string interfaceName, string description) => await this.SendRequestAsync(async () => await this.Interfaces.SetDescription(interfaceName, description));
	
		public async ValueTask<TaskInfo<InterfaceSnmpType>> GetInterfaceType(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInterfaceType(interfaceName));
	
		public async ValueTask<TaskInfo<uint>> GetMtu(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetMtu(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetSpeed(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetSpeed(interfaceName));

		public async ValueTask<TaskInfo<string>> GetPhysicalAddress(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetPhysicalAddress(interfaceName));

		public async ValueTask<TaskInfo<InterfaceAdminStatus>> GetAdminStatus(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetAdminStatus(interfaceName));

		public async ValueTask<TaskInfo> SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus) => await this.SendRequestAsync(async () => await this.Interfaces.SetAdminStatus(interfaceName, adminStatus));

		public async ValueTask<TaskInfo<InterfaceOperationalStatus>> GetOperationalStatus(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOperationalStatus(interfaceName));
		
		public async ValueTask<TaskInfo<Dictionary<string, InterfaceOperationalStatus>>> GetBulkOperationalStatus() => await this.SendRequestAsync(this.Interfaces.GetBulkOperationalStatus);

		public async ValueTask<TaskInfo<TimeSpan>> GetLastChange(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetLastChange(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetInOctets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInOctets(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetInUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInUnicastPackets(interfaceName));
	
		public async ValueTask<TaskInfo<uint>> GetInNUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInNUnicastPackets(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetInDiscards(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInDiscards(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetInErrors(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInErrors(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetInUnknownProtocols(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInUnknownProtocols(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutOctets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutOctets(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutUnicastPackets(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutNUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutNUnicastPackets(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutDiscards(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutDiscards(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutErrors(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutErrors(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetOutQLen(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutQLen(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetSpecific(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetSpecific(interfaceName));
		
		public async ValueTask<TaskInfo<uint>> GetInMulticastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInMulticastPackets(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetInBrotcastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetInBrotcastPackets(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetOutMulticastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetOutMulticastPackets(interfaceName));

		public async ValueTask<TaskInfo<uint>> GetOutBrotcastPackets(string interfaceName) =>  await this.SendRequestAsync(async () => await this.Interfaces.GetOutBrotcastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCInOctets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCInOctets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCInUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCInUnicastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCInMulticastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCInMulticastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCInBrotcastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCInBrotcastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCOutOctets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCOutOctets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCOutUnicastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCOutUnicastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCOutMulticastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCOutMulticastPackets(interfaceName));

		public async ValueTask<TaskInfo<ulong>> GetHCOutBrotcastPackets(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHCOutBrotcastPackets(interfaceName));

		public async ValueTask<TaskInfo<InterfaceSnmpTrapUpDownEnable>> GetLinkUpDownTrapEnable(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetLinkUpDownTrapEnable(interfaceName));

		public async ValueTask<TaskInfo> SetLinkUpDownTrapEnable(string interfaceName, InterfaceSnmpTrapUpDownEnable linkUpDownTrapEnable) => await this.SendRequestAsync(async () => await this.Interfaces.SetLinkUpDownTrapEnable(interfaceName, linkUpDownTrapEnable));

		public async ValueTask<TaskInfo<uint>> GetHighSpeed(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetHighSpeed(interfaceName));

		public async ValueTask<TaskInfo<InterfacePromiscuousMode>> GetPromiscuousMode(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetPromiscuousMode(interfaceName));

		public async ValueTask<TaskInfo> SetPromiscuousMode(string interfaceName, InterfacePromiscuousMode promiscuousMode) => await this.SendRequestAsync(async () => await this.Interfaces.SetPromiscuousMode(interfaceName, promiscuousMode));

		public async ValueTask<TaskInfo<InterfaceConnectionPresent>> GetConnectorPresent(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetConnectorPresent(interfaceName));

		public async ValueTask<TaskInfo<TimeSpan>> GetCounterDiscontinuityTime(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetCounterDiscontinuityTime(interfaceName));

		public TaskInfo<bool> IsAddRemoveSupported() => this.SendRequest(this.Interfaces.IsAddRemoveSupported);
	
		public async ValueTask<TaskInfo> Add(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.Add(interfaceName));

		public async ValueTask<TaskInfo> Remove(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.Remove(interfaceName));

		public async ValueTask<TaskInfo<bool>> IsIpAddressSupported(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.IsIpAddressSupported(interfaceName));

		public async ValueTask<TaskInfo<bool>> IsWriteIpAddressSupported(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.IsWriteIpAddressSupported(interfaceName));

		public async ValueTask<TaskInfo<NetworkInfo?>> GetIpAddress(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetIpAddress(interfaceName));

		public async ValueTask<TaskInfo> SetIpAddress(string interfaceName, IpAddress? ipAddress, int subnetMaskPrefix) => await this.SendRequestAsync(async () => await this.Interfaces.SetIpAddress(interfaceName, ipAddress, subnetMaskPrefix));

		public async ValueTask<TaskInfo> SetDhcpServer(string interfaceName, IpAddress? startIpAddress, IpAddress? endIpAddress, int subnetMaskPrefix, IpAddress? defaultGateway, IEnumerable<IpAddress> dnsServers, string domainName) => await this.SendRequestAsync(async () => await this.Interfaces.SetDhcpServer(interfaceName, startIpAddress, endIpAddress, subnetMaskPrefix, defaultGateway, dnsServers, domainName));
		
		public async ValueTask<TaskInfo<bool>> IsSecondaryIpAddressSupported(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.IsSecondaryIpAddressSupported(interfaceName));

		public async ValueTask<TaskInfo<IEnumerable<NetworkInfo>?>> GetSecondaryIpAddresses(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetSecondaryIpAddresses(interfaceName));

		public async ValueTask<TaskInfo<bool>> IsAddRemoveSecondaryIpAddressSupported(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.IsAddRemoveSecondaryIpAddressSupported(interfaceName));

		public async ValueTask<TaskInfo> AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix) => await this.SendRequestAsync(async () => await this.Interfaces.AddSecondaryIpAddress(interfaceName, ipAddress, subnetMaskPrefix));

		public async ValueTask<TaskInfo> RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix) => await this.SendRequestAsync(async () => await this.Interfaces.RemoveSecondaryIpAddress(interfaceName, ipAddress, subnetMaskPrefix));

		public async ValueTask<TaskInfo> GenerateIpAddressDictionary() => await this.SendRequestAsync(this.Interfaces.GenerateIpAddressDictionary);

		public async ValueTask<TaskInfo<SwitchportInfo>> GetSwitchportInfo(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetSwitchportInfo(interfaceName));

		public async ValueTask<TaskInfo> SetSwitchportMode(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId) => await this.SendRequestAsync(async () => await this.Interfaces.SetSwitchport(interfaceName, switchportMode, vlanId));

		public async ValueTask<TaskInfo<bool>> IsLayer3InterfaceBasedOnVlan(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.IsL3InterfaceBasedOnVlan(interfaceName));

		public async ValueTask<TaskInfo<int>> GetLayer3InterfaceBasedVlanId(string interfaceName) => await this.SendRequestAsync(async () => await this.Interfaces.GetL3InterfaceBasedVlanId(interfaceName));

		public async ValueTask<TaskInfo> AttachAcl(string interfaceName, string aclName, AclDirection aclDirection) => await this.SendRequestAsync(async () => await this.Interfaces.AttachAcl(interfaceName, aclName, aclDirection));

		public async ValueTask<TaskInfo> DetachAcl(string interfaceName, string aclName, AclDirection aclDirection) => await this.SendRequestAsync(async () => await this.Interfaces.DetachAcl(interfaceName, aclName, aclDirection));
	}
}
