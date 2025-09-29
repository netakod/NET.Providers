using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Simple.Datastore;

namespace Simple.AppContext
{
    public class ServerSystemSettings : SystemSettings
	{
		//public DatastoreProviderType DefaultDatastoreType = DatastoreProviderType.SqlServer;
		public int DefaultDatastoreType = 0;
		public int DefaultSqlNetworkPort = 1433;

		public const string SettingServerId = "ServerId";
		public const string SettingDatastoreType = "DatastoreType";
		public const string SettingDatastoreOfficeAccessFilePath = "DatastoreOfficeAccesFilePath";
		public const string SettingDatastoreSqlServer = "DatastoreSqlServer";
		public const string SettingDatastoreSqlDatabase = "DatastoreSqlDatabase";
		public const string SettingDatastoreSqlNetworkConnection = "DatastoreSqlNetworkConnection";
		public const string SettingDatastoreSqlNetworkPort = "DatastoreSqlNetworkPort";
		public const string SettingDatastoreSqlConnectByConnectionString = "DatastoreSqlConnectByConnectionString";
		public const string SettingDatastoreSqlConnectionString = "DatastoreSqlConnectionString";
		//public const string SettingDatastoreXmlDataSourceFolder = "SettingDatastoreXmlDataSourceFolder";

		public string DefaultDatastoreName { get; set; }
		public string DefaultOfficeAccessFilePath { get; set; }
		public string DefaultSqlServerInstanceName { get; set; } = "SQLEXPRESS";
		public string DefaultDatastoreXmlSourceFolder { get; set; }


		public ServerSystemSettings(string filePath)
            : base(filePath)
        {
        }

		//public new ServerAppContext AppContext
		//{
		//	get { return base.AppContext as ServerAppContext; }
		//}

		public int ServerId
		{
			get { return this.GetValue<int>(SettingServerId, defaultValue: 0); }
			set { this.SetValue(SettingServerId, value, defaultValue: 0); }
		}

		public int DatastoreType
		{
			get { return this.GetValue<int>(SettingDatastoreType, defaultValue: this.DefaultDatastoreType); }
			set { this.SetValue(SettingDatastoreType, (int)value, defaultValue: this.DefaultDatastoreType); }
		}

		public string DatastoreOfficeAccessFilePath
		{
			get { return this.GetValue<string>(SettingDatastoreOfficeAccessFilePath, defaultValue: this.DefaultOfficeAccessFilePath); }
			set { this.SetValue(SettingDatastoreOfficeAccessFilePath, value, defaultValue: this.DefaultOfficeAccessFilePath); }
		}

		public string DatastoreSqlServer
		{
			get { return this.GetValue<string>(SettingDatastoreSqlServer, defaultValue: ".\\" + this.DefaultSqlServerInstanceName); }
			set { this.SetValue(SettingDatastoreSqlServer, value, defaultValue: ".\\" + this.DefaultSqlServerInstanceName); }
		}

		public string DatastoreSqlDatabase
		{
			get { return this.GetValue<string>(SettingDatastoreSqlDatabase, defaultValue: this.DefaultDatastoreName); }
			set { this.SetValue(SettingDatastoreSqlDatabase, value, defaultValue: this.DefaultDatastoreName); }
		}

		public bool DatastoreSqlNetworkConnection
		{
			get { return this.GetValue<bool>(SettingDatastoreSqlNetworkConnection, defaultValue: false); }
			set { this.SetValue(SettingDatastoreSqlNetworkConnection, value, defaultValue: false); }
		}

		public int DatastoreSqlNetworkPort
		{
			get { return this.GetValue<int>(SettingDatastoreSqlNetworkPort, defaultValue: this.DefaultSqlNetworkPort); }
			set { this.SetValue(SettingDatastoreSqlNetworkPort, value, defaultValue: this.DefaultSqlNetworkPort); }
		}

		public bool DatastoreSqlConnectByConnectionString
		{
			get { return this.GetValue<bool>(SettingDatastoreSqlConnectByConnectionString, defaultValue: false); }
			set { this.SetValue(SettingDatastoreSqlConnectByConnectionString, value, defaultValue: false); }
		}

		public virtual string DatastoreSqlConnectionString
		{
			get { return this.GetValue<string>(SettingDatastoreSqlConnectionString); }
			set { this.SetValue(SettingDatastoreSqlConnectionString, value); }
		}

		//public string DatastoreXmlDataSourceFolder
		//{
		//	get { return this.GetValue<string>(SettingDatastoreXmlDataSourceFolder, this.DefaultDatastoreXmlDataSourceFolder); }
		//	set { this.SetValue(SettingDatastoreXmlDataSourceFolder, value); }
		//}
	}
}
