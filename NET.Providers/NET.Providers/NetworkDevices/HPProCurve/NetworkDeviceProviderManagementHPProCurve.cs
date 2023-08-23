using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.HPProCurve)]
    public class NetworkDeviceProviderManagementHPProCurve : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
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
                this.Provider.Terminal.Timeout = 50;

				response = await this.Provider.Terminal.SendAsync("write memory");
            }
            catch (Exception ex)
            {
                this.Provider.Terminal.Timeout = timeout;
                
                throw new ProviderInfoException(ex.Message + ": " + response);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

        }

        public override async ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            string response = String.Empty;
            int timeout = this.Provider.Terminal.Timeout;

            try
            {
                await this.Provider.Terminal.ExitConfigModeAsync();
                this.Provider.Terminal.Timeout = 100;

                if (protocol == ConfigBackupServerProtocol.TFTP)
                    response = await this.Provider.Terminal.SendAsync("copy running-config tftp " + server + " " + configFileName);
                else
                    throw new ProviderInfoException("Protocol is not supported: " + protocol);

            }
            catch (Exception ex)
            {
                this.Provider.Terminal.Timeout = timeout;
                
                throw new ProviderInfoException(ex.Message + ": " + response);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

            if (response.ToLower().Contains("unreachable"))
                throw new ProviderInfoException("Error writting config to TFTP sertver " + server + ": " + response);
        }
    }
}
