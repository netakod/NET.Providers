using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXELWebManaged)]
    public class NetworkDeviceProviderInterfacesZyXELWebManaged : NetworkDeviceProviderInterfacesGeneric, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private List<string> trunkPortInterfaceNames = null;

		#endregion |   Private Members   |

		#region |   Public Properties   |

		public async ValueTask<List<string>> GetTrunkPortInterfaceNames()
        {
            if (this.trunkPortInterfaceNames == null)
            {
                this.trunkPortInterfaceNames = new List<string>();

				HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync("vlanlist.htm");
                string responseText = await response.GetResponseTextAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseText);

                HtmlNodeCollection tableCollection = doc.DocumentNode.SelectNodes("//table");

                for (int i = 0; i < tableCollection.Count - 1; i++)
                {
                    HtmlNode table = tableCollection[i];

                    if (table.InnerText.Contains("Member ports") && table.InnerText.Contains("Tag") && table.InnerText.Contains("Untag"))
                    {
                        // Get tag bgcolor
                        string bgColor = String.Empty;

                        string[] lines = table.InnerHtml.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            string text = line.Trim().ToLower();

                            if (bgColor == String.Empty)
                            {
                                if (text.StartsWith("<td") && text.Contains("bgcolor=") && text.Contains("<td>tag"))
                                {
                                    int bgColorStartPos = text.IndexOf("bgcolor=");
                                    int bgColorEndPos = text.IndexOf(">");
                                    bgColor = text.Substring(bgColorStartPos, bgColorEndPos - bgColorStartPos);
                                }
                            }
                            else
                            {
                                if (text.StartsWith(String.Format("<td {0}>", bgColor)))
                                {
                                    int portNameStartPos = text.IndexOf('>');
                                    int portNameEndPos = text.IndexOf("</td>");

                                    string portName = text.Substring(portNameStartPos + 1, portNameEndPos - portNameStartPos - 1);
                                    int portNumber = Conversion.TryChangeType<int>(portName);

                                    if (portNumber > 0)
                                    {
                                        portName = portNumber.ToString();

                                        if (!this.trunkPortInterfaceNames.Contains(portName))
                                            this.trunkPortInterfaceNames.Add(portName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return this.trunkPortInterfaceNames;
        }

		#endregion |   Public Properties   |

		#region |   Interface Data   |

		public override async ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName)
		{
			// SNMP gaves wrong result. It is administratively up (enabled) only if link is up (connected); otherwise is always down
			//InterfaceAdminStatus snmpAdminStatus = base.GetAdminStatus(interfaceName);

			PortConfigurator portConfig = new PortConfigurator(this.Provider, interfaceName);
			
			await portConfig.GetAsync();

			return (portConfig.Admin == Status.Enable) ? InterfaceAdminStatus.Up : InterfaceAdminStatus.Down;

			//InterfaceAdminStatus webAdminStatus = InterfaceAdminStatus.Down;

			//string responseText = this.Provider.Web.SendGetRequest(String.Format("setport.cgi?port={0}", interfaceName)).GetResponseText();
			//string resultText = GetPortConfigInfo(responseText, "document.portset.admin.value");
			//int adminValue = Conversion.TryChangeType<int>(resultText);

			//webAdminStatus = (adminValue == 1) ? InterfaceAdminStatus.Up : InterfaceAdminStatus.Down;

			//return webAdminStatus;
		}

		public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
		{
			//base.SetAdminStatus(interfaceName, adminStatus); // Setting admin status via SNMP has no effect.

			PortConfigurator portConfig = new PortConfigurator(this.Provider, interfaceName);
			
			await portConfig.GetAsync();
			
			portConfig.Admin = (adminStatus == InterfaceAdminStatus.Up) ? Status.Enable : Status.Disable;

			await portConfig.SetAsync();
		}

		public override ValueTask SetDescription(string interfaceName, string description)
        {
			throw new ProviderInfoException("Set interface description is not supported"); // Do nothing - port description cannot be set.
		}

		#endregion |   Interface Data   |

		#region |   Interface Vlans   |

		public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
		{
			int vlanId = 1;
			InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.Access;
			HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync("vlanlist.htm");
			string responseText = await response.GetResponseTextAsync();
			HtmlNodeCollection tableCollection = responseText.ToHtmlDocument().DocumentNode.SelectNodes("//table");

			for (int i = 0; i < tableCollection.Count - 1; i++)
			{
				HtmlNode table = tableCollection[i];

				if (table.InnerText.Contains("Member ports") && table.InnerText.Contains("Tag") && table.InnerText.Contains("Untag"))
				{
					// Get tag bgcolor
					string bgColor = String.Empty;
					string[] lines = table.InnerHtml.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

					foreach (string line in lines)
					{
						string text = line.Trim().ToLower();

						if (text.StartsWith("<td") && text.Contains("bgcolor=") && text.Contains("<td>tag"))
						{
							int bgcolorStartPos = text.IndexOf("bgcolor=");
							int bgcolorStopPos = text.IndexOf(">");
							
							bgColor = text.Substring(bgcolorStartPos, bgcolorStopPos - bgcolorStartPos);

							// Test if this port is marked (colored) as a trunk

							string displayInterfaceName = interfaceName;
							int interfaceNumber = Conversion.TryChangeType<int>(interfaceName);

							if (interfaceNumber > 0 && interfaceNumber < 10)
							{
								displayInterfaceName = String.Format("0{0}", interfaceName);
							}

							if (table.InnerHtml.ToLower().Contains(String.Format("<td {0}>{1}</td>", bgColor, displayInterfaceName)))
							{
								switchportMode = InterfaceSwitchportMode.Trunk;
							}
							else
							{
								switchportMode = InterfaceSwitchportMode.Access;
							}

							break;
						}
					}

					break;
				}
			}

			if (switchportMode == InterfaceSwitchportMode.Access)
				vlanId = await this.GetVlanId(interfaceName);

			return new SwitchportInfo(switchportMode, vlanId);
		}

		//public override InterfaceSwitchportMode GetSwitchportMode(string interfaceName)
		//{
		//	InterfaceSwitchportMode result = InterfaceSwitchportMode.Access;
		//	HtmlNodeCollection tableCollection = this.Provider.Web.SendGetRequest("vlanlist.htm").GetResponseText().ToHtmlDocument().DocumentNode.SelectNodes("//table");

		//	for (int i = 0; i < tableCollection.Count - 1; i++)
		//	{
		//		HtmlNode table = tableCollection[i];

		//		if (table.InnerText.Contains("Member ports") && table.InnerText.Contains("Tag") && table.InnerText.Contains("Untag"))
		//		{
		//			// Get tag bgcolor
		//			string bgColor = String.Empty;

		//			string[] lines = table.InnerHtml.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		//			foreach (string line in lines)
		//			{
		//				string text = line.Trim().ToLower();

		//				if (text.StartsWith("<td") && text.Contains("bgcolor=") && text.Contains("<td>tag"))
		//				{
		//					int bgcolorStartPos = text.IndexOf("bgcolor=");
		//					int bgcolorStopPos = text.IndexOf(">");
		//					bgColor = text.Substring(bgcolorStartPos, bgcolorStopPos - bgcolorStartPos);

		//					// Test if this port is marked (colored) as a trunk

		//					string displayInterfaceName = interfaceName;
		//					int interfaceNumber = Conversion.TryChangeType<int>(interfaceName);

		//					if (interfaceNumber > 0 && interfaceNumber < 10)
		//					{
		//						displayInterfaceName = String.Format("0{0}", interfaceName);
		//					}

		//					if (table.InnerHtml.ToLower().Contains(String.Format("<td {0}>{1}</td>", bgColor, displayInterfaceName)))
		//					{
		//						result = InterfaceSwitchportMode.Trunk;
		//					}
		//					else
		//					{
		//						result = InterfaceSwitchportMode.Access;
		//					}

		//					break;
		//				}
		//			}

		//			break;
		//		}
		//	}

		//	return result;
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
				await this.SetVlanId(interfaceName, vlanId);

				var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

				if (trunkPortInterfaceNames.Contains(interfaceName))
				{
					switch (switchportMode)
					{
						case InterfaceSwitchportMode.Access:

							await this.SetInterfaceVlanTagging(interfaceName, tagged: false);

							break;

						case InterfaceSwitchportMode.Trunk:

							await this.SetInterfaceVlanTagging(interfaceName, tagged: true);

							break;
					}
				}
            }
        }

		public async ValueTask SetInterfaceVlanTagging(string interfaceName, bool tagged)
        {
			var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

			if (trunkPortInterfaceNames.Contains(interfaceName))
			{
				if (tagged)
					this.trunkPortInterfaceNames.Add(interfaceName);
				else
					this.trunkPortInterfaceNames.Remove(interfaceName);
			}

            IEnumerable<VlanInfo> vlanInfos = await this.Provider.Vlans.GetVlans();
            int interfaceNumber = Conversion.TryChangeType<int>(interfaceName);

            // Set tagging for all vlans, preserving current port normal/untagged/tagged config
            foreach (VlanInfo vlanInfo in vlanInfos)
            {
                string portValnTaggingInfo = await this.CreateVlanInterfaceTaggingInfo(vlanInfo.VlanId);
				HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync(String.Format("setvlan.cgi?vid={0}&values={1}", vlanInfo.VlanId, portValnTaggingInfo));
                string webResponseText = await response.GetResponseTextAsync();
            }
        }

        public async ValueTask<string> CreateVlanInterfaceTaggingInfo(int vlanId)
        {
			var interfaces = await this.GetInterfaceNames();
			char[] portValnTaggingInfoChars = new char[interfaces.Count()];
			var trunkPortInterfaceNames = await this.GetTrunkPortInterfaceNames();

            for (int i = 0; i < interfaces.Count(); i++)
            {
                string interfaceName = (i + 1).ToString();
                
				portValnTaggingInfoChars[i] = trunkPortInterfaceNames.Contains(interfaceName) ? '2' 
																							  : '1';
            }

            return new string(portValnTaggingInfoChars);
        }

        #endregion |   Interface Vlans   |

        #region |   Protected Methods   |

        protected override async ValueTask GenerateInterfaceDictionary()
        {
            await base.GenerateInterfaceDictionary();

            Dictionary<string, int> newInterfaceIndexesByInterfaceName = new Dictionary<string,int>();
			var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

			// Remame strange interface names to 1, 2, 3, ...
			foreach (var interfaceIndexesByInterfaceNameItem in interfaceIndexesByInterfaceName)
                newInterfaceIndexesByInterfaceName.Add(interfaceIndexesByInterfaceNameItem.Value.ToString(), interfaceIndexesByInterfaceNameItem.Value);

            this.SetInterfaceIndexesByInterfaceName(newInterfaceIndexesByInterfaceName);
        }

		protected override async ValueTask<int> GetVlanId(string interfaceName)
		{

			PortConfigurator portConfig = new PortConfigurator(this.Provider, interfaceName);
			
			await portConfig.GetAsync();

			return portConfig.PVID;

			//         int vlanId = 1;
			//string responseText = this.Provider.Web.SendGetRequest(String.Format("setport.cgi?port={0}", interfaceName)).GetResponseText();
			//         string resultText = GetPortConfigInfo(responseText, "document.portset.pvid.value");

			//         vlanId = Conversion.TryChangeType<int>(resultText);

			//         return vlanId;
		}

		#endregion |   Protected Methods   |

		#region |   Protected Intrnal Methods   |

		protected internal async ValueTask SetVlanId(string interfaceName, int vlanId)
		{
			const string errorResponse = "PVID is not an existing vlan id";
			PortConfigurator portConfig = new PortConfigurator(this.Provider, interfaceName);
			
			await portConfig.GetAsync();
			portConfig.PVID = vlanId;

			string responseText = await portConfig.SetAsync();

			if (responseText.Contains(errorResponse))
				throw new InvalidOperationException(errorResponse + "!");

			//string responseText = this.Provider.Web.SendGetRequest(String.Format("setport.cgi?port={0}", interfaceName)).GetResponseText();

			//         string admin = GetPortConfigInfo(responseText, "document.portset.admin.value");
			//         string autoNego = GetPortConfigInfo(responseText, "document.portset.autonego.value");
			//         string speed = GetPortConfigInfo(responseText, "document.portset.speed.value");
			//         string flowControl = GetPortConfigInfo(responseText, "document.portset.flow_ctrl.value");
			//         string priority = GetPortConfigInfo(responseText, "document.portset.priority.value");

			//         string data = String.Format("port={0}&admin={1}&autonego={2}", interfaceName, admin, autoNego);

			//         if (autoNego.Trim() == "0") //Auto Negotiated is Disabled -> add speed
			//             data += String.Format("&speed={0}", speed);

			//         data += String.Format("&flow_ctrl={0}&priority={1}&pvid={2}", flowControl, priority, vlanId);

			//         responseText = this.Provider.Web.SendGetRequest(String.Format("portset.cgi?{0}", data)).GetResponseText();

			//         //            response = this.Provider.Connection.Web.GetPostResonse(String.Format("portset.cgi", interfaceName), String.Format("setport.cgi?port={0}", interfaceName), data);

			//         const string errorResponse = "PVID is not an existing vlan id";

			//         if (responseText.Contains(errorResponse))
			//             throw new InvalidOperationException(errorResponse + "!");

			//         //// Set untagged...
			//         //int interfaceNumber = Conversion.TryChangeType<int>(interfaceName);
			//         //string portValnTaggingInfo = this.GetVlanPortTaggingInfo(vlanId);
			//         //portValnTaggingInfo = portValnTaggingInfo.Remove(interfaceNumber, 1).Insert(interfaceNumber, "1"); // 1 - Untagged

			//         //response = this.Provider.Connection.Web.GetResponse(String.Format("setvlan.cgi?vid={0}&values={1}", vlanId, portValnTaggingInfo));
		}

		#endregion |   Protected Intrnal Methods   |

		#region |   Public Static Methods   |

		//private string CreatePortTaggingText()
		//{
		//    string result = String.Empty;
		//    IEnumerable<ProviderInterfaceInfo> interfaceInfos = this.GetInterfaces();

		//    foreach (ProviderInterfaceInfo interfaceInfo in interfaceInfos)
		//    {
		//        result += (this.TrunkPortInterfaceNames.Contains(interfaceInfo.InterfaceName)) ? "2" : "1"; // 1 - Untagged, 2 - Tagged
		//    }

		//    return result;
		//}

		public static string GetPortConfigInfo(string portSetHtmlResponse, string documentItemName)
		{
			string result = String.Empty;
			int pos = portSetHtmlResponse.IndexOf(documentItemName);

			if (pos > 0)
			{
				result = portSetHtmlResponse.Substring(pos); //, documentItemName.Length + 20);
				
				string[] lines = result.Split('=', ';');
				
				result = lines[1].Trim();
			}

			return result;
		}

		//private string GetVlanPortTaggingInfo(int vlanId)
		//{
		//    const string strPortNewArray = "port = new Array(";
		//    string portTaggingText = String.Empty;
		//    string response = this.Provider.Connection.Web.GetResponse(String.Format("showvlan.cgi?vlanid={0}", vlanId));
		//    int startPos = response.IndexOf(strPortNewArray) + strPortNewArray.Length;
		//    int stopPos = response.IndexOf(");", startPos + strPortNewArray.Length);

		//    response = response.Substring(startPos, stopPos - startPos);
		//    return response.Replace(",", "").Trim();
		//}

		#endregion |   Public Static Methods   |

		#region |   Helpers   |

		class PortConfigurator
		{
			public PortConfigurator(NetworkDeviceProvider provider, string port)
			{
				this.Provider = provider;
				this.Port = port;
				this.PVID = 1;
			}

			private NetworkDeviceProvider Provider { get; set; }

			public string Port { get; set; }
			public Status Admin { get; set; }
			public Status AutoNego { get; set; }
			public string Speed { get; set; }
			public Status FlowControl { get; set; }
			public int Priority { get; set; }
			public int PVID { get; set; }

			public async ValueTask GetAsync()
			{
				HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync(String.Format("setport.cgi?port={0}", this.Port));
				string responseText = await response.GetResponseTextAsync();

				string admin = GetPortConfigInfo(responseText, "document.portset.admin.value = ");
				string autoNego = GetPortConfigInfo(responseText, "document.portset.autonego.value = ");
				string speed = GetPortConfigInfo(responseText, "document.portset.speed.value = ");
				string flowControl = GetPortConfigInfo(responseText, "document.portset.flow_ctrl.value = ");
				string priority = GetPortConfigInfo(responseText, "document.portset.priority.value = ");
				string pvid = GetPortConfigInfo(responseText, "document.portset.pvid.value = ");

				this.Admin = (Conversion.TryChangeType<int>(admin) == (int)Status.Enable) ? Status.Enable : Status.Disable;
				this.AutoNego = (Conversion.TryChangeType<int>(autoNego) == (int)Status.Enable) ? Status.Enable : Status.Disable;
				this.Speed = speed;
				this.FlowControl = (Conversion.TryChangeType<int>(flowControl) == (int)Status.Enable) ? Status.Enable : Status.Disable;
				this.Priority = Conversion.TryChangeType<int>(priority);
				this.PVID = Conversion.TryChangeType<int>(pvid);
			}

			public async ValueTask<string> SetAsync()
			{
				string data = String.Format("port={0}&admin={1}&autonego={2}", this.Port.ToString(), (int)this.Admin, (int)this.AutoNego);

				if (this.AutoNego == Status.Disable) //Auto Negotiated is Disabled -> add speed
					data += String.Format("&speed={0}", this.Speed);

				data += String.Format("&flow_ctrl={0}&priority={1}&pvid={2}", (int)this.FlowControl, this.Priority, this.PVID);

				HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync(String.Format("portset.cgi?{0}", data));
				string responseText = await response.GetResponseTextAsync();

				return responseText;
			}
		}
		enum Status
		{
			Disable = 0,
			Enable = 1
		}

		#endregion |   Helpers   |
	}
}
