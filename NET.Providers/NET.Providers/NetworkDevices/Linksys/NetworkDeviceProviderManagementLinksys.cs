using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Linksys)]
    public class NetworkDeviceProviderManagementLinksys : NetworkDeviceProviderManagement, INetworkDeviceProviderManagement
    {
		public override bool IsWriteConfigSupported()
        {
            return true;
        }

        public override ValueTask WriteConfigToFlash()
        {
            return new ValueTask(); // No writing memory command - every command is immediately saved into flash on Linksys
        }

        public override async ValueTask WriteConfigToServer(string server, ConfigBackupServerProtocol protocol, string configFileName)
        {
            if (protocol == ConfigBackupServerProtocol.TFTP)
            {
                if (this.Provider.UseWeb && !this.Provider.UseTerminal)
                {
                    string postData = @"rlCopyFreeHistoryIndex=&rlCopyFreeHistoryIndex%24scalar=8&rlcopyTableVT=OK&rlCopyTable%24VT=OK&rlCopyIndex%24VT=Type%3D0%3BAccess%3D1%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D&rlCopySourceFileType%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D10%3BRange0%3D%5B1%2C10%5D%3BDefault+value%3D6&rlCopySourceLocation%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D%3BDefault+value%3D1&rlCopyDestinationIpAddress%24VT=Type%3D5%3BAccess%3D2%3BNumOfEnumerations%3D0%3BDefault+value%3D0.0.0.0&rlCopyDestinationFileName%24VT=Type%3D100%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B0%2C160%5D%3BDefault+value%3Drout.cnf&rlCopyDestinationLocation%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D%3BDefault+value%3D3&rlCopyHistoryIndex%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D0%3BRange0%3D%5B-2147483648%2C2147483647%5D%3BDefault+value%3D0&rlCopyRowStatus%24VT=Type%3D0%3BAccess%3D2%3BNumOfEnumerations%3D6%3BRange0%3D%5B1%2C6%5D&rlcopyTableVT%24endVT=OK&rlCopyIndex%24Add=1&rlCopySourceFileType%24Add=6&rlCopySourceLocation%24Add=1&" +
                                       @"rlCopyDestinationIpAddress%24Add=" + server + @"&rlCopyDestinationFileName%24Add=" + configFileName + @"&rlCopyDestinationLocation%24Add=3&rlCopyHistoryIndex%24Add=8&rlCopyRowStatus%24Add=4&rlcopyTableVT%24endAdd=OK";
                    try
                    {
                        var request = await this.Provider.Web.SendPostRequestAsync("admin/tftp_cfg_ul.htm", "admin/tftp_cfg_ul.htm", postData);
                        string webResponseText = await request.GetResponseTextAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new ProviderInfoException("Error writting config to TFTP Server: " + ex.Message);
                    }
                }
                else
                {
                    string response = String.Empty;
                    int timeout = this.Provider.Terminal.Timeout;

                    try
                    {
                        await this.Provider.Terminal.ExitConfigModeAsync();
                        this.Provider.Terminal.Timeout = 15;

                        response = await this.Provider.Terminal.SendAsync(String.Format("copy startup-config tftp://{0}/{1}", server, configFileName), "bytes copied", "has failed");
                    }
                    catch (Exception ex)
                    {
                        throw new ProviderInfoException(ex.Message);
                    }
                    finally
                    {
                        this.Provider.Terminal.Timeout = timeout;
                    }

                    if (!response.ToLower().Contains("bytes copied"))
                        throw new ProviderInfoException("Error writting config to TFTP sertver " + server + ": " + response);
                }
            }
        }
    }
}
