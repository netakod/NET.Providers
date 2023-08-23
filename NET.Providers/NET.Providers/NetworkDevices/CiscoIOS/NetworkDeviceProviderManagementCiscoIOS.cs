using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.CiscoIOS)]
    public class NetworkDeviceProviderManagementCiscoIOS : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true;
        }

        public override async ValueTask WriteConfigToFlash()
        {
            string response = String.Empty;
            int timeout = this.Provider.Terminal.Timeout;

            try
            {
                           await this.Provider.Terminal.ExitConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("copy running-config startup-config", "?");

                this.Provider.Terminal.Timeout = 180;
                
                response = await this.Provider.Terminal.SendAsync("");
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

            if (!response.ToLower().Contains("ok"))
                throw new ProviderInfoException("Error writing config to flash: " + response);
        }

        public override async ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            string response = String.Empty;
            int timeout = this.Provider.Terminal.Timeout;

            try
            {
                await this.Provider.Terminal.ExitConfigModeAsync();

                if (protocol == ConfigBackupServerProtocol.TFTP)
                {
                    response = await this.Provider.Terminal.SendAsync("copy running-config tftp", "?");
                    response = await this.Provider.Terminal.SendAsync(server, "?");

                    this.Provider.Terminal.Timeout = 180;
                    response = await this.Provider.Terminal.SendAsync(configFileName);
                }
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

			if (protocol == ConfigBackupServerProtocol.TFTP)
			    if (!response.ToLower().Contains("bytes copied"))
                    throw new ProviderInfoException("Error writting config to TFTP server " + server + ": " + response);
        }
    }
}
