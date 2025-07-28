using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Dell)]
    public class NetworkDeviceProviderVlansDell : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlanInfos()
        {
            //const string strVlan = "vlan";
            //const string strName = "Name";
            List<VlanInfo> result = new List<VlanInfo>();
            VlanInfo providerVlanInfo = VlanInfo.Empty;

                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("show vlan");
            //string[][] vlanTable = ProviderHelper.GetTable(response, "----");

            string[] responseArray = response.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in responseArray)
            {
                string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                if (lineArray.Length >= 2)
                {
                    int vlanId = Conversion.TryChangeType<int>(lineArray[0]);
                    string vlanName = lineArray[1];

                    if (vlanId > 0)
                    {
                        providerVlanInfo = new VlanInfo(vlanId, vlanName);
                        result.Add(providerVlanInfo);
                    }
                }
                else if (lineArray.Length == 1 && providerVlanInfo != VlanInfo.Empty && !String.IsNullOrEmpty(providerVlanInfo.VlanName))
                {
                    char firstCharInLine = line.TrimStart()[0];
                    int position = line.IndexOf(firstCharInLine);

                    if (position > 3 && position < 6 )
                        providerVlanInfo.VlanName += lineArray[0];
                }
            }

            //string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            //for (int i = 0; i < lines.Length; i++)
            //{
            //    string line = lines[i].Trim();

            //    if (line.Length == 0)
            //        continue;




            //    if (line.ToLower().TrimStart().StartsWith(strVlan))
            //        continue;

            //    if (line.ToLower().TrimStart().StartsWith("----"))
            //        continue;

            //    string[] lineItems = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                
            //    //if (line.TrimStart().StartsWith(strVlanIsl))
            //    //{
            //    //    string[] lineItems = line.Split(':');
            //    //    string vlanIdString = lineItems.Last();
            //    //    int vlanId = Conversion.TryChangeType<int>(vlanIdString);
                    
            //    //    providerVlanInfo = new ProviderVlanInfo(vlanId, String.Empty);
            //    //    result.Add(providerVlanInfo);
            //    //}

            //    //if (line.TrimStart().StartsWith(strName) && providerVlanInfo != null && String.IsNullOrEmpty(providerVlanInfo.VlanName)) 
            //    //{
            //    //    string[] lineItems = line.Split(':');
            //    //    string vlanName = lineItems.Last().Trim();

            //    //    providerVlanInfo.VlanName = vlanName;
            //    //}
            //}

            //this.Provider.DeviceConnection.Terminal.Send("exit");

            return result;
        }

        public override async ValueTask Set(int vlanId, string name)
        {
			string newName = name.IsNullOrEmpty() ? " " : name.Replace(' ', '_');
			DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

			await this.Provider.Terminal.EnterConfigModeAsync();

			if (dellDeviceType == DellDeviceType.DellNetworkingNxxxx)
			{
				await this.Provider.Terminal.SendAsync("vlan " + vlanId);
				await this.Provider.Terminal.SendAsync("name " + newName);
				await this.Provider.Terminal.SendAsync("exit");
			}
			else
			{
				                  await this.Provider.Terminal.SendAsync("vlan database");
				                  await this.Provider.Terminal.SendAsync("vlan " + vlanId);
				string response = await this.Provider.Terminal.SendAsync("name " + newName);
				//response = this.Provider.DeviceConnection.Terminal.Send("apply");

				if (response.ToLower().Contains("abort"))
				{
					// Too many vlans - User 'abort' command to exit
					string failedReason = response;
					
                    await this.Provider.Terminal.SendAsync("abort");
					
                    throw new ProviderInfoException("Add VLAN has failed: " + failedReason);
				}
				else
				{
					response = await this.Provider.Terminal.SendAsync("exit");

					if (response.ToLower().Contains("abort"))
					{
						string failedReason = response;
						
                        await this.Provider.Terminal.SendAsync("abort");
						
                        throw new ProviderInfoException("Add VLAN has failed: " + failedReason);
					}
				}

				if (dellDeviceType == DellDeviceType.PowerConnect62xx)
				{
					List<string> trunkPortInterfaceNames = await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesDell).TrunkPortHelper.GetTrunkPortInterfaceNames();

					await this.Provider.Terminal.EnterConfigModeAsync();

					foreach (string interfaceName in trunkPortInterfaceNames)
					{
						await this.Provider.Terminal.SendAsync("interface " + interfaceName);
						await this.Provider.Terminal.SendAsync("switchport trunk allowed vlan add " + vlanId);
						await this.Provider.Terminal.SendAsync("exit");
					}
				}
			}
        }

        public override async ValueTask Remove(int vlanId)
        {
			await this.Provider.Terminal.EnterConfigModeAsync();

			DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

			if (dellDeviceType != DellDeviceType.DellNetworkingNxxxx)
				await this.Provider.Terminal.SendAsync("vlan database");

			await this.Provider.Terminal.SendAsync("no vlan " + vlanId);
			//response = this.Provider.DeviceConnection.Terminal.Send("apply");

			if (dellDeviceType != DellDeviceType.DellNetworkingNxxxx)
				await this.Provider.Terminal.SendAsync("exit");
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
            string name = String.Empty;
			DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();
			string vlanIdOrTag = (dellDeviceType == DellDeviceType.DellNetworkingNxxxx) ? "id" : "tag";
			string response = await this.Provider.Terminal.SendAsync("show vlan " + vlanIdOrTag + " " + vlanId);
            string[] responseArray = response.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in responseArray)
            {
                string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                if (lineArray.Length >= 2)
                {
                    int recievedVlanId = Conversion.TryChangeType<int>(lineArray[0]);

                    if (recievedVlanId == vlanId)
                        name = lineArray[1];
                }
                else if (lineArray.Length == 1 && !name.IsNullOrEmpty())
                {
                    char firstCharInLine = line.TrimStart()[0];
                    int position = line.IndexOf(firstCharInLine);

                    if (position > 3 && position < 6)
                        name += lineArray[0];
                }
            }

            return name;
        }

   //     public override async ValueTask SetName(int vlanId, string vlanName)
   //     {
			////string newVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_');

			////if (newVlanName.ToLower() == "vlan" + vlanId)
			////{
			////    newVlanName += "_";
			////}

			//DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

			//await this.Provider.Terminal.EnterConfigModeAsync();

			//if (dellDeviceType == DellDeviceType.DellNetworkingNxxxx)
			//{
			//	await this.Provider.Terminal.SendAsync("vlan " + vlanId);
			//}
			//else
			//{
			//	await this.Provider.Terminal.SendAsync("interface vlan " + vlanId);
			//}

			//string response = await this.Provider.Terminal.SendAsync("name \"" + vlanName + "\"");

			//if (response.ToLower().Contains("wrong"))
   //         {
   //             response = await this.Provider.Terminal.SendAsync("name \"" + vlanName + "\"");

			//	if (!response.IsNullOrEmpty())
			//	{
			//		await this.Provider.Terminal.SendAsync("exit");
					
   //                 throw new ProviderInfoException("Set VLAN name has failed: " + response);
			//	}
   //         }

   //         await this.Provider.Terminal.SendAsync("exit");
   //     }
    }
}
