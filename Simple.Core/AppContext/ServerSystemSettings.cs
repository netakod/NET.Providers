using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.AppContext
{
    public class ServerSystemSettings : SystemSettings
	{
		//public DatastoreProviderType DefaultDatastoreType = DatastoreProviderType.SqlServer;
		public string DefaultDatastoreType = "SqlServer";
		public int DefaultSqlNetworkPort = 1433;
		public int DefaultServerPort = 5050;
		public int DefaultMonitorServerPort = 5051;
		public int DefaultMonitorStartDelay = 10;
		public int DefaultMonitorStartingInterval = 500;

		public const string SettingServerId = "ServerId";
		public const string SettingDatastoreType = "DatastoreType";
		public const string SettingDatastoreOfficeAccessFilePath = "DatastoreOfficeAccesFilePath";
		public const string SettingDatastoreSqlServer = "DatastoreSqlServer";
		public const string SettingDatastoreSqlDatabase = "DatastoreSqlDatabase";
		public const string SettingDatastoreSqlNetworkConnection = "DatastoreSqlNetworkConnection";
		public const string SettingDatastoreSqlNetworkPort = "DatastoreSqlNetworkPort";
		public const string SettingDatastoreSqlConnectByConnectionString = "DatastoreSqlConnectByConnectionString";
		public const string SettingDatastoreSqlConnectionString = "DatastoreSqlConnectionString";
		public const string SettingServerPort = "ServerPort";
		public const string SettingServerMonitorPort = "ServerMonitorPort";
		public const string SettingMonitorStartDelay = "MonitorStartDelay";
		public const string SettingMonitorStartingInterval = "MonitorStartingInterval";

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

		public string DatastoreType
		{
			get { return this.GetValue<string>(SettingDatastoreType, defaultValue: this.DefaultDatastoreType); }
			set { this.SetValue(SettingDatastoreType, (string)value, defaultValue: this.DefaultDatastoreType); }
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

		public virtual int ServerPort
		{
			get { return this.GetValue<int>(SettingServerPort, defaultValue: DefaultServerPort); }
			set { this.SetValue(SettingServerPort, value, defaultValue: DefaultServerPort); }
		}

		public virtual int MonitorServerPort
		{
			get { return this.GetValue<int>(SettingServerMonitorPort, defaultValue: DefaultMonitorServerPort); }
			set { this.SetValue(SettingServerMonitorPort, value, defaultValue: DefaultMonitorServerPort); }
		}

		/// <summary>
		/// Defasult monitoring start delay in seconds
		/// </summary>
		public virtual int MonitorStartDelay
		{
			get { return this.GetValue<int>(SettingMonitorStartDelay, defaultValue: DefaultMonitorStartDelay); }
			set { this.SetValue(SettingMonitorStartDelay, value, defaultValue: DefaultMonitorStartDelay); }
		}

		public virtual int MonitorStartingInterval
		{
			get { return this.GetValue<int>(SettingMonitorStartingInterval, defaultValue: DefaultMonitorStartingInterval); }
			set { this.SetValue(SettingMonitorStartingInterval, value, defaultValue: DefaultMonitorStartingInterval); }
		}

		//public string DatastoreXmlDataSourceFolder
		//{
		//	get { return this.GetValue<string>(SettingDatastoreXmlDataSourceFolder, this.DefaultDatastoreXmlDataSourceFolder); }
		//	set { this.SetValue(SettingDatastoreXmlDataSourceFolder, value); }
		//}
	}
}
