using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.HPProCurve)]
    public class NetworkDeviceProviderVlansHPProCurve : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        // TODO: I ovo promijeniti da se vlanovi dohvate iz konfe a ne iz 
        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
			//const string strVlan = "vlan";
			//const string strName = "name";
            List<VlanInfo> result = new List<VlanInfo>();
            //ProviderVlanInfo providerVlanInfo = null;

			//this.Provider.DeviceConnection.Terminal.ExitConfigMode();
			//string response = this.Provider.DeviceConnection.Terminal.Send("show run");
			//string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			//for (int i = 0; i < lines.Length; i++)
			//{
			//	string line = lines[i].Trim();

			//	if (line.Length == 0)
			//		continue;

			//	string[] lineItems = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

			//	if (lineItems.First().ToLower() == strVlan)
			//	{
			//		string vlanIdString = lineItems.Last();
			//		int vlanId = Conversion.TryChangeType<int>(vlanIdString);
                    
			//		providerVlanInfo = new ProviderVlanInfo(vlanId, String.Empty);
			//		result.Add(providerVlanInfo);
			//	}
			//	else if (lineItems.First().ToLower() == strName && providerVlanInfo != null && String.IsNullOrEmpty(providerVlanInfo.VlanName))
			//	{
			//		string vlanName = lineItems.Last();
			//		providerVlanInfo.VlanName = vlanName;
			//	}
			//}

           // return result;

			string response = await this.Provider.Terminal.SendAsync("show vlans");
            string[][] vlanTable = ProviderHelperHPProCurve.GetTable(response, "----", skipLineAfterHeader: false);

            foreach (string[] lineArray in vlanTable)
            {
				int vlanIdIndex = 0;
                
				if (lineArray.First().StartsWith("\n"))
					vlanIdIndex++;

				int vlanId = Conversion.TryChangeType<int>(lineArray[vlanIdIndex].Trim());
				string vlanName = lineArray[vlanIdIndex + 1].Trim();

                result.Add(new VlanInfo(vlanId, vlanName));
            }
            
            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
            // Set tagging throught the trunks
            IEnumerable<string> trunkGroupNames = await ProviderHelperHPProCurve.GetInterfaceTrunkGroupNames(this.Provider.Terminal);
            string tagging = (vlanId == 1) ? "untagged" : "tagged";

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
			await this.Provider.Terminal.SendAsync("name " + name);

			foreach (string trunkGroupName in trunkGroupNames)
                await this.Provider.Terminal.SendAsync(String.Format("{0} {1}", tagging, trunkGroupName));
            
            await this.Provider.Terminal.SendAsync("exit");
        }

        public override async ValueTask Remove(int vlanId)
        {
                              await this.Provider.Terminal.EnterConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("no vlan " + vlanId, this.Provider.Terminal.PrivilegeModePrompts, "]"); // if HP respond "Do you want to continue? [y/n]

			if (response.ToLower().Contains("y/n"))
				await this.Provider.Terminal.SendAsync("y");
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
            string hpVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_');
			//string hpVlanName = "Vlan" + vlanId;

            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("vlan " + vlanId);
            await this.Provider.Terminal.SendAsync("name " + hpVlanName);
            await this.Provider.Terminal.SendAsync("exit");
        }
    }
}
