using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Dell)]
    public class NetworkDeviceProviderManagementDell : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true;
        }

        public override async ValueTask WriteConfigToFlash()
        {
			DellDeviceType dellDeviceType = DellDeviceType.GeneralDellDevice;
			string response = String.Empty;
            int timeout = this.Provider.Terminal.Timeout;

            try
            {
                dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

				           await this.Provider.Terminal.ExitConfigModeAsync();
                response = await this.Provider.Terminal.SendAsync("copy running-config startup-config", "?");

                this.Provider.Terminal.Timeout = 180;
                
                response = await this.Provider.Terminal.SendAsync("y", sendCrLf: false);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

            string successWord = (dellDeviceType == DellDeviceType.GeneralDellDevice) ? "completed successfully" : "configuration saved";

			if (!response.ToLower().Contains(successWord))
                throw new ProviderInfoException("Error writting config to flash: " + response);
        }

        public override async ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            string response = String.Empty;
            int timeout = this.Provider.Terminal.Timeout;
            DellDeviceType dellDeviceType = await (this.Provider.Terminal as TerminalClientDell).GetDellDeviceType();

            try
            {
                await this.Provider.Terminal.ExitConfigModeAsync();
                
                this.Provider.Terminal.Timeout = 180;

                if (protocol == ConfigBackupServerProtocol.TFTP)
                {
                    if (dellDeviceType == DellDeviceType.PowerConnect62xx)
                    {
                        response = await this.Provider.Terminal.SendAsync("copy running-config backup-config", "?");
                        response = await this.Provider.Terminal.SendAsync("y", sendCrLf: false);
                        response = await this.Provider.Terminal.SendAsync(String.Format("copy backup-config tftp://{0}/{1}", server, configFileName), "?");
                        response = await this.Provider.Terminal.SendAsync("y", sendCrLf: false);
                    }
                    else if (dellDeviceType == DellDeviceType.DellNetworkingNxxxx)
                    {
                        response = await this.Provider.Terminal.SendAsync(String.Format("copy running-config tftp://{0}/{1}", server, configFileName), "?");
                        response = await this.Provider.Terminal.SendAsync("y", sendCrLf: false);
                    }
                    else
                    {
                        response = await this.Provider.Terminal.SendAsync(String.Format("copy running-config tftp://{0}/{1}", server, configFileName));
                    }
                }
                else throw new ProviderInfoException("Protocol is not supported: " + protocol);

            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
            finally
            {
                this.Provider.Terminal.Timeout = timeout;
            }

            string successWord = (dellDeviceType == DellDeviceType.GeneralDellDevice) ? "bytes copied" : "completed successfully";
            
            if (!response.ToLower().Contains(successWord))
                throw new ProviderInfoException("Error writting config to TFTP sertver " + server + ": " + response);
        }
    }
}
