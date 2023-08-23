using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXEL)]
    public class NetworkDeviceProviderVlansZyXEL : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            const string strVlan = "vlan";
            const string strName = "name";
            List<VlanInfo> result = new List<VlanInfo>();
            VlanInfo providerVlanInfo = VlanInfo.Empty;

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show run");
            string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Length == 0)
                    continue;

                string[] lineItems = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (lineItems.First().ToLower() == strVlan)
                {
                    string vlanIdString = lineItems.Last();
                    int vlanId = Conversion.TryChangeType<int>(vlanIdString);
                    
                    providerVlanInfo = new VlanInfo(vlanId, String.Empty);
                    result.Add(providerVlanInfo);
                }
                else if (lineItems.First().ToLower() == strName && providerVlanInfo != VlanInfo.Empty && String.IsNullOrEmpty(providerVlanInfo.VlanName))
                {
                    string vlanName = lineItems.Last();

					providerVlanInfo.VlanName = vlanName;
                }
            }

            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
			//// First find all trunk ports
			//List<int> trunkPortNumbers = new List<int>();

			//IEnumerable<ProviderInterfaceInfo> interfaceInfos = this.Provider.Interfaces.GetInterfaces();

			//foreach (ProviderInterfaceInfo interfaceInfo in interfaceInfos)
			//{
			//    InterfaceSwitchportMode interfaceSwitchportMode = this.Provider.Interfaces.GetSwitchportMode(interfaceInfo.InterfaceName);

			//    if (interfaceSwitchportMode == InterfaceSwitchportMode.Trunk)
			//    {
			//        int portNumber = (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXEL).GetPortNumberFromPortInterfaceName(interfaceInfo.InterfaceName);

			//        trunkPortNumbers.Add(portNumber);
			//    }
			//}

			// Create vlan and set all trunk ports to be fixed
			string zyxelVlanName = name.IsNullOrEmpty() ? " " : name.Trim().Replace(' ', '_');
			IList<string> trunkPortInterfaceNames =  await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXEL).GetTrunkPortInterfaceNames();

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
			await this.Provider.Terminal.SendAsync("name " + zyxelVlanName);

			foreach (string trunkInterfaceName in trunkPortInterfaceNames)
            {
                int portNumber = (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXEL).GetPortNumberFromPortInterfaceName(trunkInterfaceName);
                
                await this.Provider.Terminal.SendAsync("fixed " + portNumber);
            }

            await this.Provider.Terminal.SendAsync("exit");
        }

        public override async ValueTask Remove(int vlanId)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("no vlan " + vlanId);
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
            string name = string.Empty;
            const string strName = "Name";

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show vlan " + vlanId);

            foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                if (!line.Contains(":"))
                    continue;

                string trimLine = line.Trim();
                string[] lineItems = trimLine.Split(':');

                string propName = lineItems[0].Trim();     // " Name: XYZ" --> Name
                string propValue = lineItems[lineItems.Length - 1].Trim();  // line.Replace(propName + ":", "").Trim();    //   " Name: XYZ" --> "  XYZ" -> "XYZ"

                if (propName == strName)
                {
                    name = propValue;
                    
                    break;
                }
            }

            return name;
        }

        public override async ValueTask SetName(int vlanId, string vlanName)
        {
            string zyxelVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_');
            
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("name " + zyxelVlanName);
            await this.Provider.Terminal.SendAsync("exit");
        }
    }
}
