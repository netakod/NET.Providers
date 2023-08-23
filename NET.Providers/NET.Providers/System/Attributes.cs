using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Providers
{
	#region |   Generic Provider Attributes   |

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class ProviderTypeAttribute : Attribute
	{
		public ProviderTypeAttribute(ProviderGroup providerType)
		{
			this.ProviderGroup = providerType;
		}

		public ProviderGroup ProviderGroup { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class DeviceProviderTypeAttribute : Attribute
	{
		public DeviceProviderTypeAttribute(int deviceProviderType)
		{
			this.DeviceProviderType = deviceProviderType;
		}

		public int DeviceProviderType { get; private set; }
	}

	#endregion |   Generic Provider Attributes   |

	#region |   NetworkDevice Specific Provider Attributes   |

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class ProviderModuleTypeAttribute : Attribute
	{
		public ProviderModuleTypeAttribute(int moduleType)
		{
			this.ModuleType = moduleType;
		}

		public int ModuleType { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class NetworkDeviceProviderTypeAttribute : DeviceProviderTypeAttribute
	{
		public NetworkDeviceProviderTypeAttribute(DeviceProviderType deviceManagementType)
			: base((int)deviceManagementType)
		{
		}

		public new DeviceProviderType DeviceProviderType
		{
			get { return (DeviceProviderType)base.DeviceProviderType; }
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class NetworkDeviceModuleTypeAttribute : ProviderModuleTypeAttribute
	{
		public NetworkDeviceModuleTypeAttribute(NetworkDeviceModule moduleType)
			: base((int)moduleType)
		{
		}

		public new NetworkDeviceModule ModuleType
		{
			get { return (NetworkDeviceModule)base.ModuleType; }
		}
	}

	#endregion |   NetworkDevice Specific Provider Attributes   |
}
