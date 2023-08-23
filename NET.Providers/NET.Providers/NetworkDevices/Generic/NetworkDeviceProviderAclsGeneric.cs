using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using static System.Net.Mime.MediaTypeNames;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Generic)]
	public class NetworkDeviceProviderAclsGeneric : NetworkDeviceProviderAcls, INetworkDeviceProviderAcls
	{
		private string lastAclName = String.Empty;

		public override async ValueTask SetAclRule(string aclName, string command, AclInfo aclInfo)
		{
			string acl = command + " " + aclInfo.ToString(useSimbolsForPortAndCodes: false); // aclInfo.ToString(useSimbolsForPortAndCodes: false);
			//string[] aclCommands = acl.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (aclName != this.lastAclName)
			{
				await this.Provider.Terminal.EnterConfigModeAsync();
				await this.Provider.Terminal.SendAsync("ip access-list extended " + aclName);
			}

			this.lastAclName = aclName;

			if (aclInfo.ToString().StartsWith("ip any any"))
			{
				await this.Provider.Terminal.SendAsync("no " + acl); // first delete the existing one to set it to be the last line
				this.lastAclName = String.Empty;
			}

			await this.Provider.Terminal.SendAsync(acl); // Insted of sending whole acl line send sigle element chunks finishing with enter (\r\n)

			//for (int i = 0; i < aclCommands.Length; i++)
			//{
			//	string element = aclCommands[i]; // + ((i < aclCommands.Length - 1) ? " " : "\r\n");

			//	if (i < aclCommands.Length - 1)
			//		await this.Provider.Terminal.SendAsync(Encoding.UTF8.GetBytes(element + " ")); // this.Provider.Terminal.Encoding.GetBytes(text element + " ", sendCrLf: false);
			//	else
			//		await this.Provider.Terminal.SendAsync(element);
			//}

			if (this.lastAclName == String.Empty)
				await this.Provider.Terminal.SendAsync("exit");
		}

		public override async ValueTask RemoveAclRule(string aclName, string command, AclInfo aclInfo)
		{
            string acl = command + " " + aclInfo.ToString(useSimbolsForPortAndCodes: false); // aclInfo.ToString(useSimbolsForPortAndCodes: false);

			if (aclName != this.lastAclName)
			{
				await this.Provider.Terminal.EnterConfigModeAsync();
				await this.Provider.Terminal.SendAsync("ip access-list extended " + aclName);
			}

			this.lastAclName = aclName;
			await this.Provider.Terminal.SendAsync("no " + acl);
		}

		public override async ValueTask RemoveAcl(string aclName)
		{
			await this.Provider.Terminal.EnterConfigModeAsync();
			await this.Provider.Terminal.SendAsync("no ip access-list extended " + aclName);
		}
	}
}
