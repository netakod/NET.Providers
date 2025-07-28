using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using Simple;
using Simple.Modeling;

namespace NET.Tools.Providers
{
	public static class ProviderModel
	{
		public static readonly ProviderTypeModel ProviderTypeModel				  = new ProviderTypeModel();
		public static readonly ProviderManagementTypeModel NetworkDeviceTypeModel = new ProviderManagementTypeModel();
		public static readonly NetworkDeviceModuleModel NetworkDeviceModuleModel  = new NetworkDeviceModuleModel();

		static ProviderModel()
		{
			ProviderTypes		 = ModelHelper.CreateModelDictionaryByReflection<int, ModelElement>(ProviderTypeModel,	      typeModel => typeModel.Index, null);
			NetworkDeviceTypes	 = ModelHelper.CreateModelDictionaryByReflection<int, ModelElement>(NetworkDeviceTypeModel,   typeModel => typeModel.Index, null);
			NetworkDeviceModules = ModelHelper.CreateModelDictionaryByReflection<int, ModelElement>(NetworkDeviceModuleModel, typeModel => typeModel.Index, null);
		}

		public static ModelDictionary<int, ModelElement> ProviderTypes		  { get; private set; }
		public static ModelDictionary<int, ModelElement> NetworkDeviceTypes	  { get; private set; }
		public static ModelDictionary<int, ModelElement> NetworkDeviceModules { get; private set; }
	}

	public class ProviderTypeModel
	{
		public ModelElement NetworkDevice = new ModelElement() { Index = (int)ProviderGroup.NetworkDevice };
	}

	public class ProviderManagementTypeModel
	{
		// Note that the index numbers are very important to be unique. They are set in datastore and uniquely specify device type cannot be changed during app lifecycle
		public ModelElement Generic          = new ModelElement() { Index = (int)DeviceProviderType.Generic };
		public ModelElement AristaEOS		 = new ModelElement() { Index = (int)DeviceProviderType.AristaEOS };
		public ModelElement CiscoIOS         = new ModelElement() { Index = (int)DeviceProviderType.CiscoIOS };
		public ModelElement Dell             = new ModelElement() { Index = (int)DeviceProviderType.Dell };
		public ModelElement HPProCurve       = new ModelElement() { Index = (int)DeviceProviderType.HPProCurve,	    Caption = "HP ProCurve" };
		public ModelElement C3Com            = new ModelElement() { Index = (int)DeviceProviderType.C3Com,		    Name    = "3Com", Caption = "3Com" };
		public ModelElement ZyXEL            = new ModelElement() { Index = (int)DeviceProviderType.ZyXEL,			Caption = "ZyXEL" };
		public ModelElement ZyXELWebManaged  = new ModelElement() { Index = (int)DeviceProviderType.ZyXELWebManaged,  Caption = "ZyXEL Web Managed" };
		public ModelElement Linksys          = new ModelElement() { Index = (int)DeviceProviderType.Linksys };
		public ModelElement MikroTikSwOS     = new ModelElement() { Index = (int)DeviceProviderType.MikroTikSwOS,     Caption = "MikroTik SwOS" };
		public ModelElement MikroTikRouterOS = new ModelElement() { Index = (int)DeviceProviderType.MikroTikRouterOS, Caption = "MikroTik RouterOS" };
	}

	public class NetworkDeviceModuleModel
	{
		public ModelElement System	   = new ModelElement() { Index = (int)NetworkDeviceModule.System };
		public ModelElement Management = new ModelElement() { Index = (int)NetworkDeviceModule.Management };
		public ModelElement Interfaces = new ModelElement() { Index = (int)NetworkDeviceModule.Interfaces };
		public ModelElement Vlans	   = new ModelElement() { Index = (int)NetworkDeviceModule.Vlans };
		public ModelElement Sockets	   = new ModelElement() { Index = (int)NetworkDeviceModule.Sockets };
	}

	public enum ProviderGroup
	{
		[Description("Network Device")]
		NetworkDevice = 0
	}

	public enum DeviceProviderType
	{
		Generic = 0,

		[Description("Arista Networks EOS")]
		AristaEOS = 1,

		[Description("Cisco IOS")]
		CiscoIOS = 2,

		Dell = 3,

		[Description("HP ProCurve")]
		HPProCurve = 4,

		[Name("3Com")]
		[Description("3Com")]
		C3Com = 5,

		ZyXEL = 6,

		[Description("ZyXEL Web Managed")]
		ZyXELWebManaged = 7,

		Linksys = 8,

		[Description("MikroTik SwOS")]
		MikroTikSwOS = 9,

		[Description("MikroTik RouterOS")]
		MikroTikRouterOS = 10,
	}

	public enum NetworkDeviceModule
	{
		System,
		Management,
		Vlans,
		Interfaces,
		Acls,
		Sockets
	}
}
