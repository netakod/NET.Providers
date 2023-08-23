using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Dell)]
    public class NetworkDeviceProviderInterfacesDell : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private List<string> switchPortShorInterfaceNames = null;
        //private List<string> trunkPortInterfaceNames = null;
        private TrunkPorts trunkPortHelper = null;

		#endregion |   Private Members   |

		#region |   Public Properties   |

		public async ValueTask<List<string>> GetSwitchPortShortInterfaceNames()
        {
            if (this.switchPortShorInterfaceNames == null)
            {
                this.switchPortShorInterfaceNames = new List<string>();
                await this.Provider.Terminal.ExitConfigModeAsync();
                    
                string response = await this.Provider.Terminal.SendAsync("show interfaces status");
                string[][] table = ProviderHelper.GetTable(response, "----");
                //string[] responseArray = response.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string[] lineArray in table)
                    this.switchPortShorInterfaceNames.Add(lineArray[0]);
            }

            return this.switchPortShorInterfaceNames;
        }

        public TrunkPorts TrunkPortHelper
        {
            get
            {
                if (this.trunkPortHelper == null)
                    this.trunkPortHelper = new TrunkPorts(this);

                return this.trunkPortHelper;
            }
        }

        #endregion |   Private Properties   |

        #region |   Interface Data  |

        public override async ValueTask<string> GetDescription(string interfaceName)
        {
            string result = String.Empty;

            if (interfaceName.ToLower().StartsWith("vlan"))
            {
                string vlanIdText = interfaceName.Substring(4);
                int vlanId = Conversion.TryChangeType<int>(interfaceName.Substring(4));

                if (vlanId > 0)
                    result = await this.Provider.Vlans.GetName(vlanId);
            }
            else
            {
                result = await base.GetDescription(interfaceName);
            }

            return result;
        }
        
        public override async ValueTask SetDescription(string interfaceName, string description)
        {
			try
			{
				await base.SetDescription(interfaceName, description);
			}
			catch
			{
				string newInterfaceName = (interfaceName.ToLower().StartsWith("vlan")) ? "vlan " + interfaceName.Substring(4) : interfaceName;
                string newDescription = description.IsNullOrEmpty() ? " " : description.Replace(' ', '_');
                string descriptionCommand = (interfaceName.ToLower().StartsWith("vlan")) ? "name" : "description";

                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);
                
                if (description == null || description.Trim().Length == 0)
                {
                    descriptionCommand = "no " + descriptionCommand;
                }
                else
                {
                    descriptionCommand = descriptionCommand + " " + newDescription;
                }

                await this.Provider.Terminal.SendAsync(descriptionCommand);
                await this.Provider.Terminal.SendAsync("exit");
			}
		}

		public override async ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName)
        {
            const string strRoutingIsNotEnabled = "routing is not enabled";
            const string strRoutingMode = "routing mode";
            const string strEnable = "enable";
            InterfaceAdminStatus result = InterfaceAdminStatus.Down;
            int interfaceIndex = await this.GetIndex(interfaceName);
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

            if (dellDeviceType == DellDeviceType.PowerConnect62xx && interfaceName.ToLower().StartsWith("vlan"))
            {
                string vlanIdText = interfaceName.Substring(4);
                string newInterfaceName = "vlan " + vlanIdText;
                int vlanId = Conversion.TryChangeType<int>(vlanIdText);

                if (vlanId > 0 && vlanId == await this.GetManagementVlanId())
                {
                    result = InterfaceAdminStatus.Up;
                }
                else
                {
                                      await this.Provider.Terminal.ExitConfigModeAsync();
                    string response = await this.Provider.Terminal.SendAsync("show ip interface " + newInterfaceName);

                    if (!response.ToLower().Contains(strRoutingIsNotEnabled))
                    {
                        string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                        foreach (string line in responseArray)
                        {
                            if (line.ToLower().TrimStart().StartsWith(strRoutingMode) && line.ToLower().Contains(strEnable))
                            {
                                result = InterfaceAdminStatus.Up;
                                
                                break;
                            }
                        }
                    }
                    else
                    {
                        result = InterfaceAdminStatus.Down;
                    }
                }
            }
			else
            {
                result = await base.GetAdminStatus(interfaceName);
            }

            return result;
        }
        
        public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
        {
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

            if (dellDeviceType == DellDeviceType.PowerConnect62xx && interfaceName.ToLower().StartsWith("vlan"))
			{
				string newInterfaceName = "vlan " + interfaceName.Substring(4);

				await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);

				if (adminStatus == InterfaceAdminStatus.Up)
				{
					await this.Provider.Terminal.SendAsync("routing");
				}
				else
				{
					await this.Provider.Terminal.SendAsync("no routing");
				}

				await this.Provider.Terminal.SendAsync("exit");
			}
			else if (dellDeviceType == DellDeviceType.DellNetworkingNxxxx && interfaceName.ToLower().StartsWith("vlan"))
			{
				// No shutdown command on vlan interface
			}
			else
			{
				                  await this.Provider.Terminal.EnterConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync("interface " + interfaceName);

				if (!response.ToLower().Contains("invalid"))
				{
					string adminStatusCommand = "shutdown";

					if (adminStatus == InterfaceAdminStatus.Up)
						adminStatusCommand = "no " + adminStatusCommand;

					response = await this.Provider.Terminal.SendAsync(adminStatusCommand);
					           await this.Provider.Terminal.SendAsync("exit");

					if (response.ToLower().Contains("invalid"))
						throw new ProviderInfoException(String.Format("Error on set Admin Status: {0}\r\n{1}", adminStatusCommand, response));
				}
				else
				{
					try
					{
						await base.SetAdminStatus(interfaceName, adminStatus);
					}
					catch
					{
						throw new ProviderInfoException(String.Format("Error on set Admin Status: {0}", response));
					}
				}
			}
        }

        #endregion |   Interface Data   |

        #region |   Add Remove Interface   |

        public override bool IsAddRemoveSupported() => true;

        public override async ValueTask Add(string interfaceName)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("interface " + interfaceName);
            await this.Provider.Terminal.SendAsync("no shutdown");
            await this.Provider.Terminal.SendAsync("exit");

            await this.GenerateInterfaceDictionary();
            await this.TrunkPortHelper.InterfaceAdded(interfaceName);
        }

        public override async ValueTask Remove(string interfaceName)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            //string response = this.Provider.Connection.Terminal.Send("interface " + interfaceName);
            //response = this.Provider.Connection.Terminal.Send("shutdown");
            //response = this.Provider.Connection.Terminal.Send("exit");
            await this.Provider.Terminal.SendAsync("no interface " + interfaceName);
            
            await this.GenerateInterfaceDictionary();
            this.TrunkPortHelper.InterfaceRemoved(interfaceName);
        }

        #endregion |   Add Remove Interface   |

        #region |   Interface IP Addresses   |

        public override ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            bool result = false;

            if (interfaceName == null || interfaceName.Trim() == "")
                result = false;
            else if (interfaceName.ToLower().TrimStart().StartsWith("vlan") || Conversion.TryChangeType<int>(interfaceName, 0) > 0) // If interface name is 'Vlan5' or '5'
                result = true;

            //this.Provider.DeviceConnection.Terminal.ExitConfigMode();
            //string response = this.Provider.DeviceConnection.Terminal.Send("show interface " + interfaceName + " switchport");

            //if (!response.ToLower().Contains("wrong"))
            //{
            //    result = !this.IsVlanSupported(interfaceName);
            //}

            return new ValueTask<bool>(result);
        }

        public override async ValueTask<bool> IsWriteIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);

        public override async ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);

        public override async ValueTask<NetworkInfo?> GetIpAddress(string interfaceName)
        {
            const string strPrimaryIpAddress = "primary ip address";
            const string strRoutingIsNotaEnabled = "routing is not enabled";
            NetworkInfo result = null;
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

            if (dellDeviceType != DellDeviceType.GeneralDellDevice && interfaceName.ToLower().StartsWith("vlan"))
            {
                string vlanIdText = interfaceName.Substring(4);
                int vlanId = Conversion.TryChangeType<int>(vlanIdText);
                int managementVlanId = await this.GetManagementVlanId();

                if (vlanId == managementVlanId)
                {
                    result = await this.GetManagementIpAddressInfo();
                }
                else
                {
                                      await this.Provider.Terminal.ExitConfigModeAsync();
                    string response = await this.Provider.Terminal.SendAsync("show ip interface vlan " + vlanId);

                    if (!response.ToLower().Contains(strRoutingIsNotaEnabled))
                    {
                        string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                        foreach (string line in responseArray)
                        {
                            if (line.ToLower().TrimStart().StartsWith(strPrimaryIpAddress))
                            {
                                string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.None);
                                string[] ipAddressArray = lineArray[1].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                                result = new NetworkInfo(ipAddressArray[0], ipAddressArray[1]);

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                result = await base.GetIpAddress(interfaceName);
            }

            return result;
        }

        public override async ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            string newInterfaceName = (interfaceName.ToLower().StartsWith("vlan")) ? "vlan " + interfaceName.Substring(4) : interfaceName;

            if (ipAddress != null)
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);
                await this.Provider.Terminal.SendAsync("ip address " + ipAddress.ToString() + " " +  IpHelper.GetSubnetMask(subnetMaskPrefix));
                await this.Provider.Terminal.SendAsync("exit");
            }
            else    // no ip address to set - remove existing
            {
                //IPAddressInfo oldIpAddressInfo = this.GetIpAddress(interfaceName);

                //if (oldIpAddressInfo.IPAddress.Trim().Length > 0 && oldIpAddressInfo.IPSubnetMask.Trim().Length > 0)
                //{
                //    this.Provider.DeviceConnection.Terminal.EnterConfigMode();

                //    response = this.Provider.DeviceConnection.Terminal.Send("interface " + interfaceName);
                //    response = this.Provider.DeviceConnection.Terminal.Send("no ip address " + oldIpAddressInfo.IPAddress);
                //    response = this.Provider.DeviceConnection.Terminal.Send("exit");
                //}

                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);
                await this.Provider.Terminal.SendAsync("no ip address");
                await this.Provider.Terminal.SendAsync("exit");
            }
        }

        public override async ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName)
        {
            const string strSecondaryIpAddress = "secondary ip address";
            const string strRoutingIsNotaEnabled = "routing is not enabled";
            bool isSecondaryIpAddressLineDetected = false;
            List<NetworkInfo> result = new List<NetworkInfo>();
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

            if (dellDeviceType != DellDeviceType.GeneralDellDevice && interfaceName.ToLower().StartsWith("vlan"))
            {
                string vlanIdText = interfaceName.Substring(4);
                int vlanId = Conversion.TryChangeType<int>(vlanIdText);

                                  await this.Provider.Terminal.ExitConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync("show ip interface  vlan " + vlanId);

                if (!response.ToLower().Contains(strRoutingIsNotaEnabled))
                {
                    string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    
                    foreach (string line in responseArray)
                    {
                        if (line.ToLower().TrimStart().StartsWith(strSecondaryIpAddress))
                        {
                            string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.None);
                            string[] ipAddressArray = lineArray[1].Split(new string[] { "/" }, StringSplitOptions.None);

                            result.Add(new NetworkInfo(ipAddressArray[0], ipAddressArray[1]));

                            isSecondaryIpAddressLineDetected = true;
                        }
                        else if (isSecondaryIpAddressLineDetected)
                        {
                            if (line.StartsWith("..."))
                            {
                                string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.None);
                                string[] ipAddressArray = lineArray[1].Split(new string[] { "/" }, StringSplitOptions.None);

                                result.Add(new NetworkInfo(ipAddressArray[0], ipAddressArray[1]));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                return await base.GetSecondaryIpAddresses(interfaceName);
            }

            return result;
        }

        public override async ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
        {
            return await this.IsSecondaryIpAddressSupported(interfaceName);
        }

        public override async ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            //if (ipAddress.Trim() != "" && subnetMaskPrefix.Trim() != "")
            //{
                DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
                string newInterfaceName = (interfaceName.ToLower().StartsWith("vlan")) ? "vlan " + interfaceName.Substring(4) : interfaceName;
                string commandSecondary = (dellDeviceType != DellDeviceType.GeneralDellDevice) ? " secondary" : "";

                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);
                await this.Provider.Terminal.SendAsync("ip address " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix) + commandSecondary);
                await this.Provider.Terminal.SendAsync("exit");
            //}
        }

        public override async ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            //if (ipAddress.Trim() != "" && ipSubnetMask.Trim() != "")
            //{
                DellDeviceType dellDeviceType = await  (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
                string newInterfaceName = (interfaceName.ToLower().StartsWith("vlan")) ? "vlan " + interfaceName.Substring(4) : interfaceName;
                string commandSecondary = (dellDeviceType != DellDeviceType.GeneralDellDevice) ? " secondary" : "";

                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + newInterfaceName);
                await this.Provider.Terminal.SendAsync("no ip address " + ipAddress.ToString() + commandSecondary);
                await this.Provider.Terminal.SendAsync("exit");
            //}
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

        #region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			const string strSwitchport = "switchport";
			const string strEnabled = "enable";
			const string strVlanMembershipMode = "vlan membership mode";
			const string strAccess = "access";
			const string strAdministrativeMode = "administrative mode";
			const string strOperationalMode = "operational mode";
			//const string strAccess = "access";
			const string strTrunk = "trunk";
			const string strGeneral = "general";
			const string strNotASwitchable = "not a switchable";
			const string strNotPresent = "not present";
			const string strAccessModeVlan = "access mode vlan:";
			const string strPvid = "pvid";

			DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			int vlanId = 1;

			if (interfaceName != null || interfaceName.Trim().Length == 0 || (dellDeviceType == DellDeviceType.GeneralDellDevice && !(await this.IsActiveSwitchPort(interfaceName))))
				return new SwitchportInfo(switchportMode, vlanId);

			                  await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show interfaces switchport " + interfaceName);

			if (!response.Contains(strNotASwitchable))
			{
				string[] lines = response.ToLines(); // Split(new string[] { "\r\n" }, StringSplitOptions.None);

				foreach (string line in lines)
				{
					string lineToLower = line.ToLower();

					// Get switchport mode
					if (dellDeviceType == DellDeviceType.GeneralDellDevice)
					{
						if (lineToLower.ContainsAll(strAdministrativeMode, strTrunk))
						{
							switchportMode = InterfaceSwitchportMode.Trunk;

							break;
						}
						else if (lineToLower.ContainsAll(strSwitchport, strEnabled))
						{
							switchportMode = InterfaceSwitchportMode.Access;

							continue;
						}
						else if (lineToLower.ContainsAll(strAdministrativeMode, strAccess))
						{
							switchportMode = InterfaceSwitchportMode.Access;

							continue;
						}
						else if (lineToLower.ContainsAll(strOperationalMode, strNotPresent))
						{
							switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;

							break;
						}
					}
					else
					{
						if (lineToLower.ContainsAll(strVlanMembershipMode, strTrunk) || lineToLower.Contains(strGeneral))
						{
							switchportMode = InterfaceSwitchportMode.Trunk;

							break;
						}
						else if (lineToLower.ContainsAll(strVlanMembershipMode, strAccess))
						{
							switchportMode = InterfaceSwitchportMode.Access;

							continue;
						}
					}

					// Get vlanId
					if (dellDeviceType == DellDeviceType.PowerConnect62xx)
					{
						if (lineToLower.Contains(strPvid))
						{
							string restOfLine = line.Substring(strPvid.Length + 1);
							
                            vlanId = Conversion.TryChangeType<int>(restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

							break;
						}
					}
					else
					{
						if (lineToLower.Contains(strAccessModeVlan))
						{
							string restOfLine = line.ToLower().Split(new string[] { strAccessModeVlan }, StringSplitOptions.None)[1];
							
                            vlanId = Conversion.TryChangeType<int>(restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

							break;
						}
					}
				}
			}

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
		//{
		//	const string strSwitchport = "switchport";
		//	const string strEnabled = "enable";
		//	const string strVlanMembershipMode = "vlan membership mode";
		//	const string strAccess = "access";
		//	const string strAdministrativeMode = "administrative mode";
		//	const string strOperationalMode = "operational mode";
		//	//const string strAccess = "access";
		//	const string strTrunk = "trunk";
		//	const string strGeneral = "general";
		//	const string strNotASwitchable = "not a switchable";
		//	const string strNotPresent = "not present";
		//	InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
		//	DellDeviceType dellDeviceType = (this.Provider.Terminal as TerminalControlDell).GetDellDeviceType();

		//	if (interfaceName == null || interfaceName.Trim().Length == 0 || (dellDeviceType == DellDeviceType.GeneralDellDevice && !this.IsActiveSwitchPort(interfaceName)))
		//		return InterfaceSwitchportMode.VlanIsNotSupported;

		//	this.Provider.Terminal.ExitConfigMode();
		//	string response = this.Provider.Terminal.Send("show interfaces switchport " + interfaceName);

		//	if (!response.Contains(strNotASwitchable))
		//	{
		//		string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

		//		foreach (string line in lines)
		//		{
		//			string lineToLower = line.ToLower();

		//			// Get switchport mode
		//			if (dellDeviceType == DellDeviceType.GeneralDellDevice)
		//			{
		//				if (lineToLower.Contains(strAdministrativeMode, strTrunk))
		//				{
		//					switchportMode = InterfaceSwitchportMode.Trunk;

		//					break;
		//				}
		//				else if (lineToLower.Contains(strSwitchport, strEnabled))
		//				{
		//					switchportMode = InterfaceSwitchportMode.Access;

		//					continue;
		//				}
		//				else if (lineToLower.Contains(strAdministrativeMode, strAccess))
		//				{
		//					switchportMode = InterfaceSwitchportMode.Access;

		//					continue;
		//				}
		//				else if (lineToLower.Contains(strOperationalMode, strNotPresent))
		//				{
		//					switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;

		//					break;
		//				}
		//			}
		//			else
		//			{
		//				if (lineToLower.Contains(strVlanMembershipMode, strTrunk) || lineToLower.Contains(strGeneral))
		//				{
		//					switchportMode = InterfaceSwitchportMode.Trunk;

		//					break;
		//				}
		//				else if (lineToLower.Contains(strVlanMembershipMode, strAccess))
		//				{
		//					switchportMode = InterfaceSwitchportMode.Access;

		//					continue;
		//				}
		//			}
		//		}
		//	}

		//	return switchportMode;
		//}

		public override async ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId)
        {
			if (switchportMode == InterfaceSwitchportMode.VlanIsNotSupported)
			{
				throw new ProviderInfoException("Port vlan mode is not supported.");
			}
			else if (switchportMode == InterfaceSwitchportMode.DoubleTagging)
			{
				throw new ProviderInfoException("Port double tagging is not supported.");
			}
			else
			{
                DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
                    
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + interfaceName);
                //response = this.Provider.DeviceConnection.Terminal.Send("switchport");

                switch (switchportMode)
                {
                    case InterfaceSwitchportMode.Access:

						await this.Provider.Terminal.SendAsync("switchport mode access");
						await this.Provider.Terminal.SendAsync("switchport access vlan " + vlanId.ToString());

						break;

                    case InterfaceSwitchportMode.Trunk:

						if (dellDeviceType == DellDeviceType.DellNetworkingNxxxx)
						{
							await this.Provider.Terminal.SendAsync("switchport mode trunk");
						}
						else if (dellDeviceType == DellDeviceType.GeneralDellDevice)
						{
							await this.Provider.Terminal.SendAsync("switchport mode trunk");
							await this.Provider.Terminal.SendAsync("switchport general allowed vlan add 1");
							await this.Provider.Terminal.SendAsync("switchport general allowed vlan add 2-4093 tagged");
						}
						else
                        {
                            await this.Provider.Terminal.SendAsync("switchport mode general");
                            await this.Provider.Terminal.SendAsync("switchport trunk allowed vlan add 1-4093");
                        }
                        
                        break;
				}

				await this.Provider.Terminal.SendAsync("exit");
                
                this.TrunkPortHelper.UpdateTrunkPortInterfaceNames(interfaceName, switchportMode);
            }
        }

        //public override int GetVlanId(string interfaceName)
        //{
        //    const string strAccessModeVlan = "access mode vlan:";
        //    const string strPvid = "pvid";
        //    int vlanId = 1;
        //    DellDeviceType dellDeviceType = (this.Provider.Terminal as TerminalControlDell).GetDellDeviceType();

        //    this.Provider.Terminal.ExitConfigMode();

        //    string response = this.Provider.Terminal.Send("show interface switchport " + interfaceName);

        //    foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
        //    {
        //        if (dellDeviceType == DellDeviceType.PowerConnect62xx)
        //        {
        //            if (line.ToLower().Contains(strPvid))
        //            {
        //                string restOfLine = line.Substring(strPvid.Length + 1);
        //                vlanId = Conversion.TryChangeType<int>(restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

        //                break;
        //            }
        //        }
        //        else
        //        {
        //            if (line.ToLower().Contains(strAccessModeVlan))
        //            {
        //                string restOfLine = line.ToLower().Split(new string[] { strAccessModeVlan }, StringSplitOptions.None)[1];
        //                vlanId = Conversion.TryChangeType<int>(restOfLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

        //                break;
        //            }
        //        }
        //    }

        //    return vlanId;
        //}

        //public override void SetVlanId(string interfaceName, int vlanId)
        //{
        //    this.Provider.Terminal.EnterConfigMode();
        //    string response = this.Provider.Terminal.Send("interface " + interfaceName);
        //    response = this.Provider.Terminal.Send("switchport access vlan " + vlanId.ToString());
        //    response = this.Provider.Terminal.Send("exit");
        //}

        //public override bool IsLayer3InterfaceBasedOnVlan(string interfaceName)
        //{
        //    int vlanId = Conversion.TryChangeType<int>(interfaceName, 0);
        //    return vlanId > 0;
        //}

        //public override int GetLayer3InterfaceBasedVlanId(string interfaceName)
        //{
        //    int vlanId = Conversion.TryChangeType<int>(interfaceName, 0);
        //    return vlanId;
        //}


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

        #region |   Protected Methods   |

        protected override async ValueTask GenerateInterfaceDictionary()
        {
            const string strRoutingIsNotEnabled = "routing is not enabled";
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
            
            await base.GenerateInterfaceDictionary();

            var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

            if (dellDeviceType == DellDeviceType.GeneralDellDevice)
            {
				Dictionary<string, int> newInterfaceIndexesByInterfaceName = new Dictionary<string, int>();

                foreach (var interfaceIndexesByInterfaceNameItem in interfaceIndexesByInterfaceName)
				{
					string interfaceName = interfaceIndexesByInterfaceNameItem.Key;
					int interfaceIndex = interfaceIndexesByInterfaceNameItem.Value;
					int interfaceNameToNumber = Conversion.TryChangeType<int>(interfaceName, 0);

					if (interfaceNameToNumber > 0 && interfaceNameToNumber <= 4096)
						interfaceName = "Vlan" + interfaceNameToNumber;

					newInterfaceIndexesByInterfaceName.Add(interfaceName, interfaceIndex);
				}

				this.SetInterfaceIndexesByInterfaceName(newInterfaceIndexesByInterfaceName);
			}
			else // if (dellDeviceType == DellDeviceType.PowerConnect62xx || dellDeviceType == DellDeviceType.DellNetworkingNxxxx)
            {
                Dictionary<string, int> newInterfaceIndexesByInterfaceName = new Dictionary<string, int>();
                List<string> ethernetInterfaceNames = new List<string>();
                List<string> portChannelInterfaceNames = new List<string>();
                int i = 0, j = 0;
                int layer3InterfaceIndex = 10000;
                int managementVlanId = -1;
                IEnumerable<VlanInfo> vlans = await this.Provider.Vlans.GetVlans();
				string ethernetInterfacePrefix = (dellDeviceType == DellDeviceType.PowerConnect62xx) ? "Ethernet " : "";
				string portChanelPrefix = (dellDeviceType == DellDeviceType.PowerConnect62xx) ? "ch" : "po";
				string portChannelInterfacePrefix = (dellDeviceType != DellDeviceType.GeneralDellDevice) ? "Port Channel " : "";

				await this.Provider.Terminal.ExitConfigModeAsync();
                
                string response = await this.Provider.Terminal.SendAsync("show interface configuration");
                string[][] vlanTable = ProviderHelper.GetTable(response, "----");

                foreach (string[] lineArray in vlanTable)
                {
                    string firstElement = lineArray[0];

                    if (firstElement.Contains("/"))
                    {
                        ethernetInterfaceNames.Add(ethernetInterfacePrefix + firstElement);
                    }
                    else if (firstElement.ToLower().StartsWith(portChanelPrefix) && firstElement.Length > 2)
                    {
                        portChannelInterfaceNames.Add(portChannelInterfacePrefix + firstElement.Substring(2));
                    }
				}

				foreach (var interfaceIndexesByInterfaceNameItem in interfaceIndexesByInterfaceName)
                {
                    string interfaceName = interfaceIndexesByInterfaceNameItem.Key;
                    int interfaceIndex = interfaceIndexesByInterfaceNameItem.Value;

                    if (i < ethernetInterfaceNames.Count && interfaceName.ContainsAny(new string[] { "Unit:", "Slot:", "Port:" }, ignoreCase: true))
                    {
                        interfaceName = ethernetInterfaceNames[i++];
                    }
                    else if (j < portChannelInterfaceNames.Count && interfaceName.TrimStart().ToLower().StartsWith("link aggregate"))
                    {
                        interfaceName = portChannelInterfaceNames[j++];
                    }
					else if (interfaceName.TrimStart().ToLower().StartsWith("vl"))
					{
						interfaceName = "Vlan" + interfaceName.Substring(2);
					}

					newInterfaceIndexesByInterfaceName.Add(interfaceName, interfaceIndex);
                }
				
				if (dellDeviceType == DellDeviceType.PowerConnect62xx) // Add L3 Vlan interfaces
				{
					managementVlanId = await this.GetManagementVlanId();
					await this.Provider.Terminal.ExitConfigModeAsync();

					foreach (VlanInfo vlanInfo in vlans)
					{
						response = await this.Provider.Terminal.SendAsync("show ip interface vlan " + vlanInfo.VlanId);

						if ((response.Trim().Length > 0 && !response.ToLower().Contains(strRoutingIsNotEnabled)) || managementVlanId == vlanInfo.VlanId)
						{
							string vlanName = "Vlan" + vlanInfo.VlanId;

							if (!newInterfaceIndexesByInterfaceName.ContainsKey(vlanName))
								newInterfaceIndexesByInterfaceName.Add(vlanName, ++layer3InterfaceIndex);
						}
					}
				}

                this.SetInterfaceIndexesByInterfaceName(newInterfaceIndexesByInterfaceName);
            }
        }

		#endregion |   Private Methods   |

		#region |   Private Methods   |

		private async ValueTask<int> GetManagementVlanId()
		{
			int managementVlanId = -1;
			const string strManagementVlan = "management vlan";

			                  await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show ip interface management");

			if (response.ToLower().Contains(strManagementVlan))
			{
				string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

				foreach (string line in responseArray)
				{
					if (line.ToLower().Contains(strManagementVlan))
					{
						string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
						string managementVlanIdText = lineArray[1];

						managementVlanId = Conversion.TryChangeType<int>(managementVlanIdText, -1);

						break;
					}
				}
			}

			return managementVlanId;
		}

		private async ValueTask<NetworkInfo> GetManagementIpAddressInfo()
		{
			string ipAddress = null, subnetMask = null;
			const string strIpAddress = "ip address";
			const string strSubnetMask = "subnet mask";

			                  await this.Provider.Terminal.ExitConfigModeAsync();
			string response = await this.Provider.Terminal.SendAsync("show ip interface management");
			string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			foreach (string line in responseArray)
			{
				if (line.ToLower().TrimStart().StartsWith(strIpAddress))
				{
					string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
					
                    ipAddress = lineArray[1];
				}
				else if (line.ToLower().TrimStart().StartsWith(strSubnetMask))
				{
					string[] lineArray = line.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
					
                    subnetMask = lineArray[1];

					break;
				}
			}

			return new NetworkInfo(ipAddress, subnetMask);
		}

		private async ValueTask<bool> IsActiveSwitchPort(string interfaceName)
        {
            bool result = false;
            var switchPortShortInterfaceNames = await this.GetSwitchPortShortInterfaceNames();

            foreach (string shortInterfaceName in switchPortShortInterfaceNames)
            {
                string interfaceSufix = shortInterfaceName.Substring(2);

                if (interfaceName.EndsWith(interfaceSufix))
                {
                    result = true;
                    
                    break;
                }
            }

            return result;
        }

        #endregion |   Private Methods   |
    }
}
