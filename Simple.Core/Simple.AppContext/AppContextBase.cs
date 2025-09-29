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
	//public class AppContext<TAppContext> : AppContext
	//	where TAppContext : AppContext
	//{
	//	public new static AppContext<TAppContext> Instance
	//	{
	//		get { return GetInstance<AppContext<TAppContext>>(); }
	//	}
	//}

	public abstract class AppContextBase : IAppContext //: ApplicationContext
	{
  //      private static AppContext instance = null;
  //      private static object lockObjectInstance = new object();

		private AppSettings systemSettings = null;
		private AppSettings userSettings = null;

		public AppContextBase()
        {
            //this.Version = Assembly.GetEntryAssembly().GetName().Version;
        }

        public string AppName { get; set; }
		public string AppDescription { get; set; }
		public string Copyright { get; set; }
        public Version Version { get; set; }

		//public static AppContext Instance
		//{
		//    get { return GetInstance<AppContext>(); }
		//}

		protected TSystemSettings GetSystemSettings<TSystemSettings>() where TSystemSettings : AppSettings
		{
			if (this.systemSettings == null)
			{
				string filePath = String.Format("{0}\\{1}.config", System.IO.Directory.GetCurrentDirectory(), this.AppName);
				this.systemSettings = Activator.CreateInstance(typeof(TSystemSettings), filePath) as TSystemSettings;
			}

			return this.systemSettings as TSystemSettings;
		}

		protected TUserSettings GetUserSettings<TUserSettings>() where TUserSettings : AppSettings
		{
			if (this.userSettings == null)
			{
				string folderName = this.AppName;
				string fileName = this.AppName + ".user.config";
				string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				DirectoryInfo localAppFolder = new DirectoryInfo(appPath);
				string directoryPath = Path.Combine(localAppFolder.FullName, folderName);
				DirectoryInfo appFolder = (Directory.Exists(directoryPath)) ? new DirectoryInfo(directoryPath) : localAppFolder.CreateSubdirectory(folderName);
				string filePath = Path.Combine(appFolder.FullName, fileName);

				this.userSettings = Activator.CreateInstance(typeof(TUserSettings), filePath) as TUserSettings;
			}

			return this.userSettings as TUserSettings;
		}

		//public T ReadFromRegistry<T>(string key)
  //      {
  //          object value = this.ReadFromRegistry(key);
  //          return Conversion.TryChangeType<T>(value);
  //      }

  //      public T ReadFromRegistry<T>(string key, T defaultValue)
  //      {
  //          object value = this.ReadFromRegistryInternal(key, defaultValue);
  //          return Conversion.TryChangeType<T>(value, defaultValue);
  //      }

        //public object ReadFromRegistry(string key)
        //{
        //    return this.ReadFromRegistry(key, null);
        //}

        //public object ReadFromRegistry(string key, object defaultValue)
        //{
        //    return this.ReadFromRegistryInternal(key, defaultValue);
        //}

        //public void WriteToRegistry(string key, object value)
        //{
        //    try
        //    {
        //        RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(String.Format(@"SOFTWARE\{0}", this.AppName));

        //        if (registryKey != null)
        //        {
        //            registryKey.SetValue(key, value);
        //            registryKey.Close();
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //protected static T GetInstance<T>() where T : AppContext, new()
        //{
        //    lock (lockObjectInstance)
        //    {
        //        if (instance == null)
        //        {
        //            instance = new T();
        //        }
        //    }

        //    return instance as T;
        //}

        //protected bool IsInstanceCreated()
        //{
        //    return instance == null;
        //}

        //protected virtual AppSettings CreateSystemSettings(string filePath)
        //{
        //    return new SystemAppSettings(this, filePath);
        //}


        //private object ReadFromRegistryInternal(string key, object defaultValue)
        //{
        //    object result = defaultValue;

        //    try
        //    {
        //        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(String.Format(@"SOFTWARE\{0}", this.AppName));

        //        if (registryKey != null)
        //        {
        //            result = registryKey.GetValue(key);
        //            registryKey.Close();
        //        }
        //    }
        //    catch
        //    {
        //        result = defaultValue;
        //    }

        //    return result;
        //}
    }
}
