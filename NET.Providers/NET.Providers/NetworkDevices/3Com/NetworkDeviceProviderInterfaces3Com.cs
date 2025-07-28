using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.C3Com)]
    public class NetworkDeviceProviderInterfaces3Com : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Interface Data   |

        public override async ValueTask SetDescription(string interfaceName, string description)
        {
            //try
            //{
            //    await base.SetDescription(interfaceName, description);
            //}
            //catch
            //{
                string newDescription = description.IsNullOrEmpty() ? " " : description.Trim().Replace(' ', '_');
                
                await this.Provider.Terminal.EnterConfigModeAsync();
                await this.Provider.Terminal.SendAsync("interface " + interfaceName);
                await this.Provider.Terminal.SendAsync("description " + newDescription);
                await this.Provider.Terminal.SendAsync("exit");
            //}
        }

		#endregion |   Interface Data   |

		#region |   Add Remove Interface   |

		public override bool IsAddRemoveSupported() => true;
        
        public override async ValueTask Add(string interfaceName)
        {
            //this.Terminal.EnterConfigurationMode();
            //string response = this.Terminal.SendSync("interface " + interfaceName);
            //response = this.Terminal.SendSync("no shutdown");
            //response = this.Terminal.SendSync("exit");
            await this.GenerateInterfaceDictionary();
        }

        public override async ValueTask Remove(string interfaceName)
        {
            //this.Terminal.EnterConfigurationMode();
            //string response = this.Terminal.SendSync("interface " + interfaceName);
            //response = this.Terminal.SendSync("shutdown");
            //response = this.Terminal.SendSync("exit");
            //response = this.Terminal.SendSync("no interface " + interfaceName);
            await this.GenerateInterfaceDictionary();
        }

        #endregion |   Add Remove Interface   |

        #region |   Interface IP Addresses   |

        public override ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            //bool result = false;

            //if (interfaceName == null || interfaceName.Trim() == "")
            //{
            //    return result;
            //}

            //this.Terminal.ExitConfigurationMode();
            //string response = this.Terminal.SendSync("show interface " + interfaceName + " switchport");

            //if (!response.Contains("invalid"))
            //{
            //    result = !this.IsVlanSupported(interfaceName);
            //}

            //return result;

            return new ValueTask<bool>(false);
        }

        //[ProviderAction("IPAddresses.GetKeys")]
        //public ProviderKeys GetIpAddressKeysByTerminal(string interfaceName)
        //{
        //    ProviderKeys keys = new ProviderKeys(ProviderKeyHelper.KeyNamesIPAddress);

        //    IPAddressInfo primaryIpAddress = this.GetPrimaryIpAddress(interfaceName);

        //    if (primaryIpAddress != null)
        //    {
        //        keys.AddKeyValue(ProviderKeyHelper.CreateIPAddressKey(primaryIpAddress.IPaddress, primaryIpAddress.IPSubnetMask));

        //        IPAddressInfo[] secondaryIpAddresses = this.GetSecondaryIpAddresses(interfaceName);

        //        foreach (IPAddressInfo secondaryIpAddressInfo in secondaryIpAddresses)
        //        {
        //            keys.AddKeyValue(ProviderKeyHelper.CreateIPAddressKey(secondaryIpAddressInfo.IPaddress, secondaryIpAddressInfo.IPSubnetMask));
        //        }
        //    }

        //    return keys;
        //}

        //public override bool IsAddRemoveSecondaryIpAddressesSupported(string interfaceName)
        //{
        //    return true;
        //    //return this.IsIpAddressSupported(interfaceName);
        //}

        public override ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
			//string lineCmd = "ip address " + ipAddress + " " + ipSubnetMask;

			//if (this.GetPrimaryIpAddress(interfaceName) != null)
			//{
			//    lineCmd += " secondary";
			//}

			//this.Terminal.EnterConfigurationMode();

			//string response = this.Terminal.SendSync("interface " + interfaceName);
			//response = this.Terminal.SendSync(lineCmd);
			//response = this.Terminal.SendSync("exit");

			return new ValueTask();
        }

        //[ProviderAction("IPAddresses.GenerateIPAddressDictionary")]
        //public void GenerateIPAddressDictionary()
        //{
        //    // This require only SNMP implementation while telnet get instant data from device and no need for cacheing.
        //}

        //private bool IsPrimaryIpAddress(string interfaceName, string ipAddress, string ipSubnetMask)
        //{
        //    bool result = false;

        //    IPAddressInfo primaryIpAddress = this.GetPrimaryIpAddress(interfaceName);
        //    if (primaryIpAddress != null)
        //    {
        //        if (primaryIpAddress.IPaddress.Trim() == ipAddress.Trim() && primaryIpAddress.IPSubnetMask.Trim() == ipSubnetMask.Trim())
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
        //        if (secondaryIpAddress.IPaddress.Trim() == ipAddress.Trim() && secondaryIpAddress.IPSubnetMask.Trim() == ipSubnetMask.Trim())
        //        {
        //            result = true;
        //            break;
        //        }
        //    }

        //    return result;
        //}

        //private IPAddressInfo GetPrimaryIpAddress(string interfaceName)
        //{
        //    IPAddressInfo ipAddressInfo = null;

        //    this.Terminal.ExitConfigurationMode();

        //    string response = this.Terminal.SendSync("show ip interface " + interfaceName + " | include Internet address");

        //    if (response.Contains("address"))
        //    {
        //        string[] responseArray = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //        string[] ipAddressAndMaskBits = responseArray[responseArray.Length - 1].Split('/');

        //        string ipAddress = ipAddressAndMaskBits[0].Trim();
        //        int ipSubnetMaskNumOfBits = Convert.ToInt32(ipAddressAndMaskBits[1]);
        //        string ipSubnetMask = IPHelper.GetIPSubnetMask(ipSubnetMaskNumOfBits);

        //        ipAddressInfo = new IPAddressInfo(ipAddress, ipSubnetMask);
        //    }

        //    return ipAddressInfo;
        //}

        //private IPAddressInfo[] GetSecondaryIpAddresses(string interfaceName)
        //{
        //    List<IPAddressInfo> secondaryIpAddressList = new List<IPAddressInfo>();

        //    this.Terminal.ExitConfigurationMode();

        //    string response = this.Terminal.SendSync("show ip interface " + interfaceName + " | include Secondary address");

        //    if (response.Contains("address"))
        //    {
        //        string[] responseArray = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

        //        foreach (string line in responseArray)
        //        {
        //            string[] lineArray = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            string[] ipAddressAndMaskBits = lineArray[lineArray.Length - 1].Split('/');

        //            string ipAddress = ipAddressAndMaskBits[0];
        //            int ipSubnetMaskNumOfBits = Convert.ToInt32(ipAddressAndMaskBits[1]);
        //            string ipSubnetMask = IPHelper.GetIPSubnetMask(ipSubnetMaskNumOfBits);

        //            secondaryIpAddressList.Add(new IPAddressInfo(ipAddress, ipSubnetMask));
        //        }
        //    }

        //    return secondaryIpAddressList.ToArray();
        //}

        #endregion |   Interface IP Addresses   |

        #region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			const string strPortLinkType = "Port link-type";
			const string strAccess = "access";
			const string strTrunk = "trunk";
			const string strPvid = "PVID";
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			int vlanId = 1;

			if (interfaceName != null && interfaceName.Trim().Length > 0)
			{
				string response = await this.Provider.Terminal.SendAsync("display interface " + interfaceName);

				if (response.Contains(strPortLinkType))
				{
					string[] lines = response.ToLines();

					foreach (string line in lines)
					{
						if (line.Contains(strPortLinkType))
						{
							if (line.Contains(strTrunk))
							{
								switchportMode = InterfaceSwitchportMode.Trunk;

								break;
							}
							else if (line.Contains(strAccess))
							{
								switchportMode = InterfaceSwitchportMode.Access;
							}
						}
						else if (line.Contains(strPvid))
						{
							string restOfLine = line.Split(new string[] { strPvid }, StringSplitOptions.None)[1];
							string[] restOfLineSplited = restOfLine.Split(new string[] { " ", ":" }, StringSplitOptions.RemoveEmptyEntries);

							vlanId = Conversion.TryChangeType<int>(restOfLineSplited[restOfLineSplited.Length - 1]);

							break;
						}
					}
				}
			}

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
  //      {
  //          const string strPortLinkType = "port link-type";
  //          const string strAccess = "access";
  //          const string strTrunk = "trunk";
  //          InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;

		//	if (interfaceName != null && interfaceName.Trim().Length > 0)
		//	{
		//		string response = this.Provider.Terminal.Send("display interface " + interfaceName);

		//		if (response.Contains(strPortLinkType))
		//		{
		//			foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
		//			{
		//				if (line.Contains(strPortLinkType))
		//				{
		//					if (line.Contains(strAccess))
		//					{
		//						switchportMode = InterfaceSwitchportMode.Access;
		//						break;
		//					}
		//					else if (line.Contains(strTrunk))
		//					{
		//						switchportMode = InterfaceSwitchportMode.Trunk;
		//						break;
		//					}
		//					else
		//					{
		//						throw new Exception("Unredable InterafaceVlanAdministrativeType: " + line);
		//					}
		//				}
		//			}
		//		}
		//	}

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
				await this.Provider.Terminal.SendAsync("interface " + interfaceName);

				switch (switchportMode)
				{
					case InterfaceSwitchportMode.Access:

						await this.Provider.Terminal.SendAsync("port link-type access");

						break;

					case InterfaceSwitchportMode.Trunk:

						await this.Provider.Terminal.SendAsync("port link-type trunk");
						await this.Provider.Terminal.SendAsync("port trunk permit vlan all");

						break;
				}

				if (vlanId == 1)
					await this.Provider.Terminal.SendAsync("undo port access vlan");
				else
					await this.Provider.Terminal.SendAsync("port access vlan " + vlanId);

				await this.Provider.Terminal.SendAsync("quit");
			}
        }

        //public new int GetVlanId(string interfaceName)
        //{
        //    const string strPvid = "PVID";
        //    int vlanId = 1;

        //    string response = this.Provider.Terminal.Send("display interface " + interfaceName);

        //    foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
        //    {
        //        if (line.Contains(strPvid))
        //        {
        //            string restOfLine = line.Split(new string[] { strPvid }, StringSplitOptions.None)[1];
        //            string[] restOfLineSplited = restOfLine.Split(new string[] { " ", ":" }, StringSplitOptions.RemoveEmptyEntries);
        //            vlanId = Conversion.TryChangeType<int>(restOfLineSplited[restOfLineSplited.Length - 1]);
        //        }
        //    }

        //    return vlanId;
        //}

   //     public override void SetVlanId(string interfaceName, int vlanId)
   //     {
   //         string response = this.Provider.Terminal.Send("interface " + interfaceName);

   //         if (vlanId == 1)
   //         {
   //             response = this.Provider.Terminal.Send("undo port access vlan");
   //         }
			//else
   //         {
   //             response = this.Provider.Terminal.Send("port access vlan " + vlanId);
   //         }

   //         response = this.Provider.Terminal.Send("quit");
   //     }

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

		//[ProviderAction("Interfaces.AttachAcl")]
		//public virtual void AttachAcl(string interfaceName, string aclName, InterfaceAclDirection aclDirection)
		//{
		//    string strAclDirection = this.GetAclDirectionString(aclDirection);

		//    this.Terminal.EnterConfigurationMode();

		//    string response = this.Terminal.SendSync("ip access-list extended " + aclName);
		//    response = this.Terminal.SendSync("exit");     // exit access-list

		//    response = this.Terminal.SendSync("interface " + interfaceName);
		//    response = this.Terminal.SendSync("ip access-group " + aclName + " " + strAclDirection);
		//    response = this.Terminal.SendSync("exit");     // exit interface
		//}

		//[ProviderAction("Interfaces.DetachAcl")]
		//public virtual void DetachAcl(string interfaceName, string aclName, InterfaceAclDirection aclDirection)
		//{
		//    string strAclDirection = this.GetAclDirectionString(aclDirection);

		//    this.Terminal.EnterConfigurationMode();

		//    string response = this.Terminal.SendSync("interface " + interfaceName);
		//    response = this.Terminal.SendSync("no ip access-group " + aclName + " " + strAclDirection);
		//    response = this.Terminal.SendSync("exit");     // exit interface

		//    response = this.Terminal.SendSync("no ip access-list extended " + aclName);
		//}

		//[ProviderAction("Interfaces.AddAclRoule")]
		//public virtual void AddAclRoule(string aclName, InterfaceAclPermition permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, InterfaceAclPortCriteria sourcePortCriteria, ushort sourcePort, string destinationIpAddress, string destinationIpSubnetMask, InterfaceAclPortCriteria destinationPortCriteria, ushort destinationPort, bool established, bool logging)
		//{
		//    string aclRoule = this.CreateAclRoule(permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort, established, logging);
		//    this.ApplyAclRoule(aclName, aclRoule);
		//}

		//[ProviderAction("Interfaces.RemoveAclRoule")]
		//public virtual void RemoveAclRoule(string aclName, InterfaceAclPermition permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, InterfaceAclPortCriteria sourcePortCriteria, ushort sourcePort, string destinationIpAddress, string destinationIpSubnetMask, InterfaceAclPortCriteria destinationPortCriteria, ushort destinationPort, bool established, bool logging)
		//{
		//    string aclRoule = this.CreateAclRoule(permition, protocol, sourceIpAddress, sourceIpSubnetMask, sourcePortCriteria, sourcePort, destinationIpAddress, destinationIpSubnetMask, destinationPortCriteria, destinationPort, established, logging);
		//    this.ApplyAclRoule(aclName, "no " + aclRoule);
		//}

		//[ProviderAction("Interfaces.CreateAclRoule")]
		//public string CreateAclRoule(InterfaceAclPermition permition, byte protocol, string sourceIpAddress, string sourceIpSubnetMask, InterfaceAclPortCriteria sourcePortCriteria, ushort sourcePort, string destinationIpAddress, string destinationIpSubnetMask, InterfaceAclPortCriteria destinationPortCriteria, ushort destinationPort, bool established, bool logging)
		//{
		//    string aclRoule = "";

		//    string strPermition = permition == InterfaceAclPermition.Permit ? "permit" : "deny";

		//    aclRoule += strPermition + " " + protocol;

		//    aclRoule += " " + this.GetAclIpAddresssString(sourceIpAddress, sourceIpSubnetMask);

		//    string strSourcePort = this.GetAclPortString(sourcePortCriteria, sourcePort);
		//    if (strSourcePort != string.Empty)
		//    {
		//        aclRoule += " " + strSourcePort;
		//    }

		//    aclRoule += " " + this.GetAclIpAddresssString(destinationIpAddress, destinationIpSubnetMask);

		//    string strDestinationPort = this.GetAclPortString(destinationPortCriteria, destinationPort);
		//    if (strDestinationPort != string.Empty)
		//    {
		//        aclRoule += " " + strDestinationPort;
		//    }

		//    if (established)
		//    {
		//        aclRoule += " established";
		//    }

		//    if (logging)
		//    {
		//        aclRoule += " log";
		//    }

		//    return aclRoule;
		//}

		#endregion |   Interface ACL   |

		#region |   Protected Methods   |

		//// Sort interface info list by interface name
		//interfaceInfos = new List<ProviderInterfaceInfo>(interfaceInfos.OrderBy(item => item.InterfaceName, new StringLogicalComparer()));
		protected override async ValueTask GenerateInterfaceDictionary()
		{
			await base.GenerateInterfaceDictionary();

			var vetInterfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

			// Sort 3Com interface by natural logical comparer
			vetInterfaceIndexesByInterfaceName = vetInterfaceIndexesByInterfaceName.OrderBy(item => item.Key, new StringLogicalComparer()).ToDictionary(pair => pair.Key, pair => pair.Value);

			this.SetInterfaceIndexesByInterfaceName(vetInterfaceIndexesByInterfaceName);
		}

		//protected virtual string GetAclName(string interfaceName, InterfaceAclDirection aclDirection)
		//{
		//    return interfaceName + aclDirection.ToString();
		//}

		//protected virtual void ApplyAclRoule(string aclName, string aclRoule)
		//{
		//    this.Terminal.EnterConfigurationMode();

		//    this.Terminal.SendSync("ip access-list extended " + aclName);
		//    this.Terminal.SendSync(aclRoule);
		//    this.Terminal.SendSync("exit");
		//}

		//protected virtual string GetAclDirectionString(InterfaceAclDirection aclDirection)
		//{
		//    return aclDirection == InterfaceAclDirection.In ? "in" : "out";
		//}

		//protected virtual string GetAclIpAddresssString(string ipAddress, string ipSubnetMask)
		//{
		//    string result = "";
		//    int ipSubnetMaskNumOfBits = IPHelper.GetNumOfBitsByIPSubnetMask(ipSubnetMask);

		//    if (ipSubnetMaskNumOfBits == 0)
		//    {
		//        result = "any";
		//    }
		//    else if (ipSubnetMaskNumOfBits == 32)
		//    {
		//        result = ipAddress + " host ";
		//    }
		//    else
		//    {
		//        result = ipAddress + " " + IPHelper.GetIPSubnetMaskWildCard(ipSubnetMaskNumOfBits);
		//    }

		//    return result;
		//}

		//protected virtual string GetAclPortString(InterfaceAclPortCriteria portCriteria, ushort port)
		//{
		//    string result = string.Empty;

		//    switch (portCriteria)
		//    {
		//        case InterfaceAclPortCriteria.None:
		//            break;
		//        case InterfaceAclPortCriteria.Equal:
		//            result = "eq";
		//            break;
		//        case InterfaceAclPortCriteria.NotEqual:
		//            result = "neq";
		//            break;
		//        case InterfaceAclPortCriteria.GreaterThan:
		//            result = "gt";
		//            break;
		//        case InterfaceAclPortCriteria.LessThan:
		//            result = "lt";
		//            break;
		//    }

		//    if (result != string.Empty)
		//    {
		//        result += " " + port;
		//    }

		//    return result;
		//}

		#endregion |   Protected Methods   |
	}
}
