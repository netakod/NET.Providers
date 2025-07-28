using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NET.Tools.Telnet_DevelopOld;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.CiscoIOS)]
    public class NetworkDeviceProviderVlansCiscoIOS : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlanInfos()
        {
            const string strVlanIsl = "VLAN ISL";
            const string strName = "Name";
            List<VlanInfo> result = new List<VlanInfo>();
            VlanInfo providerVlanInfo = VlanInfo.Empty;
            await this.Provider.Terminal.ExitConfigModeAsync();
            
            string response = await this.Provider.Terminal.SendAsync("vlan database");

			if (!response.ToLower().Contains("invalid input"))
			{
				response = await this.Provider.Terminal.SendAsync("show");
				string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                int vlanId = 0;

				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i].Trim();

                    if (line.TrimStart().StartsWith(strVlanIsl))
                    {
                        string vlanIdString = line.Split(':').Last();
                        vlanId = Conversion.TryChangeType<int>(vlanIdString);
                    }

					if (line.TrimStart().StartsWith(strName))
					{
						string vlanName = line.Split(':').Last().Trim();

						providerVlanInfo = new VlanInfo(vlanId, vlanName);
						result.Add(providerVlanInfo);
					}

					//string vlanName = String.Empty;

					//	if (i + 1 < lines.Length)
					//	{
     //                       string vlanNameLine = lines[i + 1];

     //                       if (vlanNameLine.TrimStart().StartsWith(strName))
     //                           vlanName = vlanNameLine.Split(':').Last().Trim();
					//	}

					//	providerVlanInfo = new VlanInfo(vlanId, vlanName);
					//	result.Add(providerVlanInfo);
					//}
				}

				await this.Provider.Terminal.SendAsync("exit");
			}
			else // vlan database
			{
				List<int> vlanIds = new List<int>();
				response = await this.Provider.Terminal.SendAsync("show vlan");
				// sh vlan brief is for switches  and for switching modules on routers it is sh vlan-switch brief.
				string[][] vlanTable = ProviderHelper.GetTable(response, "----");

				foreach (string[] lineArray in vlanTable)
				{
					if (lineArray.Length >= 2)
					{
						int vlanId = Conversion.TryChangeType<int>(lineArray[0].Trim());

						if (vlanId > 0 && !vlanIds.Contains(vlanId))
						{
							string vlanName = lineArray[1].Trim();
							
							result.Add(new VlanInfo(vlanId, vlanName));
							vlanIds.Add(vlanId);
						}
					}
				}
			}

            return result;
        }

        public override async ValueTask Set(int vlanId, string name)
        {
                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("vlan database");

            if (response.ToLower().Contains("invalid input"))
            {
                // vlan database command is not available
                await this.Provider.Terminal.EnterConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("vlan " + vlanId);
				response = await this.Provider.Terminal.SendAsync("name " + name);

				if (response != null && response.Trim().Length > 0)
                    throw new ProviderInfoException(response);
                else
                    response = await this.Provider.Terminal.SendAsync("exit");
            }
            else
            {
				response = await this.Provider.Terminal.SendAsync(String.Format("vlan {0} name {1}", vlanId, name));
				response = await this.Provider.Terminal.SendAsync("apply");

                if (response.ToLower().Contains("abort"))
                {
                    // Too many vlans - User 'abort' command to exit
                    string failedReason = response;
                    
                    response = await this.Provider.Terminal.SendAsync("abort");
                    
                    throw new ProviderInfoException("Add VLAN has failed: " + failedReason);
                }
				else
                {
					if (response.ToLower().Contains("apply not allowed")) // Apply not allowed ewhen device is in CLIENT/SERVER mode -> set vtp transparent mode
					{
						response = await this.Provider.Terminal.SendAsync("vtp transparent");
						response = await this.Provider.Terminal.SendAsync("apply");
					}

					response = await this.Provider.Terminal.SendAsync("exit");

                    if (response.ToLower().Contains("abort"))
                    {
                        string failedReason = response;

                        await this.Provider.Terminal.SendAsync("abort");

                        throw new ProviderInfoException("Add VLAN has failed: " + failedReason);
                    }
                }
            }
        }

        public override async ValueTask Remove(int vlanId)
        {
                              await this.Provider.Terminal.ExitConfigModeAsync();
            string response = await this.Provider.Terminal.SendAsync("vlan database");

            if (response.ToLower().Contains("invalid input"))
            {
                // vlan database command is not available
                await this.Provider.Terminal.EnterConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("no vlan " + vlanId);

                if (response != null && response.Trim().Length > 0)
                    throw new ProviderInfoException(response);
            }
            else
            {
                await this.Provider.Terminal.SendAsync("no vlan " + vlanId);
                await this.Provider.Terminal.SendAsync("apply");
                await this.Provider.Terminal.SendAsync("exit");
            }
        }

        public override async ValueTask<string> GetName(int vlanId)
        {
            string name = string.Empty;
            const string strName = "Name";
            string response;

                       await this.Provider.Terminal.ExitConfigModeAsync();
                       await this.Provider.Terminal.SendAsync("vlan database");
            response = await this.Provider.Terminal.SendAsync("show current " + vlanId);

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

            await this.Provider.Terminal.SendAsync("exit");

            return name;
        }

        //public override async ValueTask SetName(int vlanId, string vlanName)
        //{
        //    string newVlanName = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_'); 
            
        //                      await this.Provider.Terminal.ExitConfigModeAsync();
        //    string response = await this.Provider.Terminal.SendAsync("vlan database");

        //    if (response.ToLower().Contains("invalid input"))
        //    {
        //        // vlan database command is not available
        //                   await this.Provider.Terminal.EnterConfigModeAsync();
        //        response = await this.Provider.Terminal.SendAsync("vlan " + vlanId);

        //        if (response != null && response.Trim().Length > 0)
        //        {
        //            throw new ProviderInfoException(response);
        //        }
        //        else
        //        {
        //            await this.Provider.Terminal.SendAsync("name " + vlanName);
        //            await this.Provider.Terminal.SendAsync("exit");
        //        }
        //    }
        //    else
        //    {
        //                   await this.Provider.Terminal.SendAsync("vlan " + vlanId + " name " + newVlanName);
        //        response = await this.Provider.Terminal.SendAsync("apply");

        //        if (response.ToLower().Contains("abort"))
        //        {
        //            // Too many vlans - User 'abort' command to exit
        //            string failedReason = response;
                    
        //            await this.Provider.Terminal.SendAsync("abort");
                    
        //            throw new ProviderInfoException("Set VLAN name has failed: " + failedReason);
        //        }
        //        else
        //        {
        //            response = await this.Provider.Terminal.SendAsync("exit");

        //            if (response.ToLower().Contains("abort"))
        //            {
        //                string failedReason = response;
                        
        //                await this.Provider.Terminal.SendAsync("abort");
                        
        //                throw new ProviderInfoException("Set VLAN name has failed: " + failedReason);
        //            }
        //        }
        //    }
        //}
    }
}
