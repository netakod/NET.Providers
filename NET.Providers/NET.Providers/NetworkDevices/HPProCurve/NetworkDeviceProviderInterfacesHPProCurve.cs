using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Snmp;

namespace NET.Tools.Providers 
{
	[NetworkDeviceProviderType(DeviceProviderType.HPProCurve)]
    public class NetworkDeviceProviderInterfacesHPProCurve : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

		//private Dictionary<int, string> vlanInterfaceNamesByVlanId = null;
        private List<string> trunkPortInterfaceNames = null;
		private Dictionary<int, int> ifTypesByInterfaceIndex = new Dictionary<int, int>();
		private Dictionary<string, int> vlanIdsByVlanName = new Dictionary<string, int>();
		private const string strDefaultVlan = "DEFAULT_VLAN";
        //private IEnumerable<int> vlanIds = null;

        #endregion |   Private Members   |

		#region |   Private Properties   |

		//private Dictionary<int, string> VlanInterfaceNamesByVlanId
		//{
		//	get
		//	{
		//		if (this.vlanInterfaceNamesByVlanId == null)
		//		{
		//			this.vlanInterfaceNamesByVlanId = new Dictionary<int, string>();
		//			IEnumerable<ProviderVlanInfo> vlanInfos = this.Provider.Vlans.GetVlans();

		//			foreach (ProviderVlanInfo vlanInfo in vlanInfos)
		//			{
		//				this.vlanInterfaceNamesByVlanId.Add(vlanInfo.VlanId, vlanInfo.VlanName);
		//			}
		//		}

		//		return this.vlanInterfaceNamesByVlanId;
		//	}
		//}

        private async ValueTask<List<string>> GetTrunkPortInterfaceNames()
        {
            if (this.trunkPortInterfaceNames == null)
                this.trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNamesInternal();

            return this.trunkPortInterfaceNames;
        }

		//private IEnumerable<int> VlanIds
		//{
		//    get
		//    {
		//        if (this.vlanIds == null)
		//        {
		//            List<int> result = new List<int>();
		//            ProviderKeys vlanProviderKeys = this.Provider.Vlans.GetKeys();

		//            foreach (ProviderKey vlanProviderKey in vlanProviderKeys.Keys)
		//            {
		//                result.Add((int)vlanProviderKey.KeyValues[0]);
		//            }

		//            this.vlanIds = result;
		//        }

		//        return this.vlanIds;
		//    }
		//}

		#endregion |   Private Properties   |

		#region |   Interface   |

		public override async ValueTask SetDescription(string interfaceName, string description)
        {
			//if (vlanId > 0)
			//{
			//	this.Provider.DeviceConnection.Terminal.EnterConfigMode();
			//	this.Provider.DeviceConnection.Terminal.Send("vlan " + vlanId);
			//	this.Provider.DeviceConnection.Terminal.Send("name Vlan" + vlanId);
			//	this.Provider.DeviceConnection.Terminal.Send("exit");
			//}
			//else
			//{
            try
            {
                await base.SetDescription(interfaceName, description);
            }
            catch
            {
				string hpDescription = description.IsNullOrEmpty() ? " " : description.Trim().Replace(' ', '_');
				int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);
					
				await this.Provider.Terminal.EnterConfigModeAsync();

				if (vlanId > 0)
					await this.Provider.Terminal.SendAsync("vlan " + vlanId);
				else
					await this.Provider.Terminal.SendAsync("interface " + interfaceName);
                    
				await this.Provider.Terminal.SendAsync("name " + hpDescription);
                await this.Provider.Terminal.SendAsync("exit");
            }
			//}
        }

        #endregion |   Interface   |

        #region |   Add Remove Interface   |

        public override bool IsAddRemoveSupported() => true;

        public override async ValueTask Add(string interfaceName)
        {
            if (interfaceName.ToLower().Contains("vlan"))
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                await this.Provider.Terminal.SendAsync("name " + interfaceName);
                await this.Provider.Terminal.SendAsync("exit");
    
                await this.GenerateInterfaceDictionary();
            }
        }

        public override async ValueTask Remove(string interfaceName)
        {
            if (interfaceName.ToLower().Contains("vlan"))
            {
                int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);
                
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("no vlan " + vlanId);
                await this.Provider.Terminal.SendAsync("exit");
                
                await this.GenerateInterfaceDictionary();
            }
        }

        #endregion |   Add Remove Interface   |

        #region |   Interface IP Addresses   |

        public override async ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            InterfaceOperationalType interfaceOperationalType = await this.GetInterfaceOperationalType(interfaceName);
            bool result = interfaceOperationalType == InterfaceOperationalType.PhysicalPort ? false : true;
            
            return result;
        }

        public override async ValueTask<bool> IsWriteIpAddressSupported(string interfaceName)
        {
            InterfaceOperationalType interfaceOperationalType = await this.GetInterfaceOperationalType(interfaceName);
            bool result = interfaceOperationalType == InterfaceOperationalType.Vlan;
            
            return result;
        }

		//public override IPAddressInfo GetIpAddress(string interfaceName)
		//{
		//	const string strVlan = "VLAN";
		//	const string strLoopbackInterface = "Loopback Interface";
		//	bool isThisRequiredSection = false;
		//	bool isThisFirstLineAfterRequiredSection = true;

		//	IPAddressInfo result = new IPAddressInfo("", "");

		//	this.Provider.DeviceConnection.Terminal.ExitConfigMode();

		//	string response = this.Provider.DeviceConnection.Terminal.Send("show ip");
		//	string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

		//	foreach (string line in responseArray)
		//	{
		//		if (!isThisRequiredSection && line.Trim().StartsWith(strVlan))
		//		{
		//			isThisRequiredSection = true;
		//			continue;
		//		}

		//		if (line.Trim() == strLoopbackInterface)
		//			break;

		//		if (isThisRequiredSection)
		//		{
		//			if (isThisFirstLineAfterRequiredSection)
		//			{
		//				isThisFirstLineAfterRequiredSection = false;
		//				continue;
		//			}

		//			string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
		//			if (lineArray.Length > 1)
		//			{
		//				if (interfaceName == lineArray[0].Trim())
		//				{
		//					try
		//					{
		//						string ipAddress = lineArray[3].Trim();
		//						string ipSubnetMask = lineArray[4].Trim();

		//						result = new IPAddressInfo(ipAddress, ipSubnetMask);
		//					}
		//					catch
		//					{
		//					}
		//				}
		//			}
		//		}
		//	}

		//	return result;
		//}

        public override async ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            if (vlanId > 0)
            {
                NetworkInfo oldIpAddressInfo = await this.GetIpAddress(interfaceName);
                string subnetMask = IpHelper.GetSubnetMask(subnetMaskPrefix);

				if (oldIpAddressInfo.IpAddressText.Trim() == ipAddress.ToString().Trim() && oldIpAddressInfo.SubnetMask.Trim() == subnetMask)
					return;

				if (vlanId > 1 && (oldIpAddressInfo.IpAddressText.Trim() != "" || oldIpAddressInfo.SubnetMask.Trim() != ""))
                {
                    await this.Provider.Terminal.EnterConfigModeAsync();
                    await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                    await this.Provider.Terminal.SendAsync("no ip address " + oldIpAddressInfo.IpAddressText + " " + oldIpAddressInfo.SubnetMask);
                    await this.Provider.Terminal.SendAsync("exit");
                }
                
                if (ipAddress.ToString() != "" && subnetMask != "")
                {
                    await this.Provider.Terminal.EnterConfigModeAsync();
                    await this.Provider.Terminal.SendAsync("vlan " + vlanId);
                    await this.Provider.Terminal.SendAsync("ip address " + ipAddress + " " + subnetMaskPrefix);
                    await this.Provider.Terminal.SendAsync("exit");
                }
            }
        }

        //[ProviderAction("Interfaces.GetSecondaryIpAddresses")]
        //public List<IPAddressInfo> GetSecondaryIPAddresses(string interfaceName)
        //{
        //    return new List<IPAddressInfo>();
        //}

        //[ProviderAction("Interfaces.AddSecondaryIpAddress")]
        //public void AddSecondaryIPAddress(string interfaceName, string ipAddress, string ipSubnetMask)
        //{
        //}

        //[ProviderAction("Interfaces.RemoveSecondaryIpAddress")]
        //public void RemoveSecondaryIPAddress(string interfaceName, string ipAddress, string ipSubnetMask)
        //{
        //}

        #endregion |   Interface IP Addresses   |

        #region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			const string strTrunk = "Trunk";

			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			InterfaceOperationalType interfaceOperationalType = await this.GetInterfaceOperationalType(interfaceName);
			int vlanId = 1;

			if (interfaceOperationalType == InterfaceOperationalType.PhysicalPort)
			{
				HPProCurvePortTrunkInfo portTrunkInfo = await this.GetPortTrankInfo(interfaceName);

				if (portTrunkInfo.TrunkType == strTrunk)
				{
					switchportMode = InterfaceSwitchportMode.Trunk;
				}
				else
				{
					switchportMode = InterfaceSwitchportMode.Access;
					vlanId = await this.GetVlanId(interfaceName);
				}
			}

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
  //      {
  //          const string strTrunk = "Trunk";
            
  //          InterfaceSwitchportMode result = InterfaceSwitchportMode.VlanIsNotSupported;
  //          InterfaceOperationalType interfaceOperationalType = this.GetInterfaceOperationalType(interfaceName);
            
  //          if (interfaceOperationalType == InterfaceOperationalType.PhisicalPort)
  //          {
  //              HPProCurvePortTrunkInfo portTrunkInfo = this.GetPortTrankInfo(interfaceName);

  //              if (portTrunkInfo.TrunkType == strTrunk)
  //              {
  //                  result = InterfaceSwitchportMode.Trunk;
  //              }
  //              else
  //              {
  //                  result = InterfaceSwitchportMode.Access;
  //              }
  //          }
            
  //          return result;
  //      }

        public override async ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportnMode, int vlanId)
        {
            const string strTrunkGroupName = "Trk";

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
                if (switchportnMode != InterfaceSwitchportMode.VlanIsNotSupported && await this.IsInterfacePhisicalPort(interfaceName))
                {
                    //HPProCurvePortTrunkInfo portTrunkInfo = this.GetPortTrankInfo(interfaceName);
                    //bool isTrunk = portTrunkInfo.TrunkType == strTrunk;

                    if (switchportnMode == InterfaceSwitchportMode.Access)
                    {
                        await this.Provider.Terminal.EnterConfigModeAsync();
                        await this.Provider.Terminal.SendAsync("no trunk " + interfaceName);
						await this.Provider.Terminal.SendAsync("vlan " + vlanId + " untagged " + interfaceName);

                        var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

                        if (trunkPortInterfaceNames.Contains(interfaceName))
                            trunkPortInterfaceNames.Remove(interfaceName);
                    }
                    else if (switchportnMode == InterfaceSwitchportMode.Trunk)
                    {
                        // sets this interface as tagged for all vlans
                        IEnumerable<string> trunkGroupNames = await ProviderHelperHPProCurve.GetInterfaceTrunkGroupNames(this.Provider.Terminal);
                        
                        await this.Provider.Terminal.EnterConfigModeAsync();

                        for (int i = 1; i < 1000; i++)
                        {
                            string trunkGroupName = strTrunkGroupName + i.ToString();
                            string response = await this.Provider.Terminal.SendAsync("trunk " + interfaceName + " " + trunkGroupName + " trunk");

                            // If not response "max allowed port int trunk is" ... this is ok trunk group name
                            if (response.Trim().Length == 0)
                            {
                                bool setTrunkGroupThroughTheVlans = true;

                                foreach (string trunkGroupNameItem in trunkGroupNames)
                                {
                                    if (trunkGroupNameItem.ToLower() == trunkGroupName.ToLower())
                                    {
                                        setTrunkGroupThroughTheVlans = false;
                                        break;
                                    }
                                }

                                if (setTrunkGroupThroughTheVlans)
                                {
									IEnumerable<VlanInfo> vlanInfos = await this.Provider.Vlans.GetVlans();
                                    
                                    await this.Provider.Terminal.EnterConfigModeAsync();

									foreach (VlanInfo vlanInfo in vlanInfos)
                                    {
										string tagging = (vlanInfo.VlanId == 1) ? "untagged" : "tagged";
										
                                        await this.Provider.Terminal.SendAsync(String.Format("vlan {0} {1} {2}", vlanInfo.VlanId, tagging, trunkGroupName));
                                    }
                                }

                                break;
                            }
                        }

						await this.Provider.Terminal.EnterConfigModeAsync();
                        await this.Provider.Terminal.SendAsync("vlan " + vlanId + " untagged " + interfaceName);

                        var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();


                        if (!trunkPortInterfaceNames.Contains(interfaceName))
                            trunkPortInterfaceNames.Add(interfaceName);
                    }
                }
            }
        }

        //public override void SetVlanId(string interfaceName, int vlanId)
        //{
        //    if (this.IsInterfacePhisicalPort(interfaceName))
        //    {
        //        this.Provider.Terminal.EnterConfigMode();
        //        string response = this.Provider.Terminal.Send("vlan " + vlanId + " untagged " + interfaceName);
        //    }
        //}

		//public override bool IsLayer3InterfaceBasedOnVlan(string interfaceName)
		//{
		//	return this.GetInterfaceOperationalType(interfaceName) == ProviderInterfaceOperationalType.Vlan; 
		//}

		//public override int GetLayer3InterfaceBasedVlanId(string interfaceName)
		//{
		//	// Možda može return this.GetVlanInterfaceDictionary().FirstOrDefault(...

		//	int result = 0;

		//	Dictionary<int, string> vlanInterfaceDictionary = this.VlanInterfaceNamesByVlanId; //this.GetVLANInterfaceDictionary();

		//	if (vlanInterfaceDictionary.Values.Contains(interfaceName))
		//	{
		//		foreach (int vlanId in vlanInterfaceDictionary.Keys)
		//		{
		//			string ifName = vlanInterfaceDictionary[vlanId];

		//			if (ifName.Trim() == interfaceName.Trim())
		//			{
		//				result = vlanId;
		//				break;
		//			}
		//		}
		//	}

		//	return result;
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

        public override async ValueTask AttachAcl(string interfaceName, string aclName, AclDirection aclDirection)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            if (vlanId == 0)
                return;

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("ip access-list extended " + aclName);
            await this.Provider.Terminal.SendAsync("exit");     // exit access-list

            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("ip access-group " + aclName + " " + aclDirection.ToString().ToLower());
            await this.Provider.Terminal.SendAsync("exit");     // exit vlan
        }

        public override async ValueTask DetachAcl(string interfaceName, string aclName, AclDirection aclDirection)
        {
            int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

            if (vlanId == 0)
                return;
            
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("no ip access-group " + aclName + " " + aclDirection.ToString().ToLower());
            await this.Provider.Terminal.SendAsync("exit");     // exit vlan
            await this.Provider.Terminal.SendAsync("no ip access-list extended " + aclName);
        }

        #endregion |   Interface ACL   |

        #region |   Protected Methods   |

        protected override async ValueTask GenerateInterfaceDictionary()
		{
			IList<SnmpData> ifTypes = await this.Provider.Snmp.WalkAsync(SnmpOIDs.Interfaces.ifType);
			this.ifTypesByInterfaceIndex.Clear();

			foreach (SnmpData snmpData in ifTypes)
			{
				int interfaceIndex = Conversion.TryChangeType<int>(snmpData.OID.Substring(SnmpOIDs.Interfaces.ifType.Length + 1));
				int ifType = Conversion.TryChangeType<int>(snmpData.Value);

				if (!this.ifTypesByInterfaceIndex.ContainsKey(interfaceIndex))
					this.ifTypesByInterfaceIndex.Add(interfaceIndex, ifType);
			}

			IList<SnmpData> dot1qVlanStaticNames = await this.Provider.Snmp.WalkAsync(SnmpOIDs.Vlans.dot1qVlanStaticName);
			this.vlanIdsByVlanName.Clear();

			foreach (SnmpData snmpData in dot1qVlanStaticNames)
			{
				int interfaceIndex = Conversion.TryChangeType<int>(snmpData.OID.Substring(SnmpOIDs.Vlans.dot1qVlanStaticName.Length + 1));
				string vlanName = snmpData.Value;

				if (!this.vlanIdsByVlanName.ContainsKey(vlanName))
					this.vlanIdsByVlanName.Add(vlanName, interfaceIndex);
			}

			await base.GenerateInterfaceDictionary();
		}

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{
			int vlanId = 1;

			if (await this.IsInterfacePhisicalPort(interfaceName))
			{
				                  await this.Provider.Terminal.ExitConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync("show vlan ports " + interfaceName);
				string[][] vlanTable = ProviderHelperHPProCurve.GetTable(response, "----", skipLineAfterHeader: false);

				if (vlanTable.Length > 0)
					vlanId = Conversion.TryChangeType<int>(vlanTable[0][0]);
			}

			return vlanId;
		}

		protected override void OnInterfaceIndexesByInterfaceNameAdd(string interfaceName, int interfaceIndex)
		{
			string newInterfaceName = interfaceName;
			int ifType;

			if (this.ifTypesByInterfaceIndex.TryGetValue(interfaceIndex, out ifType))
			{
				if (ifType == 53)
				{
					int vlanId;

					if (this.vlanIdsByVlanName.TryGetValue(interfaceName, out vlanId)) // If interface name is vlan name -> this is vlan interface.
						newInterfaceName = String.Format("Vlan{0}", vlanId);
					else if (interfaceName == "DEFAULT_VLAN")
						newInterfaceName = "Vlan1";
				}
			}

			//string newInterfaceName = (interfaceName == "DEFAULT_VLAN") ? "Vlan1" : interfaceName;
			base.OnInterfaceIndexesByInterfaceNameAdd(newInterfaceName, interfaceIndex);
		}
		
        protected override string GetAclProtocolString(byte protocol)
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
            }

            return result;
        }

        protected override string GetAclIpAddressString(string ipAddress, byte ipSubnetMaskNumOfBits)
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


        #endregion |   Private Methods   |

        #region |   Private Methods   |

        private async ValueTask<HPProCurvePortTrunkInfo> GetPortTrankInfo(string interfaceName)
        {
            const string strPort = "Port";
            const string strTrunk = "Trunk";

            HPProCurvePortTrunkInfo result = new HPProCurvePortTrunkInfo(String.Empty, String.Empty);
            InterfaceOperationalType interfaceOperationalType = await this.GetInterfaceOperationalType(interfaceName);

            if (interfaceOperationalType == InterfaceOperationalType.PhysicalPort)
            {
                                  await this.Provider.Terminal.ExitConfigModeAsync();
                string response = await this.Provider.Terminal.SendAsync("show trunks " + interfaceName);
                string[][] trunkTable = ProviderHelper.GetTable(response, strPort);

                foreach (string[] lineArray in trunkTable)
                {
                    string trunkPort = lineArray.First();
                    string trunkType = lineArray.Last();
                    string trunkGroup = lineArray.ElementAt(lineArray.Length - 2);

                    if (trunkType == strTrunk && trunkPort == interfaceName)
                    {
                        result = new HPProCurvePortTrunkInfo(trunkType, trunkGroup);
                        
                        break;
                    }
                }
            }

            return result;
        }

        private async ValueTask<List<string>> GetTrunkPortInterfaceNamesInternal()
        {
            const string strTrunk = "Trunk";
            List<string> result = new List<string>();
            
            await this.Provider.Terminal.ExitConfigModeAsync();
            
            string response = await this.Provider.Terminal.SendAsync("show trunks");
            string[][] trunkTable = ProviderHelperHPProCurve.GetTable(response, "----", skipLineAfterHeader: false);

            foreach (string[] lineArray in trunkTable)
            {
                string trunkPort = lineArray.First();
                string trunkType = lineArray.Last();
                string trunkGroup = lineArray.ElementAt(lineArray.Length - 2);

                if (trunkType == strTrunk && !result.Contains(trunkPort))
                    result.Add(trunkPort);
            }

            return result;
        }

        //private int GetVlanIdFromVlanInterfaceName(string interfaceName)
        //{
        //    const string strVlan = "vlan";
        //    int vlanId = 0;
        //    interfaceName = interfaceName.Trim();

        //    // If interface already exists
        //    Dictionary<int, string> vlanInterfaceDictionary = this.VlanInterfaceNamesByVlanId; //this.GetVLANInterfaceDictionary();
            
        //    if (vlanInterfaceDictionary.Values.Contains(interfaceName))
        //    {
        //        foreach (int vlanIdKey in vlanInterfaceDictionary.Keys)
        //        {
        //            if (interfaceName == vlanInterfaceDictionary[vlanIdKey])
        //            {
        //                vlanId = vlanIdKey;
        //                break;
        //            }
        //        }
        //    }
        //    else if (interfaceName.ToLower().StartsWith(strVlan))  // If this is new vlan interface to be added
        //    {
        //        string strVlanId = interfaceName.ToLower().Replace(strVlan, "").Trim();

        //        try
        //        {
        //            vlanId = Conversion.TryChangeType<int>(strVlanId);
        //        }
        //        catch
        //        {
        //        }
        //    }

        //    return vlanId;
        //}

        private async ValueTask<InterfaceOperationalType> GetInterfaceOperationalType(string interfaceName)
        {
            InterfaceOperationalType result = InterfaceOperationalType.System;

			if (await this.IsL3InterfaceBasedOnVlan(interfaceName))
            {
                result = InterfaceOperationalType.Vlan;
            }
            else if (await this.IsInterfacePhisicalPort(interfaceName))
			{
				result = InterfaceOperationalType.PhysicalPort;
            }

            return result;
        }

		//private IEnumerable<int> GetVlanIds()
		//{
		//	List<int> result = new List<int>();

		//	this.Provider.DeviceConnection.Terminal.ExitConfigMode();
		//	string response = this.Provider.DeviceConnection.Terminal.Send("show vlans");

		//	string[][] vlanTable = ProviderHelperHPProCurve.GetTable(response, "----", skipLineAfterHeader: false);

		//	foreach (string[] lineArray in vlanTable)
		//	{
		//		int vlanId = Conversion.TryChangeType<int>(lineArray.First());
		//		result.Add(vlanId);
		//	}

		//	return result;
		//}

    //TODO: Dohvaćati vlan names ids iz konfe a ne iz show vlans!!!


		//private Dictionary<int, string> GetVLANInterfaceDictionary()
		//{
		//	const string strVlanId = "VLAN ID";
		//	bool isThisRequiredSection = false;

		//	Dictionary<int, string> vlanInterfaceDictionary = new Dictionary<int, string>();

		//	this.Provider.DeviceConnection.Terminal.ExitConfigMode();
		//	string response = this.Provider.DeviceConnection.Terminal.Send("show vlans");

		//	string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

		//	foreach (string line in responseArray)
		//	{
		//		if (!isThisRequiredSection && line.Trim().StartsWith(strVlanId))
		//		{
		//			isThisRequiredSection = true;
		//			continue;
		//		}

		//		if (isThisRequiredSection)
		//		{
		//			string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
		//			if (lineArray.Length > 1)
		//			{
		//				try
		//				{
		//					int vlanId = Int32.Parse(lineArray[0]);
		//					string vlanName = lineArray[1].Trim();
		//					vlanInterfaceDictionary.Add(vlanId, vlanName);
		//				}
		//				catch
		//				{
		//				}
		//			}
		//		}
		//	}

		//	return vlanInterfaceDictionary;
		//}

        private async ValueTask<bool> IsInterfacePhisicalPort(string interfaceName)
        {
            const string strInvalidInput = "Invalid input";
            bool result = true;

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show interfaces " + interfaceName);

            if (response.Trim().Contains(strInvalidInput))
                result = false;

            return result;
        }


        #endregion |   Private Methods   |
    }

	#region |   Helper Classes   |

	public struct HPProCurvePortTrunkInfo
    {
        public HPProCurvePortTrunkInfo(string trunkType, string trunkGroup)
        {
            this.TrunkType = trunkType;
            this.TrunkGroup = trunkGroup;
        }
        public string TrunkType { get; private set; }
        public string TrunkGroup { get; private set; }
    }

	#endregion |   Helper Classes   |
}
