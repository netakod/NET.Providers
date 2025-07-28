using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikRouterOS)]
    public class NetworkDeviceProviderVlansMikroTikRouterOS : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		private bool isBridgeTrunkSet = false;
		private const string strBridgeTrunk = "bridge-trunk";
		
		public override bool IsVlanSupported()
        {
            return true;
        }

		public async ValueTask<string> GetBridgeTrunk()
		{
			if (!this.isBridgeTrunkSet)
			{
				string response = await this.Provider.Terminal.SendAsync("interface bridge print without-paging");

				if (!response.Contains($"name=\"{strBridgeTrunk}\""))
					await this.Provider.Terminal.SendAsync($"interface bridge add name={strBridgeTrunk} disabled=no protocol-mode=rstp");

				this.isBridgeTrunkSet = true;
			}

			return strBridgeTrunk;
		}

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlanInfos()
        {
            List<VlanInfo> result = new List<VlanInfo>();
            string response = await this.Provider.Terminal.SendAsync("interface vlan print without-paging");
			var lines = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1);

			foreach (string line in lines)
			{
				VlanInfo vlanInfo = this.GetVlanInfo(line);

				if (vlanInfo.VlanId > 0)
					result.Add(vlanInfo);
			}

			return result;
        }

        public override async ValueTask Set(int vlanId, string name)
        {
			// Check if vlan already exists first
			string response = await this.Provider.Terminal.SendAsync("interface vlan print where vlan-id=" + vlanId);
			string vlanInfo = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join().Trim();

			if (vlanInfo.Length == 0) // vlan does not exists
			{
				string fixedName = "Vlan" + vlanId;
				var bridgeTrunk = await this.GetBridgeTrunk();
				
				await this.Provider.Terminal.SendAsync(String.Format("interface vlan add vlan-id={0} name={1} interface={2} disabled=no", vlanId, fixedName, bridgeTrunk));

				// Add switch port vlan in all trunk interfaces
				var trunkInterfaceNames = await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesMikroTikRouterOS).GetTrunkPortInterfaceNames();

				foreach (string interfaceName in trunkInterfaceNames)
				{
					string switchName = await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesMikroTikRouterOS).GetInterfaceEthernetSwitchName(interfaceName);
					
					await this.Provider.Terminal.SendAsync(String.Format("interface ethernet switch vlan add ports={0} switch={1} vlan-id={2}", interfaceName, switchName, vlanId));
				}
			}
			else // vlan exists, set name only
			{
				await this.SetName(vlanId, name);
			}
		}

		public override async ValueTask Remove(int vlanId)
		{
			string vlanName = await this.GetName(vlanId);

			if (!vlanName.IsNullOrEmpty())
				await this.Provider.Terminal.SendAsync("interface vlan remove " + vlanName);
		}

        public override async ValueTask<string> GetName(int vlanId)
        {
			string name = String.Empty;
			string response = await this.Provider.Terminal.SendAsync("interface vlan print where vlan-id=" + vlanId);
			string infoLine = response.RemoveFirstLines(s => s.TrimStart().StartsWith("#")).Skip(1).Join();
			var vlanInfo = this.GetVlanInfo(infoLine);

			if (vlanInfo.VlanId > 0)
				name = vlanInfo.VlanName;

			return name;
        }

        private async ValueTask SetName(int vlanId, string name)
        {
			string currentName = await this.GetName(vlanId);
			string fixedName = "Vlan" + vlanId;

			if (currentName != fixedName)
				await this.Provider.Terminal.SendAsync("interface vlan set {0} name={1}" + currentName, fixedName);
        }

		private VlanInfo GetVlanInfo(string vlanLine)
		{
			int vlanId = 0;
			string vlanName = String.Empty;
			string[] lineArray = vlanLine.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

			if (lineArray.Length >= 6)
			{
				vlanId = Conversion.TryChangeType<int>(lineArray[lineArray.Length - 2]);
				vlanName = lineArray[lineArray.Length - 5];
			}
			else
			{
				vlanName = "ErrorGettingVlanInfo from: " + vlanLine;
			}

			return new VlanInfo(vlanId, vlanName);
		}
	}
}
