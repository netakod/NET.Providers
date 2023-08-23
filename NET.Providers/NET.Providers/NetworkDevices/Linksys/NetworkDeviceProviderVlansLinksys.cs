using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Linksys)]
    public class NetworkDeviceProviderVlansLinksys : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            List<VlanInfo> result = new List<VlanInfo>();

            if (this.Provider.UseWeb)
            {
                List<int> vlans = new List<int>();

                result.Add(new VlanInfo(1, "Default"));
                vlans.Add(1);

                var request = await this.Provider.Web.SendGetRequestAsync("Vmember/bridg_vlan_properties_a.htm", "home.htm");
                var response = await request.GetResponseTextAsync();
                HtmlDocument responseHtml = response.ToHtmlDocument();
                int lastVlanId = 0;

                foreach (HtmlNode input in responseHtml.DocumentNode.SelectNodes("//input"))
                {
                    if (input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("dot1qVlanIndex$repeat"))
                    {
                        lastVlanId = Conversion.TryChangeType<int>(input.Attributes["Value"].Value);
                    }
                    else if (lastVlanId > 0 && input.Attributes.Contains("Name") && input.Attributes["Name"].Value.StartsWith("dot1qVlanStaticName$repeat"))
                    {
                        string lastVlanName = input.Attributes["Value"].Value;

                        if (!lastVlanName.IsNullOrEmpty())
                        {
                            if (!vlans.Contains(lastVlanId))
                            {
                                result.Add(new VlanInfo(lastVlanId, lastVlanName));
                                vlans.Add(lastVlanId);
                            }

                            lastVlanId = 0;
                        }
                    }
                }
            }
            else
            {
                VlanInfo providerVlanInfo = VlanInfo.Empty;
                string response;

                           await this.Provider.Terminal.ExitConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("show vlan");
                
                string[][] vlanTable = ProviderHelper.GetTable(response, "----");

                foreach (string[] lineArray in vlanTable)
                {
                    if (lineArray.Length >= 2)
                    {
                        int vlanId = Conversion.TryChangeType<int>(lineArray[0]);
                        string vlanName = lineArray[1];

                        providerVlanInfo = new VlanInfo(vlanId, vlanName);
                        result.Add(providerVlanInfo);

                    }
                    else if (lineArray.Length == 1 && providerVlanInfo != VlanInfo.Empty)
                    {
                        providerVlanInfo.VlanName += lineArray[0];
                    }
                }
            }

            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
            if (this.Provider.UseWeb && !this.Provider.UseTerminal)
            {
                string postData = @"restoreUrl=&errorCollector=&VlanMemberStaticVT=OK&dot1qVlanStaticTable%24VT=OK&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticName%24VT=Type%3D2%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C32%5D&dot1qVlanStaticEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticRowStatus%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D&VlanMemberStaticVT%24endVT=OK&" +
                                  @"dot1qVlanIndex%24add=" + vlanId.ToString() + @"&String%24add=OK&dot1qVlanStaticName%24add=&Default%24add=OK&dot1qVlanStaticEgressPorts%24add=00&dot1qVlanForbiddenEgressPorts%24add=00&dot1qVlanStaticUntaggedPorts%24add=00&dot1qVlanStaticRowStatus%24add=4&VlanMemberStaticVT%24endAdd=OK&VlanInfo=OK&dot1qVlanCurrentTable%24VT=OK&dot1qVlanTimeMark%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStatus%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D3%3BRange0%3D%5B1%2C3%5D&VlanInfo%24endVT=OK";
                var request = await this.Provider.Web.SendPostRequestAsync("Vmember/bridg_vlan_properties_a.htm", "Vmember/bridg_vlan_properties_a.htm?[VlanMemberStaticVT]Query:dot1qVlanIndex=0", postData);
                string webResponseText = await request.GetResponseTextAsync();

				await this.SetName(vlanId, name);
			}
            else
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("vlan database");
                await this.Provider.Terminal.SendAsync($"vlan {vlanId} name {name}");
				await this.Provider.Terminal.SendAsync("exit");

                // Additionaly we need to allow this vlan (set tagging) for all trunk ports.
                NetworkDeviceProviderInterfacesLinksys networkDeviceProviderInterfacesLinksys = this.Provider.Interfaces as NetworkDeviceProviderInterfacesLinksys;
                List<string> trunkPortInterfaceNames = await networkDeviceProviderInterfacesLinksys.GetTrunkPortInterfaceNames();

                foreach (string trunkInterfaceName in trunkPortInterfaceNames)
                {
                    string interfaceTypeCommand = ProviderHelperLinksys.GetLinksysInterfaceTypeCommand(trunkInterfaceName);

                    await this.Provider.Terminal.EnterConfigModeAsync();
                    await this.Provider.Terminal.SendAsync($"interface {interfaceTypeCommand} {trunkInterfaceName}");
                    await this.Provider.Terminal.SendAsync($"switchport trunk allowed vlan add {vlanId}");
                    await this.Provider.Terminal.SendAsync("exit");
                }
            }
        }

        public override async ValueTask Remove(int vlanId)
        {
            if (this.Provider.UseWeb && !this.Provider.UseTerminal)
            {
                string postData = @"restoreUrl=&errorCollector=&VlanMemberStaticVT=OK&dot1qVlanStaticTable%24VT=OK&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticName%24VT=Type%3D2%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C32%5D&dot1qVlanStaticEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticRowStatus%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D&VlanMemberStaticVT%24endVT=OK&" +
                                  @"dot1qVlanIndex%24repeat1%3F6=" + vlanId.ToString() + @"&String%24repeat%3F6=OK&dot1qVlanStaticName%24repeat%3F6=&Default%24repeat%3F6=OK&dot1qVlanStaticRowStatus%24repeat1%3F6=6&VlanMemberStaticVT%24endRepeat%3F6=OK&String%24add=OK&Default%24add=OK&VlanInfo=OK&dot1qVlanCurrentTable%24VT=OK&dot1qVlanTimeMark%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStatus%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D3%3BRange0%3D%5B1%2C3%5D&VlanInfo%24endVT=OK";
                var request = await this.Provider.Web.SendPostRequestAsync("Vmember/bridg_vlan_properties_a.htm", "Vmember/bridg_vlan_properties_a.htm?[VlanMemberStaticVT]Query:dot1qVlanIndex=0", postData);
                string webResponseText = await request.GetResponseTextAsync();
            }
            else
            {
                string response;

                           await this.Provider.Terminal.EnterConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("vlan database");
                response = await this.Provider.Terminal.SendAsync($"no vlan {vlanId}");
                response = await this.Provider.Terminal.SendAsync("exit");
            }
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
            string result = String.Empty;
            string response;
            
                       await this.Provider.Terminal.ExitConfigModeAsync();
            response = await this.Provider.Terminal.SendAsync($"show vlan tag {vlanId}");
            
            string[][] vlanTable = ProviderHelper.GetTable(response, "----");

            if (vlanTable.Count() > 0)
                result = vlanTable[0][1];

            //string result = this.Provider.Interfaces.GetDescription("Vlan" + vlanId);

            return result;
        }

        public override async ValueTask SetName(int vlanId, string vlanName)
        {
            string newVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim();
            
            if (this.Provider.UseWeb && !this.Provider.UseTerminal)
            {
                string postData = @"restoreUrl=%5BVlanMemberStaticVT%5DQuery%3Adot1qVlanIndex%3D0&errorCollector=&VlanMemberStaticVT=OK&dot1qVlanStaticTable%24VT=OK&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStaticName%24VT=Type%3D2%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C32%5D&dot1qVlanStaticEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanForbiddenEgressPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticUntaggedPorts%24VT=Type%3D101%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D&dot1qVlanStaticRowStatus%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D&VlanMemberStaticVT%24endVT=OK&" +
                                  @"dot1qVlanIndex%24repeat1%3F5=" + vlanId.ToString() + @"&String%24repeat%3F5=OK&dot1qVlanStaticName%24repeat%3F5=" + newVlanName + @"&Default%24repeat%3F5=OK&VlanMemberStaticVT%24endRepeat%3F5=OK&String%24add=OK&Default%24add=OK&VlanInfo=OK&dot1qVlanCurrentTable%24VT=OK&dot1qVlanTimeMark%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanIndex%24VT=Type%3D7%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&dot1qVlanStatus%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D3%3BRange0%3D%5B1%2C3%5D&VlanInfo%24endVT=OK";
                var request = await this.Provider.Web.SendPostRequestAsync("Vmember/bridg_vlan_properties_a.htm", "Vmember/bridg_vlan_properties_a.htm?[VlanMemberStaticVT]Query:dot1qVlanIndex=0", postData);
                string webResponseText = await request.GetResponseTextAsync();
            }
            else
            {
                string response;
                newVlanName = vlanName.Replace(' ', '_');

                           await this.Provider.Terminal.EnterConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync($"interface vlan {vlanId}");
                response = await this.Provider.Terminal.SendAsync($"name {newVlanName}");
                response = await this.Provider.Terminal.SendAsync("exit");

                // Alternative
                //this.Provider.Interfaces.SetDescription("Vlan" + vlanId, vlanName);
            }
        }
    }
}
