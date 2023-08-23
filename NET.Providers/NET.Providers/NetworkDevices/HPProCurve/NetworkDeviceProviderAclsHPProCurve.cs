using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.HPProCurve)]
	public class NetworkDeviceProviderAclsHPProCurve : NetworkDeviceProviderAcls, INetworkDeviceProviderAcls
	{
        public override async ValueTask SetAclRule(string aclName, string command, AclInfo aclInfo) // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
        {
            string acl = command + " " + this.CreateAclRule(aclInfo); // permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, sourcePort2, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort,destinationPort2, dscp, established, logging);
            
            await this.ApplyAclRule(aclName, acl);
        }

        public override async ValueTask RemoveAclRule(string aclName, string command, AclInfo aclInfo) // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
        {
            string acl = command + " " + this.CreateAclRule(aclInfo);
            
            await this.ApplyAclRule(aclName, "no " + acl);
        }

        public override async ValueTask RemoveAcl(string aclName)
		{
			await this.Provider.Terminal.EnterConfigModeAsync();
			await this.Provider.Terminal.SendAsync("no ip access-list extended " + aclName);
		}

        protected string CreateAclRule(AclInfo aclInfo) // L3InterfaceAclPermission permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, L3InterfaceAclPortOperator sourcePortCriteria, ushort? sourcePort, ushort? sourcePort2, string destinationIpAddress, string destinationIpSubnetMask, L3InterfaceAclPortOperator destinationPortCriteria, ushort? destinationPort, ushort? destinationPort2, byte? dscp, bool established, bool logging)
        {
            //TODO: include dscp

            string strProtocol = this.GetAclProtocolString(aclInfo.Protocol);
            string aclRoule = strProtocol + " " + this.GetAclIpAddressString(aclInfo.SourceIpAddress.ToString(), aclInfo.SourceSubnetMaskPrefix);
            string strSourcePort = this.GetAclPortString(aclInfo.SourcePortOperator, aclInfo.SourcePort, aclInfo.SourcePort2);
            string strDestinationPort = this.GetAclPortString(aclInfo.DestinationPortOperator, aclInfo.DestinationPort, aclInfo.DestinationPort2);

            if (strSourcePort != string.Empty)
                aclRoule += " " + strSourcePort;

            aclRoule += " " + this.GetAclIpAddressString(aclInfo.SourceIpAddress.ToString(), aclInfo.DestinationSubnetMaskPrefix);

            if (strDestinationPort != string.Empty)
                aclRoule += " " + strDestinationPort;

            if (aclInfo.Established)
                aclRoule += " established";

            if (aclInfo.Logging)
                aclRoule += " log";

            return aclRoule;
        }

        protected async ValueTask ApplyAclRule(string aclName, string aclRoule)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("ip access-list extended " + aclName);
            await this.Provider.Terminal.SendAsync(aclRoule);
            await this.Provider.Terminal.SendAsync("exit");
        }

        protected string GetAclDirectionString(AclDirection aclDirection)
        {
            return aclDirection == AclDirection.In ? "in" : "out";
        }

        protected string GetAclProtocolString(byte protocol)
        {
            string result = protocol.ToString();

            switch (protocol)
            {
                case 4:
                    
                    result = "ip";
                    
                    break;
                
                case 6:
                    
                    result = "tcp";
                    
                    break;
                
                case 17:
                    
                    result = "udp";
                    
                    break;

                default:

                    result = protocol.ToString();
                    
                    break;
            }

            return result;
        }

        protected string GetAclIpAddressString(string ipAddress, int ipSubnetMaskNumOfBits)
        {
            string result = "";

            if (ipSubnetMaskNumOfBits == 0)
                result = "any";
            else if (ipSubnetMaskNumOfBits == 32)
                result = " host " + ipAddress;
            else
                result = ipAddress + " " + IpHelper.GetSubnetMaskWildCard(ipSubnetMaskNumOfBits);

            return result;
        }

        protected string GetAclPortString(AclPortOperator portCriteria, ushort? port, ushort? port2)
        {
            string result = string.Empty;

            switch (portCriteria)
            {
                case AclPortOperator.Equal:

                    result = "eq";
                    
                    break;

                case AclPortOperator.NotEqual:

                    result = "neq";
                    
                    break;

                case AclPortOperator.GreaterThan:

                    result = "gt";
                    
                    break;

                case AclPortOperator.LessThan:

                    result = "lt";
                    
                    break;
            }

            if (result != string.Empty)
                result += " " + port;

            return result;
        }
    }
}
