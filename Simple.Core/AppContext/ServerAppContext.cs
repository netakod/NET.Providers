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
//using System.Data.SqlClient;
//using Microsoft.Win32;
using Simple;
//using Simple.Datastore;

namespace Simple.AppContext
{
    public abstract class ServerAppContextBase : AppContextBase
	{
		//private static ServerAppContext instance = null;
		//private static object lockObjectInstance = new object();

		static ServerAppContextBase()
		{
			//Instance = new ServerAppContextBase();
		}

		public ServerAppContextBase()
        {
            this.AdminUsername = "System Admin";
            this.Version = Assembly.GetEntryAssembly().GetName().Version;
        }

		//public string DatastoreVersion { get; set; }
		public string EmptyDatastoreFileName { get; set; }
        public string AdminUsername { get; set; }

		public string DatastoreUsername { get; set; }
		public string DatastorePassword { get; set; }


		public ServerSystemSettings SystemSettings
		{
			get { return base.GetSystemSettings<ServerSystemSettings>(); }
		}


		//public static ServerAppContextBase Instance { get; protected set; }

		//public static ServerAppContext Instance
		//{
		//	get { return GetInstance<ServerAppContext>(); }
		//}

		//protected static T GetInstance<T>() where T : ServerAppContext, new()
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
