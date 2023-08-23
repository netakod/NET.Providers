using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXEL)]
    public class NetworkDeviceProviderInterfacesZyXEL : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private const int startVlanInterfaceIndex = 10000;
        private List<string> trunkInterfaceNames = null;
        private IEnumerable<VlanInfo> vlans = null;

		#endregion |   Private Members   |

		#region |   Private Properties   |

		private async ValueTask<IEnumerable<VlanInfo>> GetVlans()
		{
			if (this.vlans == null)
				this.vlans = await this.Provider.Vlans.GetVlans();

			return this.vlans;
		}

		#endregion |   Private Properties   |

		#region |   Interface   |

		//public override async ValueTask<IEnumerable<ProviderInterfaceInfo>> GetInterfaces()
  //      {
  //          List<ProviderInterfaceInfo> interfaces = new List<ProviderInterfaceInfo>(await base.GetInterfaces());
  //          //IEnumerable<ProviderVlanInfo> vlans = this.Provider.Vlans.GetVlans();

  //          //foreach (ProviderVlanInfo vlanInfo in vlans)
  //          //{
  //          //    interfaces.Add(new ProviderInterfaceInfo("Vlan" + vlanInfo.VlanId));
  //          //}

  //          return interfaces;
  //      }

        public override async ValueTask<string> GetName(int interfaceIndex)
        {
            string interfaceName = String.Empty;

            if (interfaceIndex > startVlanInterfaceIndex)
            {
                interfaceName = "Vlan" + (interfaceIndex - startVlanInterfaceIndex);
            }
            else
            {
                //interfaceName = base.GetName(interfaceIndex);

                var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();
                var interfaceItem = interfaceIndexesByInterfaceName.FirstOrDefault(item => item.Value == interfaceIndex);

                //if (!interfaceItem.Equals(default(KeyValuePair<string, int>)))
                    interfaceName = interfaceItem.Key;
            }

            return interfaceName;
        }

        public override async ValueTask<int> GetIndex(string interfaceName)
        {
            int interfaceIndex = -1;
            InterfaceOperationalType interfaceType = this.GetZyXelInterfaceType(interfaceName);

            if (interfaceType == InterfaceOperationalType.Vlan)
            {
                interfaceIndex = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName) + startVlanInterfaceIndex;
            }
            else
            {
                interfaceIndex = await base.GetIndex(interfaceName);
            }

            return interfaceIndex;
        }

        public override async ValueTask<string> GetDescription(string interfaceName)
        {
            string description = String.Empty;
            InterfaceOperationalType interfaceType = this.GetZyXelInterfaceType(interfaceName);

            if (interfaceType == InterfaceOperationalType.Vlan)
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);
                
                description = await this.Provider.Vlans.GetName(vlanId);
            }
            else
            {
                description = await base.GetDescription(interfaceName);
            }

            return description;
        }

        public override async ValueTask SetDescription(string interfaceName, string description)
        {
            string zyxelDescription = description.IsNullOrEmpty() ? " " : description.Trim().Replace(' ', '_');
            InterfaceOperationalType interfaceType = this.GetZyXelInterfaceType(interfaceName);

            if (interfaceType == InterfaceOperationalType.Vlan)
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);
                await this.Provider.Vlans.SetName(vlanId, zyxelDescription);
            }
            else if (interfaceType == InterfaceOperationalType.PhysicalPort)
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface port-channel " + interfaceName);
                await this.Provider.Terminal.SendAsync("name " + zyxelDescription);
                await this.Provider.Terminal.SendAsync("exit");
            }
            else
            {
                await base.SetDescription(interfaceName, zyxelDescription);
            }
        }

        public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
        {
            InterfaceOperationalType interfaceType = this.GetZyXelInterfaceType(interfaceName);

            if (interfaceType == InterfaceOperationalType.PhysicalPort)
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface port-channel " + interfaceName);

                string adminStatusCommand = "inactive";

                if (adminStatus == InterfaceAdminStatus.Up)
                    adminStatusCommand = "no " + adminStatusCommand;

                await this.Provider.Terminal.SendAsync(adminStatusCommand);
                await this.Provider.Terminal.SendAsync("exit");
            }
            else
            {
                await base.SetAdminStatus(interfaceName, adminStatus);
            }
        }

        #endregion |   Interface   |

        #region |   Add Remove Interface   |

        public override bool IsAddRemoveSupported() => true;


        public override async ValueTask Add(string interfaceName)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            if (vlanId != 0)
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                await this.Provider.Terminal.SendAsync("exit");

                await this.GenerateInterfaceDictionary();
            }

            if (this.trunkInterfaceNames != null && (await this.GetSwitchportInfo(interfaceName)).SwitchportMode == InterfaceSwitchportMode.Trunk)
                this.trunkInterfaceNames.Add(interfaceName);
        }

        public override async ValueTask Remove(string interfaceName)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            if (vlanId != 0)
            {
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("no vlan " + vlanId);
                await this.Provider.Terminal.SendAsync("exit");

                await this.GenerateInterfaceDictionary();
            }

            if (this.trunkInterfaceNames != null && this.trunkInterfaceNames.Contains(interfaceName))
                this.trunkInterfaceNames.Remove(interfaceName);
        }

        #endregion |   Add Remove Interface   |

        #region |   Interface IP Addresses   |

        public override ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            bool result = this.GetZyXelInterfaceType(interfaceName) == InterfaceOperationalType.Vlan;
            
            return new ValueTask<bool>(result);
        }

        public override async ValueTask<bool> IsWriteIpAddressSupported(string interfaceName)
        {
            return await this.IsIpAddressSupported(interfaceName);
        }

        //public override IPAddressInfo GetIPAddress(string interfaceName)
        //{
        //    List<IPAddressInfo> ipAddresses = base.GetIPAddresses(interfaceName);
        //    IPAddressInfo ipAddress = ipAddresses.Count > 0 ? ipAddresses[0] : new IPAddressInfo("", "");

        //    return ipAddress;
        //}

        public override async ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            if (this.GetZyXelInterfaceType(interfaceName) == InterfaceOperationalType.Vlan)
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

                if (ipAddress != null)
                {
                    await this.Provider.Terminal.EnterConfigModeAsync();
                    await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                    await this.Provider.Terminal.SendAsync("ip address " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix));
                    await this.Provider.Terminal.SendAsync("exit");
                }
                else    // no ip address to set - remove existing
                {
                    NetworkInfo oldIpAddressInfo = await this.GetIpAddress(interfaceName);

                    if (oldIpAddressInfo.IpAddressText.Trim() != "" && oldIpAddressInfo.SubnetMask.Trim() != "")
                    {
                        await this.Provider.Terminal.EnterConfigModeAsync();
                        await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                        await this.Provider.Terminal.SendAsync("no ip address " + oldIpAddressInfo.IpAddressText + " " + oldIpAddressInfo.SubnetMask);
                        await this.Provider.Terminal.SendAsync("exit");
                    }
                }
            }
        }

        public override async ValueTask<List<NetworkInfo>> GetIpAddresses(string interfaceName)
        {
			string[] strIpAddressList = new string[] { "IP Address", "Management IP" };
            List<NetworkInfo> ipAddressInfoList = new List<NetworkInfo>();

            if (this.GetZyXelInterfaceType(interfaceName) == InterfaceOperationalType.Vlan)
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

                                  await this.Provider.Terminal.ExitConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync("show vlan " + vlanId);

                if (response.ContainsAny(strIpAddressList, ignoreCase: true))
                {
                    string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    foreach (string line in responseArray)
                    {
                        if (line.ContainsAny(strIpAddressList, ignoreCase: true))
                        {
                            string[] lineArray = line.Split(new string[] { " ", ":", "\t",  }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < lineArray.Count() - 1; i++)
                            {
                                if (IpHelper.ValidateIpAddress(lineArray[i]) && IpHelper.ValidateIpAddress(lineArray[i + 1]))
                                {
                                    string ipAddress = lineArray[i];
                                    string ipSubnetMask = lineArray[i + 1];
                                    NetworkInfo ipAddressInfo = new NetworkInfo(ipAddress, ipSubnetMask);
                                    
                                    ipAddressInfoList.Add(ipAddressInfo);
                                }
                            }
                        }
                    }
                }
            }

            return ipAddressInfoList;
        }

        public override ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName)
        {
            bool result = this.GetZyXelInterfaceType(interfaceName) == InterfaceOperationalType.Vlan;

            return new ValueTask<bool>(result);
        }

        //public override IEnumerable<IPAddressInfo> GetSecondaryIPAddresses(string interfaceName)
        //{
        //    List<IPAddressInfo> ipAddresses = this.GetIPAddresses(interfaceName);

        //    if (ipAddresses.Count > 0)
        //    {
        //        ipAddresses.RemoveAt(0);    // Remove primary ip address
        //    }

        //    return ipAddresses;
        //}

        public override async ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
        {
            return await this.IsSecondaryIpAddressSupported(interfaceName);
        }

        public override async ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            await this.SetIpAddress(interfaceName, ipAddress, subnetMaskPrefix);
            await this.GenerateIpAddressDictionary();
        }

        public override async ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("no ip address " + ipAddress.ToString() + " " + IpHelper.GetSubnetMask(subnetMaskPrefix));
            await this.Provider.Terminal.SendAsync("exit");

            await this.GenerateIpAddressDictionary();
        }

        #endregion |   Interface IP Addresses   |

        #region |   Interface Vlans   |

        public async ValueTask<IList<string>> GetTrunkPortInterfaceNames()
        {
            if (this.trunkInterfaceNames == null)
            {

                this.trunkInterfaceNames = new List<string>();
                IEnumerable<string> interfaceNames = await this.GetInterfaceNames();

                await this.Provider.Terminal.ExitConfigModeAsync();

                foreach (string interfaceName in interfaceNames)
                {
                    SwitchportInfo switchportInfo = await this.GetSwitchportInfo(interfaceName);
                    InterfaceSwitchportMode switchportMode = switchportInfo.SwitchportMode;

                    if (switchportMode == InterfaceSwitchportMode.Trunk)
                        this.trunkInterfaceNames.Add(interfaceName);
                }
            }

            return this.trunkInterfaceNames.AsReadOnly();
        }

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			string strVlanTrunk = "vlan-trunk";
			string strInterfacePortChannel = "interface port-channel";
			string strExit = "exit";
			int vlanId = 1;
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			InterfaceOperationalType interfaceOperationalTypeZyXel = this.GetZyXelInterfaceType(interfaceName);

			if (interfaceName != null && interfaceOperationalTypeZyXel == InterfaceOperationalType.PhysicalPort)
			{
				switchportMode = InterfaceSwitchportMode.Access;
				int portNumber = this.GetPortNumberFromPortInterfaceName(interfaceName);

				                  await this.Provider.Terminal.ExitConfigModeAsync();
				string response = await this.Provider.Terminal.SendAsync("show running-config interface port-channel " + portNumber);
				int interfacePortChannelPosition = response.IndexOf(strInterfacePortChannel + " " + portNumber);

				if (interfacePortChannelPosition > 0)
				{
					string[] lines = response.Substring(interfacePortChannelPosition).Split(new string[] { "\r\n" }, StringSplitOptions.None);

					for (int i = 1; i < lines.Length - 1; i++)
					{
						string line = lines[i];

						if (line.Contains(strInterfacePortChannel) || line.Contains(strExit))
							break;

						if (line.Contains(strVlanTrunk))
						{
							switchportMode = InterfaceSwitchportMode.Trunk;
							
                            break;
						}
					}
				}
			}


			if (switchportMode == InterfaceSwitchportMode.Access)
				vlanId = await this.GetVlanId(interfaceName);

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
  //      {
  //          string strVlanTrunk = "vlan-trunk";
  //          string strInterfacePortChannel = "interface port-channel";
  //          string strExit = "exit";
  //          InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
  //          InterfaceOperationalType interfaceOperationalTypeZyXel = this.GetZyXelInterfaceType(interfaceName);

  //          if (interfaceName != null && interfaceOperationalTypeZyXel == InterfaceOperationalType.PhisicalPort)
  //          {
  //              switchportMode = InterfaceSwitchportMode.Access;
  //              int portNumber = this.GetPortNumberFromPortInterfaceName(interfaceName);

  //              this.Provider.Terminal.ExitConfigMode();
  //              string response = this.Provider.Terminal.Send("show running-config interface port-channel " + portNumber);

  //              int interfacePortChannelPosition = response.IndexOf(strInterfacePortChannel + " " + portNumber);

  //              if (interfacePortChannelPosition > 0)
  //              {
  //                  string[] lines = response.Substring(interfacePortChannelPosition).Split(new string[] { "\r\n" }, StringSplitOptions.None);

  //                  for (int i = 1; i < lines.Length - 1; i++)
  //                  {
  //                      string line = lines[i];

  //                      if (line.Contains(strInterfacePortChannel) || line.Contains(strExit))
  //                          break;

  //                      if (line.Contains(strVlanTrunk))
  //                      {
  //                          switchportMode = InterfaceSwitchportMode.Trunk;
  //                          break;
  //                      }
  //                  }
  //              }
  //          }

  //          return switchportMode;
  //      }

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
                int portNumber = this.GetPortNumberFromPortInterfaceName(interfaceName);

                await this.Provider.Terminal.EnterConfigModeAsync();

                switch (switchportMode)
                {
                    case InterfaceSwitchportMode.Access:

						await this.Provider.Terminal.SendAsync("interface port-channel " + portNumber);
                        await this.Provider.Terminal.SendAsync("no vlan-trunking");
						await this.Provider.Terminal.SendAsync("pvid " + vlanId);
						await this.Provider.Terminal.SendAsync("exit");
						await this.Provider.Terminal.SendAsync("vlan " + vlanId);
						await this.Provider.Terminal.SendAsync("untagged " + portNumber);
						await this.Provider.Terminal.SendAsync("fixed " + portNumber);
						await this.Provider.Terminal.SendAsync("exit");

						break;

                    case InterfaceSwitchportMode.Trunk:

						await this.Provider.Terminal.SendAsync("interface port-channel " + portNumber);
                        await this.Provider.Terminal.SendAsync("vlan-trunking");
						await this.Provider.Terminal.SendAsync("pvid " + vlanId);
						await this.Provider.Terminal.SendAsync("exit");

                        // For all vlans set this trunk port as fixed
                        IEnumerable<VlanInfo> vlans = await this.GetVlans();
                        
                        await this.Provider.Terminal.EnterConfigModeAsync();

                        foreach (VlanInfo vlan in vlans)
                        {
                            await this.Provider.Terminal.SendAsync("vlan " + vlan.VlanId);
                            await this.Provider.Terminal.SendAsync("fixed " + portNumber);
                            await this.Provider.Terminal.SendAsync("no untagged " + portNumber);
                            await this.Provider.Terminal.SendAsync("exit");
                        }

                        break;
                }

                // Refresh trunk interface names cache
                if (this.trunkInterfaceNames != null)
                {
                    if (switchportMode == InterfaceSwitchportMode.Trunk)
                    {
                        if (!this.trunkInterfaceNames.Contains(interfaceName))
                            this.trunkInterfaceNames.Add(interfaceName);
                    }
                    else
                    {
                        if (this.trunkInterfaceNames.Contains(interfaceName))
                            this.trunkInterfaceNames.Remove(interfaceName);
                    }
                }
            }
        }

        //public override void SetVlanId(string interfaceName, int vlanId)
        //{
        //    int portNumber = this.GetPortNumberFromPortInterfaceName(interfaceName);

        //    this.Provider.Terminal.EnterConfigMode();
        //    string response = this.Provider.Terminal.Send("interface port-channel " + portNumber);
        //    //response = this.Provider.Connection.Terminal.Send("no vlan-trunking");
        //    response = this.Provider.Terminal.Send("pvid " + vlanId);
        //    response = this.Provider.Terminal.Send("exit");
        //    response = this.Provider.Terminal.Send("vlan " + vlanId);
        //    response = this.Provider.Terminal.Send("untagged " + portNumber);
        //    response = this.Provider.Terminal.Send("fixed " + portNumber);
        //    response = this.Provider.Terminal.Send("exit");
        //}

        #endregion |   Interface Vlans   |

        #region |   Interface ACL   |

        // ACL is not supported on ZyXEL

        #endregion |   Interface ACL   |

        #region |   Internal Methods   |

        internal int GetPortNumberFromPortInterfaceName(string interfaceName)
        {
            //int portNumber = -1;

            //try
            //{
                //string newInterfaceName = interfaceName.Replace("swp", "");
               int portNumber = Conversion.TryChangeType<int>(interfaceName, -1);
                //portNumber++; // While interface ifDescr is zero based, and terminal access interface number is not.
            //}
            //catch
            //{
            //}

            return portNumber;
        }

        #endregion |   Internal Methods   |

        #region |   Protected Methods   |

        protected override async ValueTask GenerateInterfaceDictionary()
        {
            string snmpPortPrefix = "swp";
            Dictionary<string, int> newInterfaceIndexesByInterfaceName = new Dictionary<string, int>();
            
            await base.GenerateInterfaceDictionary();

            var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

            foreach (var item in interfaceIndexesByInterfaceName)
            {
                string interfaceName = item.Key;
                int interfaceIndex = item.Value;
                string newInterfaceName = interfaceName;

                if (interfaceName.StartsWith(snmpPortPrefix))
                {
                    int interfacePortNumber = Conversion.TryChangeType<int>(interfaceName.Substring(snmpPortPrefix.Length)) + 1;
                    
                    newInterfaceName = interfacePortNumber.ToString();
                }

                newInterfaceIndexesByInterfaceName.Add(newInterfaceName, interfaceIndex);
            }

            this.SetInterfaceIndexesByInterfaceName(newInterfaceIndexesByInterfaceName);
        }

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{
			const string strAccessModeVlan = "PVID";
			int vlanId = 1;
			int portNumber = this.GetPortNumberFromPortInterfaceName(interfaceName);

			                  await this.Provider.Terminal.ExitConfigModeAsync();
			string response = await this.Provider.Terminal.SendAsync("show interfaces config " + portNumber.ToString());

			foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
			{
				if (line.Trim().StartsWith(strAccessModeVlan))
				{
					string[] lineArray = line.Split(new string[] { ":", "\t" }, StringSplitOptions.RemoveEmptyEntries);
					string strVlanId = lineArray[1];
					
                    vlanId = Conversion.TryChangeType<int>(strVlanId);
				}
			}

			return vlanId;
		}

		#endregion |   Protected Methods   |

		#region |   Private Methods   |

		private InterfaceOperationalType GetZyXelInterfaceType(string interfaceName)
        {
            InterfaceOperationalType result = InterfaceOperationalType.System;

            if (interfaceName.StartsWith("vlan", true, null))
            {
                result = InterfaceOperationalType.Vlan;
            }
            else if (Conversion.TryChangeType<int>(interfaceName) > 0)
            {
                result = InterfaceOperationalType.PhysicalPort;
            }

            return result;
        }

        #endregion |   Private Methods   |
    }
}
