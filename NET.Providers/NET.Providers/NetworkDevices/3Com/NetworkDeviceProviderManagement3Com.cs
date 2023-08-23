using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.C3Com)]
    public class NetworkDeviceProviderManagement3Com : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true;
        }

        public override async ValueTask WriteConfigToFlash()
        {
			int timeout = this.Provider.Terminal.Timeout;

			try
			{
                //this.Provider.DeviceConnection.Terminal.ExitConfigMode();
                await this.Provider.Terminal.SendAsync("save", "]");
				
				this.Provider.Terminal.Timeout = 100; // 100 seconds

				await this.Provider.Terminal.SendAsync("y", ":");
                await this.Provider.Terminal.SendAsync("", "]");
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
			finally
			{
				this.Provider.Terminal.Timeout = timeout;
			}
		}

		public override async ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
		{
			int timeout = this.Provider.Terminal.Timeout;

			try
			{
				if (protocol == ConfigBackupServerProtocol.TFTP)
				{
					//this.Provider.DeviceConnection.Terminal.ExitConfigMode();
					await this.Provider.Terminal.SendAsync("save " + configFileName, "]");

					this.Provider.Terminal.Timeout = 100; // 100 seconds

					string response = await this.Provider.Terminal.SendAsync("y", ":");

					if (response.Contains("overwrite"))
						await this.Provider.Terminal.SendAsync("y", ":");

					await this.Provider.Terminal.SendAsync("quit", ">"); // Quit from system-view
					await this.Provider.Terminal.SendAsync(String.Format("tftp {0} put flash:/{1}", server, configFileName), ">");
					await this.Provider.Terminal.SendAsync(this.Provider.Terminal.PrivilegeModeCommand, "]"); // return to system-view
				}
				else throw new ProviderInfoException("Not supported protocol: " + protocol);

            }
			catch (Exception ex)
			{
				throw new ProviderInfoException(ex.Message);
			}
			finally
			{
				this.Provider.Terminal.Timeout = timeout;
			}
		}
	}
}
