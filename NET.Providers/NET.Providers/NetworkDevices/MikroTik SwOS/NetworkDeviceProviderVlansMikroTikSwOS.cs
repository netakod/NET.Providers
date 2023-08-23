using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Simple;
using HtmlAgilityPack;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikSwOS)]
    public class NetworkDeviceProviderVlansMikroTikSwOS : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public WebClientMikroTikSwOS WebClient
		{
			get { return this.Provider.Web as WebClientMikroTikSwOS; }
		}

        public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            List<VlanInfo> result = new List<VlanInfo>();
			HashSet<int> vlanIds = new HashSet<int>();
			//         string responseText = this.Provider.Web.SendGetRequest("vlan.b").GetResponseText();
			//List<string> responseSegments = NetworkDeviceProviderHelperMikroTik.CreateConfigSegments(responseText);

			//for (int i = 0; i < this.WebControl.VlanConfigSegments.Count; i++)

			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();

			foreach (var keyValuePairs in vlanConfigSegments)
			{
				//string vlanSegment = this.WebControl.VlanConfigSegments[i];
				//Dictionary<string, string> keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(vlanSegment);

				string hexVlanIdText = keyValuePairs["vid"]; // Vlans has no name, only VLAN ID exists
				int vlanId = this.WebClient.ConvertHexStringToInt32(hexVlanIdText);
				string vlanName = "Vlan" + vlanId;

				if (keyValuePairs.ContainsKey("nm"))
				{
					string hexVlanName = keyValuePairs["nm"];

					//hexVlanName = hexVlanName.Substring(1, hexVlanName.Length - 2);

					vlanName = this.WebClient.ConvertAsciiToString(hexVlanName);
				}

				if (vlanId > 0 && !vlanIds.Contains(vlanId)) // result.FirstOrDefault(item => item.VlanId == vlanId) == null)
				{
					result.Add(new VlanInfo(vlanId, vlanName));
					vlanIds.Add(vlanId);
				}
			}

			if (result.Count == 0)
				result.Add(new VlanInfo(1, "Default")); // 

			//foreach (string segment in responseSegments)
			//{
			//	var keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(segment);
			//	string hexVlanIdText = keyValuePairs["vid"]; // Vlans has no name, only VLAN ID exists
			//	int vlanId = NetworkDeviceProviderHelperMikroTik.ConvertHexStringToInt32(hexVlanIdText);

			//	if (vlanId > 0 && result.FirstOrDefault(item => item.VlanId == vlanId) == null)
			//		result.Add(new ProviderVlanInfo(vlanId, "Vlan" + vlanId));
			//}

			 return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
			List<string> trunkPortNames = await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesMikroTikSwOS).GetTrunkPortInterfaceNames();
			IEnumerable<string> interfaceNames = await this.Provider.Interfaces.GetInterfaceNames();
			//string responseText = this.Provider.Web.SendGetRequest("vlan.b").GetResponseText();
			//List<string> vlanConfigSegments = NetworkDeviceProviderHelperMikroTik.CreateConfigSegments(responseText);
			string asciiVlanName = this.WebClient.ConvertStringToAscii(name);
			//responseText = this.Provider.Web.SendGetRequest("link.b").GetResponseText();
			//Dictionary<string, string> linkKeyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(responseText);
			var dvidDictionary = await this.WebClient.GetDvidDictionary();
			string portVlanIdText = dvidDictionary["dvid"];
			string[] portVlanIdValues = this.WebClient.ParseMultipleValues(portVlanIdText);
			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();
			int vlanIdHexDecimalPlaces = 4;
			int vlanHeaderHexDecimalPlaces = 2;

			foreach (var keyValuePairs in vlanConfigSegments) // Check if vlan already exists
			{
				//var keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(vlanSegment);
				string hexCurrentVlanIdText = keyValuePairs["vid"]; // Vlans has no name, only VLAN ID exists
				int currentVlanId = this.WebClient.ConvertHexStringToInt32(hexCurrentVlanIdText);

				if (currentVlanId == vlanId) // vlan already exists, no need for adding
				{
					await this.SetName(vlanId, name);
					
					return;
				}

				vlanIdHexDecimalPlaces = hexCurrentVlanIdText.Length - 2;
				vlanHeaderHexDecimalPlaces = keyValuePairs["prt"].Length - 2;
			}

			string prtValues = "[";

			for (int i = 0; i < interfaceNames.Count(); i++)
			{
				if (i > 0)
					prtValues += ",";

				string interfaceName = interfaceNames.ElementAt(i);
				InterfaceSwitchportMode trunkPortMode = (trunkPortNames.Contains(interfaceName)) ? InterfaceSwitchportMode.Trunk : InterfaceSwitchportMode.Access;
				MikroTikVlanHeader vlanHeader = MikroTikVlanHeader.LeaveAsIs;

				if (trunkPortMode == InterfaceSwitchportMode.Access)
				{
					string portVlanIdHex = portVlanIdValues[i];
					int portVlanId = this.WebClient.ConvertHexStringToInt32(portVlanIdHex);

					if (portVlanId != vlanId)
						vlanHeader = MikroTikVlanHeader.NotAMember;
				}

				prtValues += this.WebClient.ConvertInt32ToHexString((int)vlanHeader, vlanHeaderHexDecimalPlaces);
			}
			
			prtValues += "]";

			vlanConfigSegments.Add(new Dictionary<string, string>() { { "vid", this.WebClient.ConvertInt32ToHexString(vlanId, vlanIdHexDecimalPlaces) },
																	  { "nm", asciiVlanName },
																	  { "prt", prtValues },
																	  { "ivl", "0x00" } });

			await this.SetName(vlanId, name);
			//vlanConfigSegment = String.Format("vid:{0},prt:{1},ivl:0x00", NetworkDeviceProviderHelperMikroTik.ConvertInt32ToHexString(vlanId, 4), prtValues);
			//this.WebControl.VlanConfigSegments.Add(vlanConfigSegment);

			//string postData = NetworkDeviceProviderHelperMikroTik.CreateConfigTextFromConfigSegments(vlanConfigSegments);
			//HttpWebResponse response = this.Provider.Web.SendPostRequest("vlan.b", postData);
		}

		public override async ValueTask Remove(int vlanId)
        {
			// Get the vlan list. Response example: "[{vid:0x0001,prt:[0x00,0x00,0x00,0x00,0x00,0x00],ivl:0x00},{vid:0x0002,prt:[0x00,0x00,0x00,0x00,0x00,0x00],ivl:0x00}]"
			// We need to find line with the same VlanId and to remove this line and send it back to the device.

			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();
			int vlanIndex = vlanConfigSegments.FindIndex(item => this.WebClient.ConvertHexStringToInt32(item["vid"]) == vlanId);

			if (vlanIndex >= 0)
				vlanConfigSegments.RemoveAt(vlanIndex);


			//for (int i = 0; i < this.WebControl.VlanConfigSegments.Count; i++)
			//{
			//	string vlanSegment = this.WebControl.VlanConfigSegments.ElementAt(i);
			//	var keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(vlanSegment);
			//	var vidItem = keyValuePairs.FirstOrDefault(item => item.Key == "vid");

			//	if (vidItem != default(KeyValuePair<string, string>))


			//	foreach (var item in keyValuePairs)
			//	{
			//		if (item.Key == "vid")
			//		{
			//			int deviceVlanIdValue = NetworkDeviceProviderHelperMikroTik.ConvertHexStringToInt32(item.Value);

			//			if (deviceVlanIdValue == vlanId) // vlanId match
			//			{
			//				vlanConfigSegments.RemoveAt(i); // remove vlan line
			//				isVlanFound = true;

			//				break;
			//			}
			//		}

			//	}




			//	string responseText = this.Provider.Web.SendGetRequest("vlan.b").GetResponseText();
			//List<string> vlanConfigSegments = new List<string>(NetworkDeviceProviderHelperMikroTik.CreateConfigSegments(responseText));
			//bool isVlanFound = false;
			//int i = 0;

			//while (i < vlanConfigSegments.Count && !isVlanFound)
			//{
			//	string vlanSegment = vlanConfigSegments[i];
			//	var keyValuePairs = NetworkDeviceProviderHelperMikroTik.ParseConfigLine(vlanSegment);

			//	foreach (var item in keyValuePairs)
			//	{
			//		if (item.Key == "vid")
			//		{
			//			int deviceVlanIdValue = NetworkDeviceProviderHelperMikroTik.ConvertHexStringToInt32(item.Value);

			//			if (deviceVlanIdValue == vlanId) // vlanId match
			//			{
			//				vlanConfigSegments.RemoveAt(i); // remove vlan line
			//				isVlanFound = true;

			//				break;
			//			}
			//		}
			//	}

			//	i++;
			//}

			//if (isVlanFound)
			//{
			//	string postData = NetworkDeviceProviderHelperMikroTik.CreateConfigTextFromConfigSegments(vlanConfigSegments);
			//	HttpWebResponse response = this.Provider.Web.SendPostRequest("vlan.b", postData);
			//}
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
			string vlanName = "Vlan" + vlanId;
			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();

			foreach (var keyValuePairs in vlanConfigSegments)
			{
				string hexVlanIdText = keyValuePairs["vid"]; 
				int currentVlanId = this.WebClient.ConvertHexStringToInt32(hexVlanIdText);
				
				if (currentVlanId == vlanId && keyValuePairs.ContainsKey("nm"))
				{
					string asciiVlanName = keyValuePairs["nm"];

					vlanName = this.WebClient.ConvertAsciiToString(asciiVlanName);

					break; ;
				}
			}

			return vlanName;
        }

        public override async ValueTask SetName(int vlanId, string name)
        {
			// Specific Vlan name cannot be set (no vlan naming on device).
			//throw new ProviderInfoException("Set vlan name is not supported");

			var vlanConfigSegments = await this.WebClient.GetVlanConfigSegments();

			foreach (var keyValuePairs in vlanConfigSegments)
			{
				string hexVlanIdText = keyValuePairs["vid"];
				int currentVlanId = this.WebClient.ConvertHexStringToInt32(hexVlanIdText);

				if (currentVlanId == vlanId && keyValuePairs.ContainsKey("nm"))
				{
					string asciiVlanName = this.WebClient.ConvertStringToAscii(name);

					keyValuePairs["nm"] = asciiVlanName;

					break;
				}
			}
		}
	}
}
