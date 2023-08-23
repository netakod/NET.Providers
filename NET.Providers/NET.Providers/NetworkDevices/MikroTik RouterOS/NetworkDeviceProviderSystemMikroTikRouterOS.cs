using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikRouterOS)]
    public class NetworkDeviceProviderSystemMikroTikRouterOS : NetworkDeviceProviderSystemGeneric, INetworkDeviceProviderSystem
    {
        public override async ValueTask SetName(string name)
        {
            string valueToSet = name.IsNullOrEmpty() ? " " : name.Trim();

            try
            {
                await this.Provider.Terminal.SendAsync("system identity set name=" + valueToSet);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
        }


   //     public override void SetLocation(string location)
   //     {
   //         string valueToSet = location.IsNullOrEmpty() ? " " : location.Trim();

   //         try
   //         {
			//	this.Provider.Terminal.Send("system identity set name=" + valueToSet);
			//}
			//catch (Exception ex)
   //         {
   //             throw new ProviderInfoException(ex.Message);
   //         }
   //     }


   //     public override void SetContact(string contact)
   //     {
   //         string valueToSet = contact.IsNullOrEmpty() ? " " : contact.Trim();

   //         try
   //         {
   //             this.Provider.Snmp.Set(String.Format("{0}.0", SnmpOIDs.System.sysContact), valueToSet);
   //         }
   //         catch (Exception ex)
   //         {
   //             throw new ProviderInfoException(ex.Message);
   //         }
   //     }
    }
}
