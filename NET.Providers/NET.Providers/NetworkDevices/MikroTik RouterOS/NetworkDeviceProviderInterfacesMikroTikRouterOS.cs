using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikRouterOS)]
	public class NetworkDeviceProviderInterfacesMikroTikRouterOS : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
	{
		private List<string> trunkPortInterfaceNames = null;
		
		#region |   Interface Data  |

		//public override string GetStandardizedName(string interfaceName)
		//{
		//	if (interfaceName.TrimEnd().EndsWith("-802.1Q vLAN subif"))
		//	{
		//		return interfaceName.Replace("-802.1Q vLAN subif", "");
		//	}
		//	else
		//	{
		//		return base.GetStandardizedName(interfaceName);
		//	}
		//}

		public override async ValueTask SetDescription(string interfaceName, string description)
		{
			try
			{
				await base.SetDescription(interfaceName, description);
			}
			catch
			{
				await this.Provider.Terminal.SendAsync($"interface {interfaceName} comment \"{description}\"");
			}
		}

		public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
		{
			try
			{
				await base.SetAdminStatus(interfaceName, adminStatus);
			}
			catch 
			{
				string disabled = (adminStatus) == InterfaceAdminStatus.Down ? "yes" : "no";
				string response = await this.Provider.Terminal.SendAsync($"interface set {interfaceName} disabled={disabled}");

				if (response.ToLower().Contains("no such item"))
					throw new ProviderInfoException($"Error set admin status on interface {interfaceName}: {response}");
			}
		}

		#endregion |   Interface Data   |

		#region |   Add Remove Interface   |

		public override bool IsAddRemoveSupported() => true;

		public override async ValueTask Add(string interfaceName)
		{
			if (interfaceName.TrimStart().ToLower().StartsWith("vlan"))
			{
				int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

				if (vlanId > 0)
				{
					await this.Provider.Terminal.SendAsync($"interface vlan add name={interfaceName} vlan-id={vlanId}");
					await this.GenerateInterfaceDictionary();
				}
				else
				{
					throw new Exception("Vlan interface name is not in proper format VlanX, where X is VLAN ID.");
				}
			}
			else
			{
				throw new Exception("Adding non vlan interface is not supported.");
			}
		}

		public override async ValueTask Remove(string interfaceName)
		{
			if (interfaceName.TrimStart().ToLower().StartsWith("vlan"))
			{
				int vlanId = NetworkDeviceHelper.GetVlanIdFromVlanInterfaceName(interfaceName);

				if (vlanId > 0)
				{
					await this.Provider.Vlans.Remove(vlanId);
					await this.GenerateInterfaceDictionary();
				}
				else
				{
					throw new Exception("Vlan interface name is not in proper format VlanX, where X is VLAN ID.");
				}
			}
			else
			{
				throw new Exception("Removing non vlan interface is not supported.");
			}
		}

		#endregion |   Add Remove Interface   |

		#region |   Interface IP Addresses   |

		public override async ValueTask<bool> IsIpAddressSupported(string interfaceName)
		{
			if (interfaceName == null || interfaceName.Trim() == "")
				return false;

			string response = await this.Provider.Terminal.SendAsync("interface print where name=" + interfaceName);
			string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();

			if (infoLine.Contains(interfaceName))
			{
				string[] lineArray = infoLine.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

				if (lineArray.Length >= 2)
				{
					string interfaceType = lineArray[lineArray.Length - 3].Trim();

					if (interfaceType == "bridge") // Add interface types with no IP address support
						return false;
				}
			}

			return true;
		}

		public override async ValueTask<bool> IsWriteIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);

		public override async ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName) => await this.IsIpAddressSupported(interfaceName);

		public override async ValueTask<NetworkInfo> GetIpAddress(string interfaceName)
		{
			var ipAddresses = await this.GetIpAddresses(interfaceName);
			
			return (ipAddresses.Count() > 0) ? ipAddresses.ElementAt(0) : new NetworkInfo("", "");
		}

		public override async ValueTask SetIpAddress(string interfaceName, IpAddress? ipAddress, int subnetMaskPrefix)
		{
			if (ipAddress != null)
			{
				await this.Provider.Terminal.SendAsync($"ip address add interface={interfaceName} address={ipAddress.ToString()} netmask={IpHelper.GetSubnetMask(subnetMaskPrefix)}");
			}
			else // no ip address to set - remove existing
			{
				NetworkInfo oldIpAddressInfo = await this.GetIpAddress(interfaceName);

				if (oldIpAddressInfo.IpAddressText.Trim().Length > 0 && oldIpAddressInfo.SubnetMask.Trim().Length > 0)
					await this.RemoveIpAddress(interfaceName, oldIpAddressInfo.IpAddressText, oldIpAddressInfo.SubnetMaskPrefix);
			}
		}

		public override async ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName)
		{
			List<NetworkInfo> result = await this.GetIpAddresses(interfaceName);

			if (result.Count > 0)
				result.RemoveAt(0);

			return result;
		}

		public override async ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
		{
			return await this.IsSecondaryIpAddressSupported(interfaceName);
		}

		public override async ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
		{
			//if (ipAddress.Trim() != "" && subnetMaskPrefix.Trim() != "")
				await this.SetIpAddress(interfaceName, ipAddress, subnetMaskPrefix);
		}

		public override async ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
		{
			//if (ipAddress.Trim() != "" && subnetMaskPrefix.Trim() != "")
				await this.RemoveIpAddress(interfaceName, ipAddress.ToString(), subnetMaskPrefix);
		}


		public override async ValueTask<List<NetworkInfo>> GetIpAddresses(string interfaceName)
		{
			List<NetworkInfo> result = new List<NetworkInfo>();
			string response = await this.Provider.Terminal.SendAsync("ip address print where interface=" + interfaceName);
			var lines = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1);

			foreach (string line in lines)
			{
				string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

				if (lineArray.Length >= 3 && lineArray[lineArray.Length - 1].Trim() == interfaceName)
				{
					string ipAddressWithMaskNumber = lineArray[lineArray.Length - 3]; // file format is "192.168.88.1/24"
					string[] ipAddressWithMaskNumberArray = ipAddressWithMaskNumber.Split('/');
					string ipAddress = ipAddressWithMaskNumberArray[0];
					int ipSubnetMaskNumOfBits = Conversion.TryChangeType<int>(ipAddressWithMaskNumberArray[1]);

					result.Add(new NetworkInfo(ipAddress, ipSubnetMaskNumOfBits));
				}
			}

			return result;
		}

		private async ValueTask RemoveIpAddress(string interfaceName, string ipAddress, int ipSubnetMaskNumberOfBits)
		{
			string response = await this.Provider.Terminal.SendAsync("ip address print without-paging" + interfaceName);
			var lines = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1);
			int number = -1;

			foreach (string line in lines)
			{
				string[] lineElements = line.ToLines();

				if (lineElements.Length >= 1 && lineElements[lineElements.Length - 1].Trim() == interfaceName && (lineElements[1].Trim() == ipAddress + "/" + ipSubnetMaskNumberOfBits))
				{
					number = Conversion.TryChangeType<int>(lineElements[0]);

					if (number >= 0)
						await this.Provider.Terminal.SendAsync("ip address remove " + number);
				}
			}
		}

		#endregion |   Interface IP Addresses   |

		#region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			string response = await this.Provider.Terminal.SendAsync("interface bridge port print where interface=" + interfaceName);
			string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();
			int vlanId = 1;

			if (infoLine.Contains(interfaceName))
			{
				// Vlan is supported, now we have to determine interface switchport mode.
				response = await this.Provider.Terminal.SendAsync("interface ethernet switch port print detail where name=" + interfaceName);
				infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("Flags:")).Skip(1).Join();

				if (!infoLine.Contains("vlan-mode=disabled"))
				{
					if (infoLine.Contains("vlan-header=always-strip"))
					{
						switchportMode =  InterfaceSwitchportMode.Access;
					}
					else
					{
						// TODO: Check if interface is double tagged (Q-in-Q) on any vlan
						switchportMode = InterfaceSwitchportMode.Trunk;
					}
				}
			}

			if (switchportMode == InterfaceSwitchportMode.Access)
				vlanId = await this.GetVlanId(interfaceName);

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
		//{
		//	string response = this.Provider.Terminal.Send("interface bridge port print where interface=" + interfaceName);
		//	string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();

		//	if (infoLine.Contains(interfaceName))
		//	{
		//		// Vlan is supported, now we have to determine interface switchport mode.
		//		response = this.Provider.Terminal.Send("interface ethernet switch port print detail where name=" + interfaceName);
		//		infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("Flags:")).Skip(1).Join();

		//		//if (infoLine.Contains("name=\"" + interfaceName + "\""))
		//		//{
		//			if (!infoLine.Contains("vlan-mode=disabled"))
		//			{
		//				if (infoLine.Contains("vlan-header=always-strip"))
		//				{
		//					return InterfaceSwitchportMode.Access;
		//				}
		//				else
		//				{
		//					// TODO: Check if interface is double tagged (Q-in-Q) on any vlan
		//					return InterfaceSwitchportMode.Trunk;
		//				}
		//			}
		//		//}
		//	}

		//	return InterfaceSwitchportMode.VlanIsNotSupported;
		//}

		public override async ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId)
		{
			string switchName = null;

			switch (switchportMode)
			{
				case InterfaceSwitchportMode.Access:

					await this.Provider.Terminal.SendAsync($"interface ethernet switch port set {interfaceName} vlan-mode=secure vlan-header=always-strip");
					switchName = await this.GetInterfaceEthernetSwitchName(interfaceName);

					if (this.trunkPortInterfaceNames != null && this.trunkPortInterfaceNames.Contains(interfaceName))
						this.trunkPortInterfaceNames.Remove(interfaceName);

					break;

				case InterfaceSwitchportMode.Trunk:

					await this.Provider.Terminal.SendAsync($"interface ethernet switch port set {interfaceName} vlan-mode=secure vlan-header=add-if-missing default-vlan-id=1");

					if (this.trunkPortInterfaceNames != null && !this.trunkPortInterfaceNames.Contains(interfaceName))
						this.trunkPortInterfaceNames.Add(interfaceName);

					switchName = await this.GetInterfaceEthernetSwitchName(interfaceName);
					var vlans = await this.Provider.Vlans.GetVlans();

					foreach (var vlan in vlans)
						await this.Provider.Terminal.SendAsync($"interface ethernet switch vlan add ports={interfaceName} switch={switchName} vlan-id={vlan.VlanId}");

					break;

				case InterfaceSwitchportMode.DoubleTagging:
					
					throw new ProviderInfoException("Port double tagging is not supported.");

				case InterfaceSwitchportMode.VlanIsNotSupported:
					
					throw new ProviderInfoException("Port vlan mode is not supported.");
			}

			if (switchportMode != InterfaceSwitchportMode.VlanIsNotSupported && !switchName.IsNullOrEmpty())
				await this.Provider.Terminal.SendAsync($"interface ethernet switch port set {interfaceName} default-vlan-id={vlanId}");
		}

		//public override void SetVlanId(string interfaceName, int vlanId)
		//{
		//	string switchName = this.GetInterfaceEthernetSwitchName(interfaceName);

		//	if (!switchName.IsNullOrEmpty())
		//	{
		//		//string response = this.Provider.Terminal.Send(String.Format("interface ethernet switch vlan add ports={0}, switch={1} vlan-id={2}", interfaceName, switchName, vlanId));
		//		string response = this.Provider.Terminal.Send(String.Format("interface ethernet switch port set {0} default-vlan-id={1}", interfaceName, vlanId));
		//	}
		//	else
		//	{
		//		throw new ProviderException("Interface " + interfaceName + " does no belong to any switch CPU");
		//	}
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

		public override ValueTask AttachAcl(string interfaceName, string aclName, AclDirection aclDirection)
		{
			throw new ProviderException("Not implemented yet.");
		}

		public override ValueTask DetachAcl(string interfaceName, string aclName, AclDirection aclDirection)
		{
			throw new ProviderException("Not implemented yet.");
		}

		#endregion |   Interface ACL   |

		#region |   Protected Methods    |

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{
			int vlanId = 1;
			string response = await this.Provider.Terminal.SendAsync("interface ethernet switch port print where name=" + interfaceName);
			string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();
			string[] infoLineArray = infoLine.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

			if (infoLineArray.Length >= 2)
				vlanId = Conversion.TryChangeType<int>(infoLineArray[infoLineArray.Length - 1]);

			return vlanId;
		}

		#endregion |   Protected Methods    |

		#region |   Helper Methods    |

		public async ValueTask<string> GetInterfaceEthernetSwitchName(string ethernetInterfaceName)
		{
			string switchName = null;
			string response = await this.Provider.Terminal.SendAsync("interface ethernet switch port print where name=" + ethernetInterfaceName);
			string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();
			string[] infoLineArray = infoLine.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

			if (infoLineArray.Length >= 2)
				switchName = infoLineArray[2];

			return switchName;
		}

		public async ValueTask<List<string>> GetTrunkPortInterfaceNames()
		{
			if (this.trunkPortInterfaceNames == null)
			{
				var interfaceNames = await this.GetInterfaceNames();

				foreach (string interfaceName in interfaceNames)
				{
					var switchportInfo = await this.GetSwitchportInfo(interfaceName);

					if (switchportInfo.SwitchportMode == InterfaceSwitchportMode.Trunk)
						this.trunkPortInterfaceNames.Add(interfaceName);
				}
			}

			return this.trunkPortInterfaceNames;
		}

		#endregion |   Helper Methods    |
	}
}
