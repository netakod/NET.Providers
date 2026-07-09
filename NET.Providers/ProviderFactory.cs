using Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NET.Providers
{
	public class ProviderFactory
	{
		//private static ProviderDiscovery instance = null;
		private static object lockObject = new object();

		private static IEnumerable<Type> assemblyTypes;
		private static Dictionary<Type, Dictionary<DeviceProviderType, Type>> networkDeviceInheritedClassesByProviderTypeByConnectionControlType = new Dictionary<Type, Dictionary<DeviceProviderType, Type>>();
		//private Dictionary<int, IProviderModel> providerModelsByProviderModelType = new Dictionary<int, IProviderModel>();
		private static List<ProviderInfo> providerList = new List<ProviderInfo>();
		//private List<ProviderDiscoveryInfoNew> providerInterfaceList = new List<ProviderDiscoveryInfoNew>();
		private static List<ProviderModuleInfo> providerModuleList = new List<ProviderModuleInfo>();
		private static Dictionary<Type, Dictionary<ProviderGroup, Dictionary<int, Type>>> connectionControlTypesByDeviceTypeByProviderTypeByBaseControlType = new Dictionary<Type, Dictionary<ProviderGroup, Dictionary<int, Type>>>();
		private static Dictionary<Type, List<ProviderInfo>> connectionControlInfoListsByBaseControlType = new Dictionary<Type, List<ProviderInfo>>();

		//static ProviderDiscovery()
		//	: this(AppDomain.CurrentDomain.GetAssemblies())
		//{
		//}

		//public ProviderDiscovery(Assembly[] assemblies)
		//	: this(ReflectionHelper.CollectTypesInAssemblies(assemblies))
		//{
		//}

		static ProviderFactory()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			assemblyTypes = ReflectionHelper.SelectAssemblyTypes(assemblies);

			foreach (Type objectType in assemblyTypes)
			{
				if (objectType.IsAbstract)
					continue;

				if (typeof(Provider).IsAssignableFrom(objectType))
				{
					ProviderInfo providerInfo = CollectProviderInfo<ProviderInfo>(objectType);

					if (!providerInfo.ObjectType.IsSubclassOf(typeof(ClientProvider)))
						providerList.Add(providerInfo);
				}
				if (typeof(ProviderModule).IsAssignableFrom(objectType))
				{
					ProviderModuleInfo providerModuleInfo = CollectProviderInfo<ProviderModuleInfo>(objectType);

					FillProviderModuleInfo(objectType, ref providerModuleInfo);
					providerModuleList.Add(providerModuleInfo);
				}
			}
		}

		// TODO: Create Dictionary that caches object types by providerType
		public static T? CreateProvider<T>(ProviderGroup providerGroup, int providerType)
			where T : Provider
		{
			T? result = null;
			Type? objectType = providerList.Find(info => info.ProviderType == providerGroup && info.DeviceManagementType == providerType)?.ObjectType;

			if (objectType == null) // Try with Generic device type (0), if exists
				objectType = providerList.Find(info => info.ProviderType == providerGroup && info.DeviceManagementType == 0)?.ObjectType;

			if (objectType != null)
			{
				result = Activator.CreateInstance(objectType) as T;

				if (result != null)
				{
					result.ProviderGroup = providerGroup;
					result.ProviderType = providerType;
					//result.Initialize();
				}
			}

			return result;
		}

		public static ProviderModule? CreateProviderModule(Provider provider, int deviceManagementType, int moduleType)
		{
			ProviderModule? result = null;
			Type? objectType = providerModuleList.Find(info => info.ProviderType == provider.ProviderGroup && info.DeviceManagementType == deviceManagementType && info.ModuleType == moduleType)?.ObjectType;

			if (objectType == null) // Try with Generic device type (0), if exists
				objectType = providerModuleList.Find(info => info.ProviderType == provider.ProviderGroup && info.DeviceManagementType == 0 && info.ModuleType == moduleType)?.ObjectType;

			if (objectType != null)
			{
				result = Activator.CreateInstance(objectType) as ProviderModule;

				if (result != null)
				{
					result.ModuleType = moduleType;
					result.Provider = provider;
				}
			}

			return result;
		}

		// TODO: Create Dictionary that caches object types by providerType, deviceType
		public static T? CreateProviderModule<T>(Provider provider, int deviceManagementType, int moduleType)
			where T : ProviderModule
		{
			T? result = null;
			Type? objectType = providerModuleList.Find(info => info.ProviderType == provider.ProviderGroup && info.DeviceManagementType == deviceManagementType && info.ModuleType == moduleType)?.ObjectType;

			if (objectType == null) // Try with Generic device type (0), if exists
				objectType = providerModuleList.Find(info => info.ProviderType == provider.ProviderGroup && info.DeviceManagementType == 0 && info.ModuleType == moduleType)?.ObjectType;

			if (objectType != null)
			{
				result = Activator.CreateInstance(objectType) as T;

				if (result != null)
				{
					result.ModuleType = moduleType;
					result.Provider = provider;
				}
			}

			return result;
		}

		//public static T CreateNetworkDeviceConnectionControl<T>(DeviceManagementType deviceManagementType) where T : class
		//{
		//	return CreateProviderConnectionControl<T>(ProviderModelType.NetworkDevice, (int)deviceManagementType);
		//}

		public static T? CreateProviderClient<T>(ProviderGroup providerModel, int deviceManagementType) 
			where T : class
		{
			Type controlType = GetProviderClientType(typeof(T), providerModel, deviceManagementType);
			T? result = Activator.CreateInstance(controlType) as T;

			return result;
		}

		public static Type GetProviderClientType(Type baseControlType, ProviderGroup providerType, int deviceManagementType)
		{
			Type result = baseControlType;

			Dictionary<ProviderGroup, Dictionary<int, Type>>? connectionControlTypesByDeviceTypeByProviderType;

			if (!connectionControlTypesByDeviceTypeByProviderTypeByBaseControlType.TryGetValue(baseControlType, out connectionControlTypesByDeviceTypeByProviderType))
			{
				// Get the all inherited classes 
				IEnumerable<Type> inheritedClasses = ReflectionHelper.SelectAssemblySubTypesOf(assemblyTypes, baseControlType);

				// Create dictionary of inherited classes by provider type
				connectionControlTypesByDeviceTypeByProviderType = new Dictionary<ProviderGroup, Dictionary<int, Type>>();

				foreach (Type connectionControlType in inheritedClasses)
				{
					ProviderGroup itemProviderType = default(ProviderGroup);
					int itemDeviceType = default(int);

					foreach (ProviderTypeAttribute attribute in connectionControlType.GetCustomAttributes(typeof(ProviderTypeAttribute), true))
						itemProviderType = attribute.ProviderGroup;

					foreach (DeviceProviderTypeAttribute attribute in connectionControlType.GetCustomAttributes(typeof(DeviceProviderTypeAttribute), true))
						itemDeviceType = attribute.DeviceProviderType;

					Dictionary<int, Type>? connectionControlTypesByDeviceType;

					if (!connectionControlTypesByDeviceTypeByProviderType.TryGetValue(itemProviderType, out connectionControlTypesByDeviceType))
					{
						connectionControlTypesByDeviceType = new Dictionary<int, Type>();
						connectionControlTypesByDeviceTypeByProviderType[itemProviderType] = connectionControlTypesByDeviceType;
					}

					connectionControlTypesByDeviceType[itemDeviceType] = connectionControlType;

					if (itemProviderType == providerType && itemDeviceType == deviceManagementType)
						result = connectionControlType;
				}
			}
			else
			{
				Dictionary<int, Type>? connectionControlTypesByDeviceType;
				Type? type = null;

				if (connectionControlTypesByDeviceTypeByProviderType.TryGetValue(providerType, out connectionControlTypesByDeviceType))
					if (connectionControlTypesByDeviceType.TryGetValue(deviceManagementType, out type))
						result = type;
			}

			return result;
		}

		public static Type GetProviderClientType2(Type baseControlType, ProviderGroup providerType, int deviceManagementType)
		{
			List<ProviderInfo> connectionControlInfoList;

			if (!connectionControlInfoListsByBaseControlType.TryGetValue(baseControlType, out connectionControlInfoList))
			{
				// Get the all inherited classes 
				IEnumerable<Type> inheritedClasses = ReflectionHelper.SelectAssemblySubTypesOf(assemblyTypes, baseControlType);

				connectionControlInfoList = new List<ProviderInfo>(inheritedClasses.Count());

				foreach (Type connectionControlType in inheritedClasses)
				{
					ProviderGroup itemProviderType = default(ProviderGroup);
					int itemDeviceType = default(int);

					foreach (ProviderTypeAttribute attribute in connectionControlType.GetCustomAttributes(typeof(ProviderTypeAttribute), true))
						itemProviderType = attribute.ProviderGroup;

					foreach (DeviceProviderTypeAttribute attribute in connectionControlType.GetCustomAttributes(typeof(DeviceProviderTypeAttribute), true))
						itemDeviceType = attribute.DeviceProviderType;

					connectionControlInfoList.Add(new ProviderInfo(connectionControlType, itemProviderType, itemDeviceType));
				}

				connectionControlInfoListsByBaseControlType.Add(baseControlType, connectionControlInfoList);
			}

			ProviderInfo? connectionControlInfo = connectionControlInfoList.Find(item => item.ProviderType == providerType && item.DeviceManagementType == deviceManagementType);

			return (connectionControlInfo != null) ? connectionControlInfo.ObjectType : baseControlType;
		}


		public static NetworkDeviceClientProvider CreateNetworkDeviceClientProvider(INetworkDevice networkDevice)
		{
			return CreateNetworkDeviceClientProvider(networkDevice, setLogging: true);
		}

		public static NetworkDeviceClientProvider CreateNetworkDeviceClientProvider(INetworkDevice networkDevice, bool setLogging, string logFolder = "Logs")
		{
			NetworkDeviceProvider provider = ProviderFactory.CreateProvider<NetworkDeviceProvider>(ProviderGroup.NetworkDevice, (int)networkDevice.ProviderType)!;
			NetworkDeviceClientProvider clientProvider = new NetworkDeviceClientProvider(provider);

			UpdateNetworkDeviceClientProviderSettings(clientProvider, networkDevice);

			if (setLogging)
			{
				string logPath = String.Format("{0}\\{1}", System.IO.Directory.GetCurrentDirectory(), logFolder);

				if (!Directory.Exists(logPath))
					Directory.CreateDirectory(logPath);

				clientProvider.SetLogging(String.Format("{0}\\{1}.log", logPath, networkDevice.Hostname));
			}

			return clientProvider;
		}

		public static void UpdateNetworkDeviceClientProviderSettings(NetworkDeviceClientProvider clientProvider, INetworkDevice networkDevice)
		{
			clientProvider.UseSnmp = networkDevice.UseSnmp;

			if (clientProvider.UseSnmp)
				clientProvider.SetSnmpSettings(networkDevice);

			clientProvider.UseTerminal = networkDevice.UseTerminal;

			if (clientProvider.UseTerminal)
				clientProvider.SetTerminalSettings(networkDevice);

			clientProvider.UseWeb = networkDevice.UseWeb;

			if (clientProvider.UseWeb)
				clientProvider.SetWebSettings(networkDevice);
		}



		//public Type GetNetworkDeviceConnectionControl_OLD(NetworkDeviceType deviceType, Type baseControlType)
		//{
		//	Dictionary<NetworkDeviceType, Type> inheritedClassesByProviderType;

		//	if (!this.networkDeviceInheritedClassesByProviderTypeByConnectionControlType.TryGetValue(baseControlType, out inheritedClassesByProviderType))
		//	{
		//		// Get the all inherited classes 
		//		IEnumerable<Type> inheritedClasses = ReflectionHelper.SelectSubclassesOf(this.assemblyTypes, baseControlType);

		//		// Create dictionary of inherited classes by provider type
		//		inheritedClassesByProviderType = new Dictionary<NetworkDeviceType, Type>();

		//		foreach (Type connectionControlType in inheritedClasses)
		//			foreach (DeviceTypeAttribute attribute in connectionControlType.GetCustomAttributes(typeof(DeviceTypeAttribute), true))
		//				if (attribute.DeviceType == (int)deviceType)
		//					inheritedClassesByProviderType.Add(deviceType, connectionControlType);

		//		this.networkDeviceInheritedClassesByProviderTypeByConnectionControlType.Add(baseControlType, inheritedClassesByProviderType);
		//	}

		//	Type result;

		//	if (inheritedClassesByProviderType.TryGetValue(deviceType, out result))
		//		return result;

		//	return baseControlType;
		//}

		//public static ProviderDiscovery Instance
		//{
		//	get { return GetInstance<ProviderDiscovery>(); }
		//}

		//protected static T GetInstance<T>() where T : ProviderDiscovery, new()
		//{
		//	lock (lockObject)
		//	{
		//		if (instance == null)
		//			instance = new T();
		//	}

		//	return instance as T;
		//}

		private static T CollectProviderInfo<T>(Type objectType) where T : ProviderInfo, new()
		{
			T result = new T();
			result.ObjectType = objectType;

			foreach (ProviderTypeAttribute attribute in objectType.GetCustomAttributes(typeof(ProviderTypeAttribute), true))
				result.ProviderType = attribute.ProviderGroup;

			foreach (DeviceProviderTypeAttribute attribute in objectType.GetCustomAttributes(typeof(DeviceProviderTypeAttribute), true))
				result.DeviceManagementType = attribute.DeviceProviderType;

			return result;
		}

		private static void FillProviderModuleInfo(Type objectType, ref ProviderModuleInfo providerModuleInfo)
		{
			bool isSet = false;

			foreach (ProviderModuleTypeAttribute attribute in objectType.GetCustomAttributes(typeof(ProviderModuleTypeAttribute), true))
			{
				providerModuleInfo.ModuleType = attribute.ModuleType;
				isSet = true;
			}

			if (!isSet)
			{
				// Module type info is not set, try to find module type specified in the interface attributes.
				foreach (Type interfaceType in objectType.GetInterfaces())
					foreach (ProviderModuleTypeAttribute attribute in interfaceType.GetCustomAttributes(typeof(ProviderModuleTypeAttribute), true))
						providerModuleInfo.ModuleType = attribute.ModuleType;
			}
		}
	}


	public class ProviderModuleInfo : ProviderInfo
	{
		public int ModuleType { get; set; }
	}

	public class ProviderInfo
	{
		public ProviderInfo()
		{
			this.ObjectType = typeof(object);
		}

		public ProviderInfo(Type objectType, ProviderGroup providerType, int deviceManagementType)
		{
			this.ObjectType = objectType;
			this.ProviderType = providerType;
			this.DeviceManagementType = deviceManagementType;
		}

		public Type ObjectType { get; set; }
		public ProviderGroup ProviderType { get; set; }
		public int DeviceManagementType { get; set; }
	}

}
