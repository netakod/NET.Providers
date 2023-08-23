using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simple.Network;
using NET.Tools.Terminal;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Linksys)]
    public class TerminalClientlLinksys : TerminalClient
    {
		protected override async ValueTask<bool> TryConnectAsync(IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
		{
            List<string> usernameAndPasswordPromptList = new List<string>();
            usernameAndPasswordPromptList.AddRange(this.UsernamePromptList);
            usernameAndPasswordPromptList.AddRange(this.PasswordPromptList);

            this.connectionState = TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt; // First regular Linksys terminal authorization
            await this.InitializeConnection();
            this.connectionState = TerminalConnectionState.Authenticated; // Manually handle Linksys specific authorization and menu

            bool isConnected = await base.TryConnectAsync(remoteEndpoint, cancellationToken);

            if (isConnected)
			{
                bool oldLogging = this.logging;
                this.logging = false;
                string response = await this.SendAsync("", sendCrLf: false, waitFor: this.PasswordPromptList.ToArray());
                this.connectionLog = response;

                await Task.Delay(40);
                this.connectionLog += String.Format("\r\n{0} \t **********\r\n", this.Username);
                response = await this.SendAsync(this.Username + "\t" + this.Password, waitFor: "1."); // "1." The first menu item

                string ctrlZ = new string(new char[] { (char)26 });
                this.connectionLog += "\r\nCtrl+Z\r\n\r\n";

                await Task.Delay(100);
                response = await this.SendAsync(ctrlZ, sendCrLf: false, waitFor: ">"); // Send (char)26 is Ctrl+Z -> get hidden CLI
                this.connectionLog += response;

                this.connectionState = TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt;
                await Task.Delay(100);
                await this.SendAsync("lcli");
                await Task.Delay(100);
                await this.SendAsync("\r\n");
                await Task.Delay(100);

                this.logging = oldLogging;

                //isAuthorized = await this.LogInAsync(cancellationToken);

                //this.AutoLogin = false; 
            }

            return isConnected;
        }

		public override async ValueTask CloseAsync(CloseReason closeReason)
		{
            if (this.IsConnected)
            {
                await this.ExitConfigModeAsync();
                await this.SendAsync("exit");
                await Task.Delay(100);
                await this.SendAsync("\r\n");
                await Task.Delay(440);
                await this.SendAsync("\r\n");
                await Task.Delay(340);

                //this.lastSendingCommand = "exit\r\n";
            }

            await base.CloseAsync(closeReason);
        }
    }
}
