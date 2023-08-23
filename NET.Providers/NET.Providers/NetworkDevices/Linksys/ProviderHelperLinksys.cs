using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Providers
{
    internal class ProviderHelperLinksys
    {
        public static string GetLinksysInterfaceTypeCommand(string interfaceName)
        {
            string result = String.Empty;
            LinksysPortType linksysPortType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);

            switch (linksysPortType)
            {
                case LinksysPortType.Ethernet:
                    
                    result = "ethernet";
                    
                    break;
                
                case LinksysPortType.PortChannel:
                    
                    result = "port-channel";
                    
                    break;

                case LinksysPortType.Vlan:
                    
                    result = "vlan";
                    
                    break;
            }

            return result;
        }

        public static LinksysPortType GetLinksysPortType(string interfaceName)
        {
            LinksysPortType result;

            if (interfaceName.ToLower().StartsWith("vlan"))
            {
                result = LinksysPortType.Vlan;
            }
            else if (interfaceName.ToLower().StartsWith("ch"))
            {
                result = LinksysPortType.PortChannel;
            }
            else
            {
                result = LinksysPortType.Ethernet;
            }

            return result;
        }
    }

    internal enum LinksysPortType
    {
        Ethernet = 0,
        PortChannel = 1,
        Vlan = 2
    }
}
