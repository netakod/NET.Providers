using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(Providers.DeviceProviderType.MikroTikRouterOS)]
	public class NetworkDeviceProviderMikroTikRouterOS : NetworkDeviceProvider 
    {
		public const string PrivilegeModePrompts = ">";
		public const string LogoutCommand = "quit";
		public const string MorePrompts = "-- [Q quit||D dump||down]";
	}
}
