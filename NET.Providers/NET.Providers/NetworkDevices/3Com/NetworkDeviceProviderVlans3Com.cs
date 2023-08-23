using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.C3Com)]
    public class NetworkDeviceProviderVlans3Com : NetworkDeviceProviderVlans, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            const string strVlanId = "VLAN ID";
            const string strName = "Name";
            List<VlanInfo> result = new List<VlanInfo>();
            VlanInfo providerVlanInfo = VlanInfo.Empty;
            string response = await this.Provider.Terminal.SendAsync("display vlan all");
            string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.TrimStart().StartsWith(strVlanId))
                {
                    string[] lineItems = line.Split(':');
                    string vlanIdString = lineItems.Last();
                    int vlanId = Conversion.TryChangeType<int>(vlanIdString);
                    
                    providerVlanInfo = new VlanInfo(vlanId, String.Empty);
                    result.Add(providerVlanInfo);
                }

                if (line.TrimStart().StartsWith(strName) &&  providerVlanInfo != VlanInfo.Empty && String.IsNullOrEmpty(providerVlanInfo.VlanName))
                {
                    string[] lineItems = line.Split(':');
                    string vlanName = lineItems.Last().Trim();

                    providerVlanInfo.VlanName = vlanName;
                }
            }

            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
			string newVlanName = name.IsNullOrEmpty() ? " " : name.Trim().Replace(' ', '_');
			
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("name " + newVlanName);
			await this.Provider.Terminal.SendAsync("quit");
        }

        public override async ValueTask Remove(int vlanId)
        {
            await this.Provider.Terminal.SendAsync("undo vlan " + vlanId);
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
            string name = string.Empty;
            const string strName = "Name";
            string response = await this.Provider.Terminal.SendAsync("display vlan " + vlanId);

            foreach (string line in response.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                string trimLine = line.Trim();
                string[] lineItems = trimLine.Split(':');
                string propName = lineItems[0].Trim();     // " Name: XYZ" --> Name
                string propValue = line.Replace(propName + ":", "").Trim();    //   " Name: XYZ" --> "  XYZ" -> "XYZ"

                if (propName == strName)
                {
                    name = line.Split(new string[] { "Name:" }, StringSplitOptions.None)[1].Trim();
                    
                    break;
                }
            }

            return name;
        }

        public override async ValueTask SetName(int vlanId, string vlanName)
        {
            string newVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_');            
            
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("name " + newVlanName);
            await this.Provider.Terminal.SendAsync("quit");
        }
    }
}
