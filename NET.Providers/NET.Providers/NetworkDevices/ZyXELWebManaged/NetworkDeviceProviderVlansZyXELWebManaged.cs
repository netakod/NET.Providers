using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Simple;
using HtmlAgilityPack;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.ZyXELWebManaged)]
    public class NetworkDeviceProviderVlansZyXELWebManaged : NetworkDeviceProviderVlansGeneric, INetworkDeviceProviderVlans
    {
		//private ZyXELWebManagerHelper zyXELWebManagerHelper = null;

		//private ZyXELWebManagerHelper ZyXELWebManagerHelper
		//{
		//    get
		//    {
		//        if (this.zyXELWebManagerHelper == null)
		//        {
		//            this.zyXELWebManagerHelper = new ZyXELWebManagerHelper();
		//            this.zyXELWebManagerHelper.RemoteHost = this.Provider.Connection.Web.RemoteHost;
		//        }

		//        return this.zyXELWebManagerHelper;
		//    }
		//}

		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            List<VlanInfo> result = new List<VlanInfo>();
            HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync("vlanlist.htm");
            string responseText = await response.GetResponseTextAsync();
            HtmlDocument responseHtml = responseText.ToHtmlDocument();

            foreach (HtmlNode table in responseHtml.DocumentNode.SelectNodes("//table"))
            {
                if (table.InnerText.TrimStart().StartsWith("VLAN ID"))
                {
                    string[] lines = table.InnerText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        string text = line.Trim();

                        if (text.Length == 0)
                            continue;

                        int vlanId =  Conversion.TryChangeType<int>(text);

                        if (vlanId > 0 && result.FirstOrDefault(item => item.VlanId == vlanId).Equals(default(VlanInfo)))
                            result.Add(new VlanInfo(vlanId, "Vlan" + vlanId));
                    }

                    break;
                }
            }

            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
            //// Create vlan and set all trunk ports to be fixed
            //IList<string> trunkPortInterfaceNames = (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXEL).TrunkPortInterfaceNames;

            //this.Provider.Connection.Terminal.EnterConfigMode();
            //string response = this.Provider.Connection.Terminal.Send("vlan " + vlanId);

            //foreach (string trunkInterfaceName in trunkPortInterfaceNames)
            //{
            //    int portNumber = (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXEL).GetPortNumberFromPortInterfaceName(trunkInterfaceName);
            //    response = this.Provider.Connection.Terminal.Send("fixed " + portNumber);
            //}

            //response = this.Provider.Connection.Terminal.Send("exit");

            // Create VLAN without changing port status
            var interfaces = await this.Provider.Interfaces.GetInterfaceNames();
            string interfaceVlanText = new String('0', interfaces.Count());
            HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync(String.Format("newvlan.cgi?newid={0}&values={1}", vlanId, interfaceVlanText));
            string responseText = await response.GetResponseTextAsync();

            // Set created vlan tagging for trunk ports
            string portValnTaggingInfo = await ((NetworkDeviceProviderInterfacesZyXELWebManaged)this.Provider.Interfaces).CreateVlanInterfaceTaggingInfo(vlanId);
            response = await this.Provider.Web.SendGetRequestAsync(String.Format("setvlan.cgi?vid={0}&values={1}", vlanId, portValnTaggingInfo));
            responseText = await response.GetResponseTextAsync();
        }

        public override async ValueTask Remove(int vlanId)
        {
            const string strChangePvidOfPort = ">Change PVID of port";
            bool isVlanRemoved = false;

            do
            {
                HttpWebResponse response = await this.Provider.Web.SendGetRequestAsync(String.Format("delvlan.cgi?id={0}", vlanId));
                string responseText = await response.GetResponseTextAsync();

                if (responseText.Replace(" ", "").ToLower().Contains("<inputtype=buttonvalue=\"retry\""))
                {
                    if (responseText.Contains(strChangePvidOfPort))
                    {
                        int startPos = responseText.IndexOf(strChangePvidOfPort);

                        string problemInterfaceNumberText = responseText.Substring(startPos + strChangePvidOfPort.Length, + 3);
                        int problemInterfaceNumber = Conversion.TryChangeType<int>(problemInterfaceNumberText);

						await (this.Provider.Interfaces as NetworkDeviceProviderInterfacesZyXELWebManaged).SetVlanId(problemInterfaceNumber.ToString(), 1);
                    }
                    else
                    {
                        throw new InvalidOperationException(responseText);
                    }
                }
                else
                {
                    isVlanRemoved = true;
                }
            }
            while (!isVlanRemoved);
        }

        public override ValueTask<string> GetName(int vlanId)
        {
            return new ValueTask<string>(String.Format("Vlan{0}", vlanId.ToString()));
        }

        public override ValueTask SetName(int vlanId, string vlanName)
        {
            // Vlan name cannot be set (no vlan naming on device).
            return new ValueTask();
        }
    }
}
