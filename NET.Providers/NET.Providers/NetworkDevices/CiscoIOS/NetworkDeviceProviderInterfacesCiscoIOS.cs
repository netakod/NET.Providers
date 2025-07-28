using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.CiscoIOS)]
    public class NetworkDeviceProviderInterfacesCiscoIOS : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
		#region |   Interface Data  |

		public override string GetStandardizedName(string interfaceName)
		{
			if (interfaceName.TrimEnd().EndsWith("-802.1Q vLAN subif"))
				return interfaceName.Replace("-802.1Q vLAN subif", "");
			else
				return base.GetStandardizedName(interfaceName);
		}

        public override async ValueTask SetDescription(string interfaceName, string description)
        {
            //string result;
            //try
            //{
                //await base.SetDescription(interfaceName, description);
            //}
            //catch
            //{
                string ciscoDescription = description.IsNullOrEmpty() ? " " : description.Replace(' ', '_');

                await this.Provider.Terminal.EnterConfigModeAsync();
                
                string response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);
                
              
            if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} is not configurable or does not exists.", interfaceName));

			await this.Provider.Terminal.SendAsync("description " + ciscoDescription);
			await this.Provider.Terminal.SendAsync("exit");

			//}
		}

        public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
        {
            //try
            //{
            //    await base.SetAdminStatus(interfaceName, adminStatus);
            //}
            //catch
            //{
            await this.Provider.Terminal.EnterConfigModeAsync();
                
            string response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

            if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Error on set Admin Status: {0}", response));

			//if (interfaceName.ToLower().StartsWith("vlan")) // there is no shutdown command on vlan interfaces
			//{
			string adminStatusCommand = "shutdown";

				if (adminStatus != InterfaceAdminStatus.Down)
					adminStatusCommand = "no " + adminStatusCommand;

				response = await this.Provider.Terminal.SendAsync(adminStatusCommand);

			//if (response.ToLower().Contains("invalid"))
			//	throw new ProviderInfoException(String.Format("Error on set Admin Status: {0}\r\n{1}", adminStatusCommand, response));
			//}

			await this.Provider.Terminal.SendAsync("exit");
			//}
		}

		#endregion |   Interface Data   |

		#region |   Add Remove Interface   |

		public override bool IsAddRemoveSupported() => true;

        public override async ValueTask Add(string interfaceName)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

            if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: \r\n{1}", interfaceName, response)); // interface does not exists, just return

			await this.Provider.Terminal.SendAsync("no shutdown");
            await this.Provider.Terminal.SendAsync("exit");

            await this.GenerateInterfaceDictionary();
        }

        public override async ValueTask Remove(string interfaceName)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            //string response = this.Provider.Connection.Terminal.Send("interface " + interfaceName);
            //response = this.Provider.Connection.Terminal.Send("shutdown");
            //response = this.Provider.Connection.Terminal.Send("exit");
            await this.Provider.Terminal.SendAsync("no interface " + interfaceName);
            await this.GenerateInterfaceDictionary();
        }

        #endregion |   Add Remove Interface   |

        #region |   Interface IP Addresses   |

        public override async ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            //bool result = false;

            //if (interfaceName == null || interfaceName.Trim() == "")
            //    return result;

            //this.Provider.Terminal.ExitConfigMode();
            //string response = this.Provider.Terminal.Send("show interface " + interfaceName + " switchport");

            //if (!response.Contains("invalid"))
            SwitchportInfo switchportInfo = await this.GetSwitchportInfo(interfaceName);
            bool isIpAddressSupported = switchportInfo.SwitchportMode == InterfaceSwitchportMode.VlanIsNotSupported;

            return isIpAddressSupported;
        }

        public override async ValueTask<bool> IsWriteIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);

        public override async ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);


        public override async ValueTask<NetworkInfo> GetIpAddress(string interfaceName)
        {
            string response;
			//try
			//{
			//    return base.GetIpAddress(interfaceName);
			//}
			//catch
			//{
			NetworkInfo result; // new NetworkInfo("", "");

            await this.Provider.Terminal.ExitConfigModeAsync();
            response = await this.Provider.Terminal.SendAsync("sh ip int " + interfaceName + " | in Internet addre");

			if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: \r\n{1}", interfaceName, response)); // interface does not exists, just return
			
            try
			{
                if (response.Contains("address"))
                {
                    string[] responseArray = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ipAddressAndMaskBits = responseArray[responseArray.Length - 1].Split('/');
                    string ipAddressText = ipAddressAndMaskBits[0].Trim();
                    int ipSubnetMaskNumOfBits = Conversion.TryChangeType<int>(ipAddressAndMaskBits[1]);
                    string subnetMask = IpHelper.GetSubnetMask(ipSubnetMaskNumOfBits);

                    result = new NetworkInfo(ipAddressText, subnetMask);
                }
				else
				{
					result = new NetworkInfo("", 0);
				}
			}
            catch
            {
                result = new NetworkInfo("", 0);
            }

            return result;
            //}
        }

        public override async ValueTask SetIpAddress(string interfaceName, IpAddress? ipAddress, int subnetMaskPrefix)
        {
            string response;

			await this.Provider.Terminal.EnterConfigModeAsync();
			response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

			if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: \r\n{1}", interfaceName, response)); // interface does not exists, just return

			string ipAddressCommand = "ip address";

			if (ipAddress != null)
                 await this.Provider.Terminal.SendAsync(ipAddressCommand + " " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix));
            else    // no ip address to set - remove existing
				await this.Provider.Terminal.SendAsync("no " + ipAddressCommand);

			await this.Provider.Terminal.SendAsync("exit");
		}

		public override async ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName)
        {
            List<NetworkInfo> result = new List<NetworkInfo>();

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("sh ip int " + interfaceName + " | in Secondary addre");

            if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: {1}", interfaceName, response));

			if (response.Contains("address"))
            {
                string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                foreach (string line in responseArray)
                {
                    if (line.Trim().Length == 0)
                        continue;
                    
                    string[] lineArray = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ipAddressAndMaskBits = lineArray[lineArray.Length - 1].Split('/');
                    string ipAddress = ipAddressAndMaskBits[0].Trim();
                    int ipSubnetMaskNumOfBits = Conversion.TryChangeType<int>(ipAddressAndMaskBits[1]);

                    if (IpHelper.ValidateIpAddress(ipAddress))
                        result.Add(new NetworkInfo(ipAddress, ipSubnetMaskNumOfBits));
                }
            }

            return result;
        }

        public override async ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
        {
            return await this.IsSecondaryIpAddressSupported(interfaceName);
        }

        public override async ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            string response;
            //if (ipAddress() != "" && subnetMaskPrefix.Trim() != "")
            //{
                await this.Provider.Terminal.EnterConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

			if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: \r\n{1}", interfaceName, response)); // interface does not exists, just return

            await this.Provider.Terminal.SendAsync("ip address " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix) + " secondary");
            await this.Provider.Terminal.SendAsync("exit");
			//}
		}

        public override async ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            string response;
            //if (ipAddress.Trim() != "" && subnetMaskPrefix.Trim() != "")
            //{
            await this.Provider.Terminal.EnterConfigModeAsync();
            response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

			if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} does not exists: \r\n{1}", interfaceName, response)); // interface does not exists, just return

            await this.Provider.Terminal.SendAsync("no ip address " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix) + " secondary");
            await this.Provider.Terminal.SendAsync("exit");
		}

		//[ProviderAction("IPAddresses.GetKeys")]
		//public ProviderKeys GetIpAddressKeysByTerminal(string interfaceName)
		//{
		//    ProviderKeys keys = new ProviderKeys(ProviderKeyHelper.KeyNamesIPAddress);

		//    IPAddressInfo primaryIpAddress = this.GetPrimaryIpAddress(interfaceName);

		//    if (primaryIpAddress != null)
		//    {
		//        keys.AddKeyValue(ProviderKeyHelper.CreateIPAddressKey(primaryIpAddress.IPAddress, primaryIpAddress.IPSubnetMask));

		//        IPAddressInfo[] secondaryIpAddresses = this.GetSecondaryIpAddresses(interfaceName);

		//        foreach (IPAddressInfo secondaryIpAddressInfo in secondaryIpAddresses)
		//        {
		//            keys.AddKeyValue(ProviderKeyHelper.CreateIPAddressKey(secondaryIpAddressInfo.IPAddress, secondaryIpAddressInfo.IPSubnetMask));
		//        }
		//    }

		//    return keys;
		//}

		//[ProviderAction("IPAddresses.Add")]
		//public void AddIPAddress(string interfaceName, string ipAddress, string ipSubnetMask)
		//{
		//    string lineCmd = "ip address " + ipAddress + " " + ipSubnetMask;

		//    if (this.GetPrimaryIpAddress(interfaceName) != null)
		//    {
		//        lineCmd += " secondary";
		//    }

		//    this.Terminal.EnterConfigMode();

		//    string response = this.Terminal.SendSync("interface " + interfaceName);
		//    response = this.Terminal.SendSync(lineCmd);
		//    response = this.Terminal.SendSync("exit");
		//}

		///// <summary>
		///// Remove primary or secondary ip address/mask. If secondary ip address exist and primary address needs to be removed, 
		///// firs we must remove all secondary ip addresses, than to remove primary and finally restore secondary ip addresses
		///// positioning first one as primary.
		///// </summary>
		///// <param name="interfaceName">The name of the interface.</param>
		///// <param name="ipAddress">IP address.</param>
		///// <param name="ipSubnetMask">IP subnet mask.</param>
		//[ProviderAction("IPAddresses.Remove")]
		//public void RemoveIPAddress(string interfaceName, string ipAddress, string ipSubnetMask)
		//{
		//    string response;

		//    if (this.IsPrimaryIpAddress(interfaceName, ipAddress, ipSubnetMask))
		//    {
		//        // If secondary ip address exist and primary address needs to be removed, firs we must remove all secondary ip addresses,
		//        // than to remove primary and finally restore secondary ip addresses positioning first one as primary.
		//        IPAddressInfo[] secondaryIpAddresses = this.GetSecondaryIpAddresses(interfaceName);

		//        this.Terminal.EnterConfigMode();
		//        response = this.Terminal.SendSync("interface " + interfaceName);

		//        if (secondaryIpAddresses.Length > 0)
		//        {
		//            // Temporary remove secondary ip addresses
		//            foreach (IPAddressInfo ipAddressInfo in secondaryIpAddresses)
		//            {
		//                response = this.Terminal.SendSync("no ip address " + ipAddressInfo.IPAddress + " " + ipAddressInfo.IPSubnetMask + " secondary");
		//            }
		//        }

		//        response = this.Terminal.SendSync("no ip address " + ipAddress + " " + ipSubnetMask);

		//        // Restore secondary ip addresses positioning first one as primary.
		//        if (secondaryIpAddresses.Length > 0)
		//        {
		//            response = this.Terminal.SendSync("ip address " + secondaryIpAddresses[0].IPAddress + " " + secondaryIpAddresses[0].IPSubnetMask);

		//            for (int i = 1; i < secondaryIpAddresses.Length; i++)
		//            {
		//                response = this.Terminal.SendSync("ip address " + secondaryIpAddresses[i].IPAddress + " " + secondaryIpAddresses[i].IPSubnetMask + " secondary");
		//            }
		//        }

		//        // Exit interface
		//        response = this.Terminal.SendSync("exit");
		//    }
		//    else
		//    {
		//        this.Terminal.EnterConfigMode();
		//        response = this.Terminal.SendSync("interface " + interfaceName);
		//        response = this.Terminal.SendSync("no ip address " + ipAddress + " " + ipSubnetMask + " secondary");
		//        // Exit interface
		//        response = this.Terminal.SendSync("exit");
		//    }
		//}

		//[ProviderAction("IPAddresses.Update")]
		//public void SetIPAddress(string interfaceName, string oldIpAddress, string oldIpSubnetMask,  string newIpAddress, string newIpSubnetMask)
		//{
		//    string response; 

		//    if (this.IsPrimaryIpAddress(interfaceName, oldIpAddress, oldIpSubnetMask))
		//    {
		//        this.Terminal.EnterConfigMode();

		//        response = this.Terminal.SendSync("interface " + interfaceName);
		//        response = this.Terminal.SendSync("ip address " + newIpAddress + " " + newIpSubnetMask);
		//        response = this.Terminal.SendSync("exit");
		//    }
		//    else if (this.IsSecondaryIpAddress(interfaceName, oldIpAddress, oldIpSubnetMask))
		//    {
		//        this.Terminal.EnterConfigMode();

		//        response = this.Terminal.SendSync("interface " + interfaceName);
		//        response = this.Terminal.SendSync("no ip address " + oldIpAddress + " " + oldIpSubnetMask + " secondary");
		//        response = this.Terminal.SendSync("ip address " + newIpAddress + " " + newIpSubnetMask + " secondary");
		//        response = this.Terminal.SendSync("exit");
		//    }
		//}

		//[ProviderAction("IPAddresses.GenerateIPAddressDictionary")]
		//public void GenerateIPAddressDictionary()
		//{
		//    // This require only SNMP implementation while telnet get instant data from device and no need for cacheing.
		//}

		//public bool IsPrimaryIpAddress(string interfaceName, string ipAddress, string ipSubnetMask)
		//{
		//    bool result = false;

		//    IPAddressInfo primaryIpAddress = this.GetPrimaryIpAddress(interfaceName);
		//    if (primaryIpAddress != null)
		//    {
		//        if (primaryIpAddress.IPAddress.Trim() == ipAddress.Trim() && primaryIpAddress.IPSubnetMask.Trim() == ipSubnetMask.Trim())
		//        {
		//            result = true;
		//        }
		//    }

		//    return result;
		//}

		//public bool IsSecondaryIpAddress(string interfaceName, string ipAddress, string ipSubnetMask)
		//{
		//    bool result = false;
		//    IPAddressInfo[] secondaryIpAddresses = this.GetSecondaryIpAddresses(interfaceName);

		//    foreach (IPAddressInfo secondaryIpAddress in secondaryIpAddresses)
		//    {
		//        if (secondaryIpAddress.IPAddress.Trim() == ipAddress.Trim() && secondaryIpAddress.IPSubnetMask.Trim() == ipSubnetMask.Trim())
		//        {
		//            result = true;
		//            break;
		//        }
		//    }

		//    return result;
		//}


		#endregion |   Interface IP Addresses   |     

		#region |   Interface Services   |

		public override async ValueTask SetDhcpServer(string interfaceName, IpAddress? startIpAddress, IpAddress? endIpAddress, int subnetMaskPrefix, IpAddress? defaultGateway, IEnumerable<IpAddress> dnsServers, string domainName)
		{
			string response;
			string ipDhcpCommand = "ip dhcp pool";

			await this.Provider.Terminal.EnterConfigModeAsync();

			if (startIpAddress != null)
			{
				response = await this.Provider.Terminal.SendAsync("dhcp service"); // enable DHCp service
				response = await this.Provider.Terminal.SendAsync($"{ipDhcpCommand} {interfaceName}");
				response = await this.Provider.Terminal.SendAsync($"network {startIpAddress} {IpHelper.GetSubnetMask(subnetMaskPrefix)}");
				response = await this.Provider.Terminal.SendAsync($"default-router {defaultGateway}");

				string? dnsServersText = dnsServers.Count() == 0 ? defaultGateway?.ToString() : 
																   dnsServers.ToString(separator: " ");

				if (!dnsServersText.IsNullOrEmpty())
					response = await this.Provider.Terminal.SendAsync($"dns-server {dnsServersText}");

				if (domainName.Trim().Length > 0)
					response = await this.Provider.Terminal.SendAsync($"domain-name {domainName}");

				response = await this.Provider.Terminal.SendAsync("exit");
			}
			else // no dhcp-server
			{
				response = await this.Provider.Terminal.SendAsync($"no {ipDhcpCommand} {interfaceName}");
			}
		}

		#endregion |   Interface Services   |

		#region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			const string strSwitchport = "switchport";
			//const string strEnabled = "enabled";
			const string strDisabled = "disabled";
			const string strAdministrativeMode = "administrative mode";
			const string strAccessModeVlan = "access mode vlan:";
			const string strTrunk = "trunk";
			const string strNotASwitchable = "not a switchable";

			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			int vlanId = 1;

			if (interfaceName != null && interfaceName.Trim().Length > 0)
			{
				await this.Provider.Terminal.ExitConfigModeAsync();

				string response = await this.Provider.Terminal.SendAsync("show interface " + interfaceName + " switchport");
				string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

				foreach (string line in lines)
				{
					string lineToLower = line.ToLower();

					if (lineToLower.ContainsAll(strSwitchport, strDisabled) || lineToLower.Contains(strNotASwitchable))
					{
						switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;

						break;
					}
					if (lineToLower.ContainsAll(strAdministrativeMode, strTrunk))
					{
						switchportMode = InterfaceSwitchportMode.Trunk;

						break;
					}
					else if (lineToLower.Contains(strAccessModeVlan))
					{
						string restOfLine = lineToLower.Split(new string[] { strAccessModeVlan }, StringSplitOptions.None)[1];
						string vlanIdText = restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0];

						switchportMode = InterfaceSwitchportMode.Access;
						vlanId = Conversion.TryChangeType<int>(vlanIdText);

						break;
					}
				}
			}

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
  //      {
  //          const string strSwitchport = "switchport";
  //          const string strEnabled = "enabled";
  //          const string strAdministrativeMode = "administrative mode";
  //          const string strTrunk = "trunk";
  //          const string strNotASwitchable = "not a switchable";
  //          InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;

  //          if (interfaceName == null || interfaceName.Trim() == "")
  //          {
  //              return switchportMode;
  //          }
            
  //          this.Provider.Terminal.ExitConfigMode();
  //          string response = this.Provider.Terminal.Send("show interface " + interfaceName + " switchport");

  //          if (!response.Contains(strNotASwitchable))
  //          {
  //              //string stringToFind = strAdministrativeMode;
  //              string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

  //              foreach (string line in lines)
  //              {
  //                  string lineToLower = line.ToLower();

  //                  if (lineToLower.Contains(strSwitchport) && lineToLower.Contains(strEnabled))
  //                  {
  //                      switchportMode = InterfaceSwitchportMode.Access;
		//			}
		//			else if (lineToLower.Contains(strAdministrativeMode) && lineToLower.Contains(strTrunk))
  //                  {
  //                      switchportMode = InterfaceSwitchportMode.Trunk;
  //                      break;
  //                  }
  //                  //else
  //                  //{
  //                  //    //// If Admin Mode is e.g. dynamic -> check Opeational Mode that follow
  //                  //    //stringToFind = strOperationalMode;
  //                  //}
  //              }
  //          }

  //          return switchportMode;
  //      }

		public override async ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId)
		{
			string response;

			if (switchportMode == InterfaceSwitchportMode.DoubleTagging)
				throw new ProviderInfoException("Port double tagging is not supported.");

			await this.Provider.Terminal.EnterConfigModeAsync();

			response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

			if (response.ToLower().Contains("invalid"))
				throw new ProviderInfoException(String.Format("Interface {0} is not configurable or does not exists.", interfaceName));

			switch (switchportMode)
			{
				case InterfaceSwitchportMode.Access:

					await this.Provider.Terminal.SendAsync("switchport mode access");
					await this.Provider.Terminal.SendAsync("switchport access vlan " + vlanId.ToString());

					break;

				case InterfaceSwitchportMode.Trunk:

					await this.Provider.Terminal.SendAsync("switchport trunk encapsulation dot1q");
					await this.Provider.Terminal.SendAsync("switchport mode trunk");
					await this.Provider.Terminal.SendAsync("no switchport access vlan");
					await this.Provider.Terminal.SendAsync("switchport trunk native vlan 1");

					break;

				case InterfaceSwitchportMode.VlanIsNotSupported:

					await this.Provider.Terminal.SendAsync("no switchport");

					break;
			}

			await this.Provider.Terminal.SendAsync("exit");
		}

		//public override int GetVlanId(string interfaceName)
		//{
		//	const string strAccessModeVlan = "Access Mode VLAN:";
		//	const string strAdministrativeMode = "Administrative Mode:";

		//	int vlanId = 1;
		//	bool isTrunk = false;

		//	this.Provider.Terminal.ExitConfigMode();

		//	string response = this.Provider.Terminal.Send("show interface " + interfaceName + " switchport");

		//	foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
		//	{
		//		if (line.Contains(strAdministrativeMode) && line.ToLower().Contains("trunk"))
		//			isTrunk = true;

		//		if (line.Contains(strAccessModeVlan))
		//		{
		//			string restOfLine = line.Split(new string[] { strAccessModeVlan }, StringSplitOptions.None)[1];
		//			vlanId = Conversion.TryChangeType<int>(restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

		//			if (vlanId == 0 && isTrunk)
		//				vlanId = 1;
		//		}
		//	}

		//	return vlanId;
		//}

		//public override void SetVlanId(string interfaceName, int vlanId)
		//{
		//    this.Provider.Terminal.EnterConfigMode();
		//    string response = this.Provider.Terminal.Send("interface " + interfaceName);
		//    response = this.Provider.Terminal.Send("switchport access vlan " + vlanId.ToString());
		//    response = this.Provider.Terminal.Send("exit");
		//}

		public override async ValueTask<bool> IsL3InterfaceBasedOnVlan(string interfaceName)
		{
			string str802dotQ = "802.1q";
			bool result = await base.IsL3InterfaceBasedOnVlan(interfaceName);

			if (!result)
			{
				// Check if interface encapsulation is 802.1Q set
				                  await this.Provider.Terminal.ExitConfigModeAsync();
				string response = await this.Provider.Terminal.SendAsync(String.Format("show interface {0} | include Encapsulation", interfaceName));

				if (response != null && response.ToLower().Contains(str802dotQ))
					result = true;
			}

			return result;
		}

		public override async ValueTask<int> GetL3InterfaceBasedVlanId(string interfaceName)
		{
			string str802dotQ = "802.1q";
			string strId = "id";
			int vlanId = await base.GetL3InterfaceBasedVlanId(interfaceName);

			if (vlanId <= 0)
			{
				// Check if interface encapsulation is 802.1Q set
				                  await this.Provider.Terminal.ExitConfigModeAsync();
				string response = await this.Provider.Terminal.SendAsync(String.Format("show interface {0} | include capsulation", interfaceName));

				if (response != null && response.ToLower().Contains(str802dotQ))
				{
					string[] lineElements = response.Replace(".", "").ToLower().Split(new string[] { strId }, StringSplitOptions.RemoveEmptyEntries);

					if (lineElements.Length > 0)
					{
						string vlanIdText = lineElements[lineElements.Length - 1];
						
                        vlanId = Conversion.TryChangeType<int>(vlanIdText);
					}
				}
			}

			return vlanId;
		}

        #endregion |   Interface Vlans   |
       
        #region |   Interface ACL   |

        //[ProviderAction("Interfaces.IsInAclSupported")]
        //public virtual bool IsInAclSupported(string interfaceName)
        //{
        //    // TODO:
        //    return false;
        //}

        //[ProviderAction("Interfaces.IsOutAclSupported")]
        //public virtual bool IsOutAclSupported(string interfaceName)
        //{
        //    // TODO:
        //    return false;
        //}


        //[ProviderAction("Interfaces.GetAclName")]
        //private virtual void GetAclName(string interfaceName, InterfaceAclDirection aclDirection)
        //{
        //    // TODO:
        //    string strDirection = aclDirection == InterfaceAclDirection.In ? "Inbound" : "Outbound";
        //}


        #endregion |   Interface ACL   |
    }
}
