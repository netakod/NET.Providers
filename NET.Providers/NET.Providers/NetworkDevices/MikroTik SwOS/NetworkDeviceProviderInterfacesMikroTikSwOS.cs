using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikSwOS)]
    public class NetworkDeviceProviderInterfacesMikroTikSwOS : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private List<string> trunkPortInterfaceNames = null;

		#endregion |   Private Members   |

		#region |   Protected Properties   |

		protected WebClientMikroTikSwOS WebClient => this.Provider.Web as WebClientMikroTikSwOS;

		#endregion |   Protected Properties   |

		#region |   Public Properties   |

		public async ValueTask<List<string>> GetTrunkPortInterfaceNames()
        {
			if (this.trunkPortInterfaceNames == null)
			{
				this.trunkPortInterfaceNames = new List<string>();

				IEnumerable<string> interfaceNames = await this.GetInterfaceNames();
				Dictionary<string, string> dvidDictionary = await this.WebClient.GetDvidDictionary();

				//HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync("link.b");
				//string responseText = await response.GetResponseTextAsync();
				//Dictionary<string, string> keyValuePairs = this.WebClient.ParseConfigLine(responseText);
				//string[] vlanReceiveValues = null;
				//string[] defaultVlanIdValues = null;
				//string[] vlanHeaderValues = null;

				string vlanModeValueText = dvidDictionary["vlan"];
				string[] vlanModeValues = this.WebClient.ParseMultipleValues(vlanModeValueText);

				string vlanReceiveValueText = dvidDictionary["vlni"]; // Gets VLAN Receive from "vlni:[0x00,0x00,0x00,0x00,0x00,0x00]"
				string[] vlanReceiveValues = this.WebClient.ParseMultipleValues(vlanReceiveValueText);

				// Gets VLAN Header from "vlnh:[0x02,0x01,0x01,0x00,0x00,0x00]"
				string? vlanHeaderValueText = null;
				string[]? vlanHeaderValues = null;

				// Gets VLAN Header from "vlnh:[0x02,0x01,0x01,0x00,0x00,0x00]" -> newer SwOS has no header value support
				if (dvidDictionary.ContainsKey("vlnh"))
				{
					vlanHeaderValueText = dvidDictionary["vlnh"];
					vlanHeaderValues = this.WebClient.ParseMultipleValues(vlanHeaderValueText);
				}

				string vlanIdText = dvidDictionary["dvid"];
				string[] vlanIdValues = this.WebClient.ParseMultipleValues(vlanIdText);


				//if (keyValuePairs.ContainsKey("dvid"))
				//{
				//	string defaultVlanIdText = keyValuePairs["dvid"];
					
				//	defaultVlanIdValues = this.WebClient.ParseMultipleValues(defaultVlanIdText);
				//}

				//if (keyValuePairs.ContainsKey("vlnh"))
				//{
				//	string vlanHeaderValueText = keyValuePairs["vlnh"];
					
				//	vlanHeaderValues = this.WebClient.ParseMultipleValues(vlanHeaderValueText);
				//}

				foreach (string interfaceName in interfaceNames)
				{
					int zeroBasedInterfaceIndex = await this.GetIndex(interfaceName) - 1; // We need zero-based indexing index

					if (zeroBasedInterfaceIndex >= 0)
					{
						// Gets MikroTik vlan mode "vlan:[0x02,0x01,0x01,0x01,0x01,0x01]"
						string vlanModeHex = vlanModeValues[zeroBasedInterfaceIndex];
						MikroTikVlanMode vlanMode = (MikroTikVlanMode)this.WebClient.ConvertHexStringToInt32(vlanModeHex);

						// Gets VLAN Receive from "vlni:[0x00,0x00,0x00,0x00,0x00,0x00]"
						// This field does not exists in older SwOS software versions, thus check if field exists first
						string vlanReceiveHex = vlanReceiveValues[zeroBasedInterfaceIndex];
						MikroTikVlanReceive vlanReceive = (MikroTikVlanReceive)this.WebClient.ConvertHexStringToInt32(vlanReceiveHex);

						// Gets VLAN Header from "vlnh:[0x02,0x01,0x01,0x00,0x00,0x00]"
						MikroTikVlanHeader vlanHeader = default;

						if (vlanHeaderValues != null)
						{
							string vlanHeaderHex = vlanHeaderValues[zeroBasedInterfaceIndex];
							
							vlanHeader = (MikroTikVlanHeader)this.WebClient.ConvertHexStringToInt32(vlanHeaderHex);
						}

						// Gets Default VLAN ID from "dvid:[0x0001,0x0002,0x0003,0x0001,0x0001,0x0001]"
						string vlanIdHex = vlanIdValues[zeroBasedInterfaceIndex];
						int vlanId = this.WebClient.ConvertHexStringToInt32(vlanIdHex);

						// vlanReceive check is omitted
						if (vlanMode == MikroTikVlanMode.Enabled && vlanId == 1 && (vlanHeader == MikroTikVlanHeader.AddIfMissing || vlanHeader == MikroTikVlanHeader.LeaveAsIs))
							this.trunkPortInterfaceNames.Add(interfaceName);
					}
				}
			}

			return this.trunkPortInterfaceNames;
		}

        #endregion |   Public Properties   |

        #region |   Interface Data   |

        public override ValueTask SetDescription(string interfaceName, string description)
        {
			// Do nothing - description cannot be set.
			//throw new ProviderInfoException("Set interface description is not supported");
			return new ValueTask();
		}

		public override async ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName)
		{
			int zeroBasedInterfaceIndex = await this.GetIndex(interfaceName) - 1; // We need zero-based indexing index
																				  //string responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
																				  //Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
			var linkDictionary = await this.WebClient.GetLinkDictionary();
			string enValueText = linkDictionary["en"];
			ulong en = this.WebClient.ConvertHexStringToUInt64(enValueText);
			bool isEnabled = en.IsBitSet(zeroBasedInterfaceIndex);

			return (isEnabled) ? InterfaceAdminStatus.Up : InterfaceAdminStatus.Down;
		}

		public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
		{
			int zeroBasedInterfaceIndex = await this.GetIndex(interfaceName) - 1; // We need zero-based indexing index
																				  //string responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
																				  //Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
			if (zeroBasedInterfaceIndex < 0)
				return;
			
			var linkDictionary = await this.WebClient.GetLinkDictionary();
			string enValueText = linkDictionary["en"];
			ulong en = this.WebClient.ConvertHexStringToUInt64(enValueText);
			ulong interfaceMask = (ulong)1 << zeroBasedInterfaceIndex;

			if (adminStatus != InterfaceAdminStatus.Down)
				en |= interfaceMask; // en OR interfaceMask -> Set bit interface position to 1
			else
				en &= ~interfaceMask; // en AND (NOT interfaceMask) -> Set bit interface position to 0

			linkDictionary["en"] = this.WebClient.ConvertUInt64ToHexString(en, enValueText); // (enValueText.Length == 10) ? this.WebClient.ConvertUInt64ToHexString(en, 8) : this.WebClient.ConvertUInt64ToHexString(en);

			//keyValuePairs["en"] = NetworkDeviceProviderHelperMikroTik.ConvertInt32ToHexString(en);
			//string postData = "{" + keyValuePairs.ToConfigLine() + "}";
			//HttpWebResponse response = this.Provider.Web.SendPostRequest("link.b", postData);
		}

		#endregion |   Interface Data   |

		#region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
			var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();
			int vlanId = 1;

			if (trunkPortInterfaceNames.Contains(interfaceName))
			{
				switchportMode = InterfaceSwitchportMode.Trunk;
			}
			else
			{
				switchportMode = InterfaceSwitchportMode.Access;
				vlanId = await this.GetVlanId(interfaceName);
			}

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
  //      {
		//	return (this.TrunkPortInterfaceNames.Contains(interfaceName)) ? InterfaceSwitchportMode.Trunk : InterfaceSwitchportMode.Access;
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
				int zeroBasedInterfaceIndex = await this.GetIndex(interfaceName) - 1; // We need zero-based indexing index

				if (zeroBasedInterfaceIndex >= 0)
				{
					MikroTikVlanMode vlanMode = MikroTikVlanMode.Strict;
					MikroTikVlanReceive vlanReceive = MikroTikVlanReceive.Any;
					MikroTikVlanHeader vlanHeader = MikroTikVlanHeader.LeaveAsIs;

					//string responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
					//Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
					//int vlanId = 1;

					switch (switchportMode)
					{
						case InterfaceSwitchportMode.Access:

							vlanMode = MikroTikVlanMode.Strict;
							vlanReceive = MikroTikVlanReceive.Any;
							vlanHeader = MikroTikVlanHeader.AlwaysStrip;
							//vlanId = this.GetVlanId(interfaceName);

							if (this.trunkPortInterfaceNames != null && this.trunkPortInterfaceNames.Contains(interfaceName))
								this.trunkPortInterfaceNames.Remove(interfaceName);

							break;

						case InterfaceSwitchportMode.Trunk:

							vlanMode = MikroTikVlanMode.Enabled;
							vlanReceive = MikroTikVlanReceive.Any;
							vlanHeader = MikroTikVlanHeader.AddIfMissing;

							if (this.trunkPortInterfaceNames != null && !this.trunkPortInterfaceNames.Contains(interfaceName))
								this.trunkPortInterfaceNames.Add(interfaceName);

							//// Get Default VLAN ID from "dvid:[0x0001,0x0002,0x0003,0x0001,0x0001,0x0001]" and set new value 1 at the interfaceIndex position
							////vlanId = 1;
							//string defaultVlanIdText = this.WebControl.LinkDictionary["dvid"];
							//string[] defaultVlanIdValues = this.WebControl.ParseMultipleValues(defaultVlanIdText);

							//defaultVlanIdValues[zeroBasedInterfaceIndex] = this.WebControl.ConvertInt32ToHexString(vlanId, 4);
							//this.WebControl.LinkDictionary["dvid"] = String.Format("[{0}]", defaultVlanIdValues.ToMultipleValueString());

							break;
					}

					// Gets MikroTik vlan mode "vlan:[0x02,0x01,0x01,0x01,0x01,0x01]" and set new value at the interfaceIndex position

					Dictionary<string, string> dvidDictionary = await this.WebClient.GetDvidDictionary();
					string vlanModeValuesText = dvidDictionary["vlan"];
					string[] vlanModeValues = this.WebClient.ParseMultipleValues(vlanModeValuesText);
					string vlanModeHexValueText = vlanModeValues[zeroBasedInterfaceIndex];

					vlanModeValues[zeroBasedInterfaceIndex] = this.WebClient.ConvertInt32ToHexString((int)vlanMode, oldHexValueText: vlanModeHexValueText);
					dvidDictionary["vlan"] = String.Format("[{0}]", this.WebClient.CreateMultipleValueText(vlanModeValues));

					// Gets VLAN Receive from "vlni:[0x00,0x00,0x00,0x00,0x00,0x00]" and set new value at the interfaceIndex position
					// This field does not exists in older SwOS software versions, thus check if field exists first
					string vlanReceiveValueText = dvidDictionary["vlni"];
					string[] vlanReceiveValues = this.WebClient.ParseMultipleValues(vlanReceiveValueText);
					string vlanReceiveHexValueText = vlanReceiveValues[zeroBasedInterfaceIndex];

					vlanReceiveValues[zeroBasedInterfaceIndex] = this.WebClient.ConvertInt32ToHexString((int)vlanReceive, oldHexValueText: vlanReceiveHexValueText);
					dvidDictionary["vlni"] = String.Format("[{0}]", this.WebClient.CreateMultipleValueText(vlanReceiveValues));

					// Gets VLAN Header from "vlnh:[0x02,0x01,0x01,0x00,0x00,0x00]" -> newer SwOS has no header value support
					if (dvidDictionary.ContainsKey("vlnh"))
					{
						string vlanHeaderValueText = dvidDictionary["vlnh"];
						string[] vlanHeaderValues = this.WebClient.ParseMultipleValues(vlanHeaderValueText);
						string vlanHeaderHexValueText = vlanHeaderValues[zeroBasedInterfaceIndex];

						vlanHeaderValues[zeroBasedInterfaceIndex] = this.WebClient.ConvertInt32ToHexString((int)vlanHeader, oldHexValueText: vlanHeaderHexValueText);
						dvidDictionary["vlnh"] = String.Format("[{0}]", this.WebClient.CreateMultipleValueText(vlanHeaderValues));
					}

					//string postData = "{" + keyValuePairs.ToConfigLine() + "}";
					//HttpWebResponse response = this.Provider.Web.SendPostRequest("link.b", postData);

					// Set vlanId
					string vlanIdText = dvidDictionary["dvid"];
					string[] vlanIdValues = this.WebClient.ParseMultipleValues(vlanIdText);
					string vlanIdHexValueText = vlanIdValues[zeroBasedInterfaceIndex];

					vlanIdValues[zeroBasedInterfaceIndex] = this.WebClient.ConvertInt32ToHexString(vlanId, oldHexValueText: vlanIdHexValueText); // 4);
					dvidDictionary["dvid"] = String.Format("[{0}]", this.WebClient.CreateMultipleValueText(vlanIdValues));

					// Now we must go through all vlans and set vlan port policy also
					await this.SetSwitchportPolicy(zeroBasedInterfaceIndex, switchportMode, vlanId);
				}
				else
				{
					throw new Exception("The interface index is not found: InterfaceName=" + interfaceName);
				}
			}
        }

		//public override void SetVlanId(string interfaceName, int vlanId)
		//{
		//	int zeroBasedInterfaceIndex = this.GetIndex(interfaceName) - 1; // We need zero-based indexing index

		//	if (zeroBasedInterfaceIndex >= 0)
		//	{
		//		//string responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
		//		//Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
		//		string vlanIdText = this.WebControl.LinkDictionary["dvid"];
		//		string[] vlanIdValues = this.WebControl.ParseMultipleValues(vlanIdText);

		//		vlanIdValues[zeroBasedInterfaceIndex] = this.WebControl.ConvertInt32ToHexString(vlanId, 4);
		//		this.WebControl.LinkDictionary["dvid"] = String.Format("[{0}]", vlanIdValues.ToMultipleValueString());

		//		//string postData = "{" + keyValuePairs.ToConfigLine() + "}";
		//		//HttpWebResponse response = this.Provider.Web.SendPostRequest("link.b", postData);

		//		// Now we must go throght all vlans and set vlan port policy also
		//		InterfaceSwitchportMode switchportMode = this.GetSwitchportMode(interfaceName);
		//		this.SetSwitchportPolicy(zeroBasedInterfaceIndex, switchportMode, vlanId);
		//	}
		//	else
		//	{
		//		throw new IndexOutOfRangeException("Interface cannot be found: InterfaceName=" + interfaceName);
		//	}
		//}

		#endregion |   Interface Vlans   |

		#region |   Protected Methods   |

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{
			int zeroBasedInterfaceIndex = await this.GetIndex(interfaceName) - 1; // We need zero-based indexing index

			//string responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
			//Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
			Dictionary<string, string> dvidDictionary = await this.WebClient.GetDvidDictionary();
			int vlanId = 1;

			//if (dvidDictionary.ContainsKey("dvid"))
			//{
				string vlanIdText = dvidDictionary["dvid"];
				string[] vlanIdValues = this.WebClient.ParseMultipleValues(vlanIdText);
				string vlanIdHex = vlanIdValues[zeroBasedInterfaceIndex];
				
				vlanId = this.WebClient.ConvertHexStringToInt32(vlanIdHex);
			//}

			return vlanId;
		}

		internal protected async ValueTask SetSwitchportPolicy(int zeroBasedInterfaceIndex, InterfaceSwitchportMode switchportMode, int vlanId)
		{
			//List<string> trunkPortNames = (this.Provider.Interfaces as NetworkDeviceProviderInterfacesMikroTikSwOS).TrunkPortInterfaceNames;
			//IEnumerable<string> interfaceNames = this.Provider.Interfaces.GetInterfaces().Select(item => item.InterfaceName);
			//string responseText = this.Provider.Web.SendGetRequest("vlan.b").GetResponseText();
			//List<string> vlanConfigSegments = NetworkDeviceProviderHelperMikroTik.CreateConfigSegments(responseText);

			//for (int i = 0; i < this.WebControl.VlanConfigSegments.Count; i++)
			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();

			foreach (var keyValuePairs in vlanConfigSegments)
			{
				if (keyValuePairs.ContainsKey("prt")) // for old Mikrotik SwOS images
				{
					MikroTikVlanHeader switchportPolicy = MikroTikVlanHeader.LeaveAsIs;
					//string vlanSegment = this.WebControl.VlanConfigSegments[i];
					//Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(vlanSegment);

					string prtValuesText = keyValuePairs["prt"];
					string[] prtValues = this.WebClient.ParseMultipleValues(prtValuesText);
					string prtHexValueText = prtValues[zeroBasedInterfaceIndex];

					if (switchportMode == InterfaceSwitchportMode.Access) // For trunk ports do nothing
					{
						int currentVlanId = this.WebClient.ConvertHexStringToInt32(keyValuePairs["vid"]);

						if (currentVlanId != vlanId)
							switchportPolicy = MikroTikVlanHeader.NotAMember;
					}

					prtValues[zeroBasedInterfaceIndex] = this.WebClient.ConvertInt32ToHexString((int)switchportPolicy, oldHexValueText: prtHexValueText);
					keyValuePairs["prt"] = "[" + this.WebClient.CreateMultipleValueText(prtValues) + "]";

					//this.WebControl.VlanConfigSegments[i] = keyValuePairs.ToConfigLine();
				}
				else if (keyValuePairs.ContainsKey("mbr"))
				{
					int currentVlanId = this.WebClient.ConvertHexStringToInt32(keyValuePairs["vid"]);
					ulong portPositionMask = (ulong)1 << zeroBasedInterfaceIndex;
					string mbrValuesText = keyValuePairs["mbr"];
					ulong mbrPortHexValue = this.WebClient.ConvertHexStringToUInt64(mbrValuesText);

					if (currentVlanId == vlanId || switchportMode == InterfaceSwitchportMode.Trunk) // Trank port is member of all vlans
						mbrPortHexValue |= portPositionMask; // Set port to 1
					else
						mbrPortHexValue &= ~portPositionMask; // Unset this port
						
					keyValuePairs["mbr"] = this.WebClient.ConvertUInt64ToHexString(mbrPortHexValue, oldHexValueText: mbrValuesText);
				}
			}

			//string postData = NetworkDeviceProviderHelperMikroTik.CreateConfigTextFromConfigSegments(this.WebControl.VlanConfigSegments);
			//HttpWebResponse response = this.Provider.Web.SendPostRequest("vlan.b", postData);
		}

		#endregion |   Protected Methods   |

		#region |   Private Methods   |

		#endregion |   Private Methods   |
	}

	#region |   Internal Enums   |

	internal enum MikroTikVlanMode
	{
		Disabled = 0,
		Optional = 1,
		Enabled = 2,
		Strict = 3
	}

	internal enum MikroTikVlanReceive
	{
		Any = 0,
		OnlyTagged = 1,
		OnlyUntagged = 2
	}

	internal enum MikroTikVlanHeader
	{
		LeaveAsIs = 0,
		AlwaysStrip = 1,
		AddIfMissing = 2,
		NotAMember = 3
	}

	#endregion |   Internal Enums   |
}
