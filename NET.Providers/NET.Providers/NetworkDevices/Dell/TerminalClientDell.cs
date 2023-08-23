using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NET.Tools.Terminal;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Dell)]
    public class TerminalClientDell : TerminalClient
    {
        private DellDeviceType dellDeviceType;
        private bool isDellTypeDetermined = false;

        public async ValueTask<DellDeviceType> GetDellDeviceType()
        {
            if (!this.isDellTypeDetermined)
            {
                dellDeviceType = DellDeviceType.GeneralDellDevice;
                
                                  await this.ExitConfigModeAsync();
                string response = await this.SendAsync("show system");

				if (response.ToLower().Contains("machine type: powerconnect 62"))
				{
					dellDeviceType = DellDeviceType.PowerConnect62xx;
				}
				else if (response.ToLower().Contains("machine type: dell networking n"))
				{
					dellDeviceType = DellDeviceType.DellNetworkingNxxxx;
				}

				isDellTypeDetermined = true;
            }

            return this.dellDeviceType;
        }
        
        public override async ValueTask EnterConfigModeAsync()
        {
            if (await this.GetDellDeviceType() == DellDeviceType.PowerConnect62xx)
                this.ConfigModeCommand = "configure";

            await base.EnterConfigModeAsync();
        }
    }

    public enum DellDeviceType
    {
        GeneralDellDevice = 0,
        PowerConnect62xx = 1,
		DellNetworkingNxxxx = 2
    }
}
