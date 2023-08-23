using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Providers
{
	[ProviderType(ProviderGroup.NetworkDevice)]
	public abstract class NetworkDeviceProviderModule : ProviderModule
	{
		public new NetworkDeviceProvider Provider => base.Provider as NetworkDeviceProvider;

		public new NetworkDeviceModule ModuleType => (NetworkDeviceModule)base.ModuleType;
	}
}
