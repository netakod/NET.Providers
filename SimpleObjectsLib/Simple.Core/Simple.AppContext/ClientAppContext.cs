using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
//using System.Management;
using System.Security.Cryptography;
//using Microsoft.Win32;
using Simple;

namespace Simple.AppContext
{
	public abstract class ClientAppContextBase : AppContextBase, IClientAppContext, IAppContext // where T : ClientAppContext<T>, new()
	{
		//private static ClientAppContext instance = null;
		//private static object lockObjectInstance = new object();

		//static ClientAppContext()
		//{
		//	if (Instance == null)
		//		Instance = CreateInstance( new T();
		//}


		public ClientAppContextBase()
        {
            this.SystemAdminUsername = "System Admin";
            this.Version = Assembly.GetEntryAssembly().GetName().Version;
        }

        public string SystemAdminUsername { get; set; }

        public UserSettings UserSettings
        {
            get { return base.GetUserSettings<UserSettings>(); }
        }

		//public static T Instance { get; private set; }
		//{
		//    get { return GetInstance<ClientAppContext>(); }
		//}

		//protected static T GetInstance<T>() where T : ClientAppContext, new()
		//{
		//	lock (lockObjectInstance)
		//	{
		//		if (instance == null)
		//			instance = new T();
		//	}

		//	return instance as T;
		//}

		//protected bool IsInstanceCreated()
		//{
		//	return instance == null;
		//}
	}
}
