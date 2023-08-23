using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.C3Com)]
	public class NetworkDeviceProvider3Com 
    {
		public const string PrivilegeModeCommand = "system-view";
		public const string PrivilegeModePrompts = "]";
		public const string LogoutCommand = "quit";
	}
}
