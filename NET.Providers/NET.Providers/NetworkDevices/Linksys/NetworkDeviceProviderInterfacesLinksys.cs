using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Simple;
using Simple.Network;
using NET.Tools.Snmp;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Linksys)]
    public class NetworkDeviceProviderInterfacesLinksys : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private List<string> trunkPortInterfaceNames = null;
        private Dictionary<string, int> portVlanIdsByInterfaceName = null;

		#endregion |   Private Members   |

		#region |   Public Properties   |

		public async ValueTask<List<string>> GetTrunkPortInterfaceNames()
        {
            if (this.trunkPortInterfaceNames == null)
            {
                this.trunkPortInterfaceNames = new List<string>();

                if (this.Provider.UseWeb)
                {
                    //string getRequest = @"Vmember/port2vlan.htm?[VlanStatic]Query:dot1qVlanIndex=10[VlanCurrent]Query:dot1qVlanTimeMark=0@dot1qVlanIndex=10[StaticNames]Filter:dot1qVlanStatus=2[PortMode]Filter:((ifOperStatus!=6%20%26%26%20ifIndex>=1%20%26%26%20ifIndex<=24)||%20(ifIndex>=25%20%26%26%20ifIndex<=32))";
                    string getRequest = @"Vmember/port2vlan.htm?[VlanStatic]Query:dot1qVlanIndex=1[VlanCurrent]Query:dot1qVlanTimeMark=0@dot1qVlanIndex=1[StaticNames]Filter:dot1qVlanStatus=2[PortMode]"; //Filter:(ifOperStatus!=6%20%26%26%20ifIndex>=1%20%26%26%20ifIndex<=1)";
                    var request = await this.Provider.Web.SendGetRequestAsync(getRequest, "home.htm");
                    var response = await request.GetResponseTextAsync();
                    HtmlDocument responseXtml = response.ToHtmlDocument();
                    int lastInterfaceIndex = 0;

                    foreach (HtmlNode input in responseXtml.DocumentNode.SelectNodes("//input"))
                    {
                        if (input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("ifIndex$repeat"))
                        {
                            lastInterfaceIndex = Conversion.TryChangeType<int>(input.Attributes["Value"].Value);
                        }
                        else if (lastInterfaceIndex > 0 && input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("vlanPortModeState$repeat"))
                        {
                            string vlanPortModeStateText = input.Attributes["Value"].Value;
                            int vlanPortModeState = Conversion.TryChangeType<int>(vlanPortModeStateText);

                            if (vlanPortModeState == (int)LinksysSwitchportMode.Trunk)
                            {
                                var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();
                                var interfaceItem = interfaceIndexesByInterfaceName.FirstOrDefault(item => item.Value == lastInterfaceIndex);

                                if (!interfaceItem.Equals(default(KeyValuePair<string, int>)))
                                    this.trunkPortInterfaceNames.Add(interfaceItem.Key);

                                lastInterfaceIndex = 0;
                            }
                        }
                    }
                }
                else
                {
                    var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

                    foreach (string interfaceName in interfaceIndexesByInterfaceName.Keys)
                    {
                        InterfaceSwitchportMode switchportMode = await this.GetSwitchportModeByTerminal(interfaceName);

                        if (switchportMode == InterfaceSwitchportMode.Trunk)
                            trunkPortInterfaceNames.Add(interfaceName);
                    }
                }
            }

            return this.trunkPortInterfaceNames;
        }

        public async ValueTask<Dictionary<string, int>> GetPortVlanIdsByInterfaceName()
        {
            if (this.portVlanIdsByInterfaceName == null)
            {
                this.portVlanIdsByInterfaceName = new Dictionary<string, int>();

                var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

                if (this.Provider.UseWeb && !this.Provider.UseTerminal)
                {
                    int minInterfaceIndex = interfaceIndexesByInterfaceName.Values.Min();
                    int maxInterfaceIndex = interfaceIndexesByInterfaceName.Values.Max();
                    string getRequest = @"Vmember/bridg_vlan_interfaceStngs_m.htm?[fromGeneral]Filter:(rlPhdPortsIfIndex>=" + minInterfaceIndex + @"%20&&%20rlPhdPortsIfIndex<=" + maxInterfaceIndex +
                                        @")[toGeneral]Filter:(rlPhdPortsIfIndex>=" + minInterfaceIndex + @"%20&&%20rlPhdPortsIfIndex<=" + maxInterfaceIndex + @")";
                    var request = await this.Provider.Web.SendGetRequestAsync(getRequest, "home.htm");
                    string responseText = await request.GetResponseTextAsync();
                    HtmlDocument doc = new HtmlDocument();
                    int lastInterfaceIndex = 0;
                    
                    doc.LoadHtml(responseText);

                    foreach (HtmlNode input in doc.DocumentNode.SelectNodes("//input"))
                    {
                        if (input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("rlPhdPortsIfIndex$repeats"))
                        {
                            lastInterfaceIndex = Conversion.TryChangeType<int>(input.Attributes["Value"].Value);
                        }
                        else if (input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("dot1qPvid$repeats"))
                        {
                            int vlanId = Conversion.TryChangeType<int>(input.Attributes["Value"].Value);

                            if (vlanId > 0)
                            {
                                var interfaceItem = interfaceIndexesByInterfaceName.FirstOrDefault(item => item.Value == lastInterfaceIndex);

                                if (!interfaceItem.Equals(default(KeyValuePair<string, int>)))
                                {
                                    this.portVlanIdsByInterfaceName.Add(interfaceItem.Key, vlanId);
                                    lastInterfaceIndex = 0;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (string interfaceName in interfaceIndexesByInterfaceName.Keys)
                    {
                        if (interfaceName.ToLower().StartsWith("vlan"))
                            continue;

                        int vlanId = await this.GetVlanIdByTerminal(interfaceName);
                        
                        this.portVlanIdsByInterfaceName.Add(interfaceName, vlanId);
                    }
                }
            }

            return this.portVlanIdsByInterfaceName;
        }

        #endregion |   Public Properties   |

        #region |   Interface   |

        public override async ValueTask<string> GetName(int interfaceIndex)
        {
            SnmpData snmpData = await this.Provider.Snmp.GetAsync(String.Format("{0}.{1}", SnmpOIDs.IfMIB.ifName, interfaceIndex));
            string interfaceName = this.GetStandardizedName(snmpData.Value);

			return interfaceName;
        }

        #endregion |   Interface   |

        #region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			int vlanId = 1;

			if (this.Provider.UseWeb && !this.Provider.UseTerminal)
			{
                var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

                switchportMode = (trunkPortInterfaceNames.Contains(interfaceName)) ? InterfaceSwitchportMode.Trunk : InterfaceSwitchportMode.Access;
			}
			else
			{
				LinksysPortType portType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);

				if (portType == LinksysPortType.Vlan)
				{
					switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
				}
				else
				{
                    switchportMode = await this.GetSwitchportModeByTerminal(interfaceName);
				}
			}

			if (switchportMode == InterfaceSwitchportMode.Access)
				vlanId = await this.GetVlanId(interfaceName);

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
		//{
		//	InterfaceSwitchportMode result;

		//	if (this.Provider.UseWeb && !this.Provider.UseTerminal)
		//	{
		//		result = (this.TrunkPortInterfaceNames.Contains(interfaceName)) ? InterfaceSwitchportMode.Trunk : InterfaceSwitchportMode.Access;
		//	}
		//	else
		//	{
		//		LinksysPortType portType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);

		//		if (portType == LinksysPortType.Vlan)
		//		{
		//			result = InterfaceSwitchportMode.VlanIsNotSupported;
		//		}
		//		else
		//		{
		//			result = this.GetSwitchportModeByTerminal(interfaceName);
		//		}
		//	}

		//	return result;
		//}

		public override async ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportnMode, int vlanId)
        {
			if (switchportnMode == InterfaceSwitchportMode.VlanIsNotSupported)
			{
				throw new ProviderInfoException("Port vlan mode is not supported.");
			}
			else if (switchportnMode == InterfaceSwitchportMode.DoubleTagging)
            {
                throw new ProviderInfoException("Port double tagging is not supported.");
            }
            else
            {
                if (this.Provider.UseWeb && !this.Provider.UseTerminal)
                {
                    int interfaceIndex = await this.GetIndex(interfaceName);
                    LinksysSwitchportMode linksysSwitchportMode = (switchportnMode == InterfaceSwitchportMode.Trunk) ? LinksysSwitchportMode.Trunk : LinksysSwitchportMode.Access;
                    string postData = @"restoreUrl=%5BfromGeneral%5DFilter%3A%28rlPhdPortsIfIndex%3E%3D1+%26%26+rlPhdPortsIfIndex%3C%3D12%29%5BtoGeneral%5DFilter%3A%28rlPhdPortsIfIndex%3E%3D1+%26%26+rlPhdPortsIfIndex%3C%3D12%29&errorCollector=&fromGeneral=OK&rlPhdPortsTable%24VT=OK&rlPhdPortsIfIndex%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&vlanPortModeState%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qPvid%24VT=Type%3D7%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D%3BDefault+value%3D1&dot1qPortAcceptableFrameTypes%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D2%3BRange0%3D%5B1%2C2%5D%3BDefault+value%3D1&dot1qPortIngressFiltering%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D2%3BRange0%3D%5B1%2C2%5D%3BDefault+value%3D1&dot3adAggPortActorAdminKey%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C65535%5D%3BDefault+value%3D0&ifOperStatus%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D7%3BRange0%3D%5B1%2C7%5D&fromGeneral%24endVT=OK&" +
                                      @"rlPhdPortsIfIndex%24repeat%3F12=" + interfaceIndex + @"&vlanPortModeState%24repeat%3F12=" + (int)linksysSwitchportMode + @"&fromGeneral%24endRepeat%3F12=OK&toGeneral=OK&rlPhdPortsTable%24VTs=OK&rlPhdPortsIfIndex%24VTs=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qPvid%24VTs=Type%3D7%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D%3BDefault+value%3D1&dot1qPortAcceptableFrameTypes%24VTs=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D2%3BRange0%3D%5B1%2C2%5D%3BDefault+value%3D1&dot1qPortIngressFiltering%24VTs=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D2%3BRange0%3D%5B1%2C2%5D%3BDefault+value%3D1&vlanPortModeState%24VTs=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&ifOperStatus%24VTs=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D7%3BRange0%3D%5B1%2C7%5D&toGeneral%24endVTs=OK";
                    var request = await this.Provider.Web.SendPostRequestAsync("Vmember/bridg_vlan_interfaceStngs_m.htm", "Vmember/bridg_vlan_interfaceStngs_m.htm?[fromGeneral]");
                    string responseText = await request.GetResponseTextAsync();
                    var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

                    if (switchportnMode == InterfaceSwitchportMode.Access)
                    {
						if (trunkPortInterfaceNames.Contains(interfaceName))
							trunkPortInterfaceNames.Remove(interfaceName);

                    }
                    else if (switchportnMode == InterfaceSwitchportMode.Trunk)
                    {
                        if (!trunkPortInterfaceNames.Contains(interfaceName))
                            trunkPortInterfaceNames.Add(interfaceName);
                    }

					string postReferer = @"Vmember/bridg_vlan_interfaceStngs_pop.htm?[VlanMemberStaticVT]Filter: " + interfaceIndex + @" IN dot1qVlanStaticEgressPorts[PortMode]Query:ifIndex=" + interfaceIndex;
					postData =           @"restoreUrl=%5BVlanMemberStaticVT%5DFilter%3A10+IN+dot1qVlanStaticEgressPorts%5BPortMode%5DQuery%3AifIndex%3D10&errorCollector=&PortMode=OK&vlanPortModeTable%24VT=OK&ifIndex%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B1%2C2147483647%5D&vlanPortModeState%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&PortMode%24endVT=OK&VlanStatic=OK&dot1qVlanStaticTable%24VT1=OK&dot1qVlanIndex%24VT1=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticEgressPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&VlanStatic%24endVT1=OK&dot1qVlanIndex%24repeat1%3F1=2&dot1qVlanStaticEgressPorts%24repeat1%3F1=00200000&dot1qVlanStaticUntaggedPorts%24repeat1%3F1=00200000&dot1qVlanForbiddenEgressPorts%24repeat1%3F1=00000000&VlanStatic%24endRepeat1%3F1=OK&" +
										 @"dot1qVlanIndex%24repeat1%3F4=" + vlanId.ToString() + @"&dot1qVlanStaticEgressPorts%24repeat1%3F4=00400000&dot1qVlanStaticUntaggedPorts%24repeat1%3F4=00400000&dot1qVlanForbiddenEgressPorts%24repeat1%3F4=00000000&VlanStatic%24endRepeat1%3F4=OK&VlanMemberStaticVT=OK&dot1qVlanStaticTable%24VT=OK&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&VlanMemberStaticVT%24endVT=OK";

                    request = await this.Provider.Web.SendPostRequestAsync("Vmember/bridg_vlan_interfaceStngs_pop.htm", postReferer, postData);
                    responseText = await request.GetResponseTextAsync();

					portVlanIdsByInterfaceName[interfaceName] = vlanId;

                    // TODO: Set Tagging for all vlans!!!
                }
                else
                {
                    string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(interfaceName);
                    string portModeCommand = (switchportnMode == InterfaceSwitchportMode.Trunk) ? "trunk" : "access";

                    await this.Provider.Terminal.EnterConfigModeAsync();
                    await this.Provider.Terminal.SendAsync(String.Format("interface {0} {1}", interfaceTypeCommand, interfaceName));

                    //If mode is trunk -> set tagging for all vlans (allow all vlans to be in trunk)
                    if (switchportnMode == InterfaceSwitchportMode.Trunk)
                    {
                        //IEnumerable<ProviderVlanInfo> vlanInfos = this.Provider.Vlans.GetVlans();

                        //this.Provider.Connection.Terminal.EnterConfigMode();
                        //response = this.Provider.Connection.Terminal.Send(String.Format("interface {0} {1}", interfaceTypeCommand, interfaceName));

                        //foreach (ProviderVlanInfo vlanInfo in vlanInfos)
                        //{
                        await this.Provider.Terminal.SendAsync("switchport mode trunk");
                        await this.Provider.Terminal.SendAsync("switchport trunk allowed vlan add all");
                        await this.Provider.Terminal.SendAsync("switchport trunk native vlan 1");
                        //}

                        //                    response = this.Provider.Connection.Terminal.Send("exit");

                        if (this.trunkPortInterfaceNames != null && !this.trunkPortInterfaceNames.Contains(interfaceName))
                            this.trunkPortInterfaceNames.Add(interfaceName);
                    }
                    else
                    {
                        //                  this.Provider.Connection.Terminal.EnterConfigMode();

                        await this.Provider.Terminal.SendAsync("switchport trunk allowed vlan remove all");
                        await this.Provider.Terminal.SendAsync("switchport mode acces");
						await this.Provider.Terminal.SendAsync("switchport access vlan " + vlanId);
						//                    response = this.Provider.Connection.Terminal.Send("exit");

						if (this.trunkPortInterfaceNames != null && this.trunkPortInterfaceNames.Contains(interfaceName))
                            this.trunkPortInterfaceNames.Remove(interfaceName);
                    }

                    await this.Provider.Terminal.SendAsync("exit");
                }
            }
        }

        //public override void SetVlanId(string interfaceName, int vlanId)
        //{
        //    if (this.Provider.UseWeb && !this.Provider.UseTerminal)
        //    {
        //        int interfaceIndex = this.GetIndex(interfaceName);
        //        string postReferer = @"Vmember/bridg_vlan_interfaceStngs_pop.htm?[VlanMemberStaticVT]Filter: " + interfaceIndex + @" IN dot1qVlanStaticEgressPorts[PortMode]Query:ifIndex=" + interfaceIndex;
        //        string postData = @"restoreUrl=%5BVlanMemberStaticVT%5DFilter%3A10+IN+dot1qVlanStaticEgressPorts%5BPortMode%5DQuery%3AifIndex%3D10&errorCollector=&PortMode=OK&vlanPortModeTable%24VT=OK&ifIndex%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B1%2C2147483647%5D&vlanPortModeState%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&PortMode%24endVT=OK&VlanStatic=OK&dot1qVlanStaticTable%24VT1=OK&dot1qVlanIndex%24VT1=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticEgressPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT1=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&VlanStatic%24endVT1=OK&dot1qVlanIndex%24repeat1%3F1=2&dot1qVlanStaticEgressPorts%24repeat1%3F1=00200000&dot1qVlanStaticUntaggedPorts%24repeat1%3F1=00200000&dot1qVlanForbiddenEgressPorts%24repeat1%3F1=00000000&VlanStatic%24endRepeat1%3F1=OK&" +
        //                          @"dot1qVlanIndex%24repeat1%3F4=" + vlanId.ToString() + @"&dot1qVlanStaticEgressPorts%24repeat1%3F4=00400000&dot1qVlanStaticUntaggedPorts%24repeat1%3F4=00400000&dot1qVlanForbiddenEgressPorts%24repeat1%3F4=00000000&VlanStatic%24endRepeat1%3F4=OK&VlanMemberStaticVT=OK&dot1qVlanStaticTable%24VT=OK&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&VlanMemberStaticVT%24endVT=OK";

        //        string responseText = this.Provider.Web.SendPostRequest("Vmember/bridg_vlan_interfaceStngs_pop.htm", postReferer, postData).GetResponseText();

        //        this.PortVlanIdsByInterfaceName[interfaceName] = vlanId;
        //    }
        //    else
        //    {
        //        string response;
        //        string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(interfaceName);

        //        this.Provider.Terminal.EnterConfigMode();

        //        response = this.Provider.Terminal.Send(String.Format("interface {0} {1}", interfaceTypeCommand, interfaceName));
        //        response = this.Provider.Terminal.Send(String.Format("switchport access vlan {0}", vlanId));
        //        response = this.Provider.Terminal.Send("exit");

        //        if (this.portVlanIdsByInterfaceName != null)
        //            this.portVlanIdsByInterfaceName[interfaceName] = vlanId;
        //    }
        //}

        #endregion |   Interface Vlans   |

        #region |   Interface IP Addresses   |

        public override ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            LinksysPortType linksysPortType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);

            return new ValueTask<bool>(linksysPortType == LinksysPortType.Vlan);
        }

        public override ValueTask<bool> IsWriteIpAddressSupported(string interfaceName)
        {
            return new ValueTask<bool>(true);
        }

        public override async ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            string response;
            string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(interfaceName);

                       await this.Provider.Terminal.EnterConfigModeAsync();
            response = await this.Provider.Terminal.SendAsync(String.Format("interface {0} {1}", interfaceTypeCommand, interfaceName));

            if (ipAddress != null)
            {
                response = await this.Provider.Terminal.SendAsync(String.Format("ip address {0} {1}", ipAddress.ToString(), IpHelper.GetSubnetMask(subnetMaskPrefix)));
            }
            else // no ip address to set - remove existing
            {
                response = await this.Provider.Terminal.SendAsync("no ip address");
            }

            response = await this.Provider.Terminal.SendAsync("exit");
        }

        public override ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName)
        {
            return new ValueTask<bool>(false);
        }

        public override ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
        {
            return new ValueTask<bool>(false);
        }

        #endregion |   Interface IP Addresses   |

        #region |   Interface ACL   |

        // ACL is not supported on Linksys

        //public override string CreateAclRule(InterfaceACLPermition permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, InterfaceACLPortCriteria sourcePortCriteria, ushort sourcePort, string destinationIpAddress, string destinationIpSubnetMask, InterfaceACLPortCriteria destinationPortCriteria, ushort destinationPort, bool established, bool logging)
        //{
        //    throw new ProviderInfoException("ACLs are not supported on Linksys.");
        //}

        #endregion |   Interface ACL   |

        #region |   Protected Methods   |

        protected override async ValueTask GenerateInterfaceDictionary()
        {
            Dictionary<string, int> newInterfaceIndexesByInterfaceName = new Dictionary<string, int>();
            var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

            await base.GenerateInterfaceDictionary();

            foreach (var item in interfaceIndexesByInterfaceName)
            {
                string interfaceName = item.Key;
                int interfaceIndex = item.Value;
                string ifMibName = (await this.Provider.Snmp.GetAsync(String.Format("{0}.{1}", SnmpOIDs.IfMIB.ifName, interfaceIndex))).ToString();

                if (!interfaceName.TrimStart().StartsWith("vlan"))
                {
                    interfaceName = ifMibName;
                    newInterfaceIndexesByInterfaceName.Add(interfaceName, interfaceIndex);
                }
                else if (ifMibName == "1") 
                {
                    newInterfaceIndexesByInterfaceName.Add("Vlan1", interfaceIndex);
                }
            }

            this.SetInterfaceIndexesByInterfaceName(newInterfaceIndexesByInterfaceName);
        }

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{
			int result;

			if (this.Provider.UseWeb && !this.Provider.UseTerminal)
			{
                var portVlanIdsByInterfaceName = await this.GetPortVlanIdsByInterfaceName();

                if (!portVlanIdsByInterfaceName.TryGetValue(interfaceName, out result))
					result = 1;
			}
			else
			{
				result = await this.GetVlanIdByTerminal(interfaceName);
			}

			return result;

			//int result = 0;
			//int interfaceIndex = this.GetIndex(interfaceName);
			//string getRequest = @"Vmember/bridg_vlan_interfaceStngs_m.htm?[fromGeneral]Filter:(rlPhdPortsIfIndex>=" + interfaceIndex + @"%20&&%20rlPhdPortsIfIndex<=" + interfaceIndex +
			//                    @")[toGeneral]Filter:(rlPhdPortsIfIndex>=" + interfaceIndex + @"%20&&%20rlPhdPortsIfIndex<=" + interfaceIndex + @")";

			//string responseText = this.Provider.Connection.Web.GetResponseText(getRequest, "home.htm");

			//HtmlDocument doc = new HtmlDocument();
			//doc.LoadHtml(responseText);

			//foreach (HtmlNode input in doc.DocumentNode.SelectNodes("//input"))
			//{
			//    //bool isNewInput = true;
			//    //bool isDot1qVlanIndexNode = false;
			//    //bool isDot1qVlanStaticName = false;

			//    if (input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("dot1qPvid$repeats"))
			//    {
			//        result = Conversion.TryChangeType<int>(input.Attributes["Value"].Value);
			//        break;
			//    }
			//}

			//return result;
		}

		#endregion |   Protected Methods   |

		#region |   Private Methods   |

		private async ValueTask<InterfaceSwitchportMode> GetSwitchportModeByTerminal(string interfaceName)
        {
            InterfaceSwitchportMode result = InterfaceSwitchportMode.VlanIsNotSupported;
            
            await this.Provider.Terminal.ExitConfigModeAsync();

            if (interfaceName.ToLower().StartsWith("vlan"))
            {
                result = InterfaceSwitchportMode.VlanIsNotSupported;
            }
            else
            {
                string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(interfaceName);
                LinksysPortType linksysPortType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);
                string interfaceSwitchportName = (linksysPortType == LinksysPortType.PortChannel) ? interfaceName.Substring(2) : interfaceName;

                                  await this.Provider.Terminal.ExitConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync(String.Format("show interface switchport {0} {1}", interfaceTypeCommand, interfaceSwitchportName));
                
                string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    string lineToLower = line.ToLower().Trim();

                    if (lineToLower.StartsWith("port mode"))
                    {
                        if (lineToLower.Contains("trunk"))
                            result = InterfaceSwitchportMode.Trunk;
                        else
                            result = InterfaceSwitchportMode.Access;

                        break;
                    }
                }
            }

            return result;
        }

        private async ValueTask<int> GetVlanIdByTerminal(String interfaceName)
        {
            int result = 0;
            string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(interfaceName);
            LinksysPortType linksysPortType = ProviderHelperLinksys.GetLinksysPortType(interfaceName);
            string interfaceSwitchportName = (linksysPortType == LinksysPortType.PortChannel) ? interfaceName.Substring(2) : interfaceName;

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync(String.Format("show interface switchport {0} {1}", interfaceTypeCommand, interfaceSwitchportName));
            string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                string lineToLower = line.ToLower().Trim();

                if (lineToLower.StartsWith("ingress untagged vlan"))
                {
                    string[] lineSegments = lineToLower.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string resultText = lineSegments[lineSegments.Length - 1];
                    
                    result = Conversion.TryChangeType<int>(resultText);

                    break;
                }
            }

            return result;
        }

		#endregion |   Private Methods   |

		#region |   Private Enums   |

		private enum LinksysSwitchportMode
		{
			General = 1,
			Access = 2,
			Trunk = 3
		}

		#endregion |   Private Enums   |
	}
}
