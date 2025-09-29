using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Simple.Network;

namespace Simple.AppContext
{
    public class ClientSystemSettings : SystemSettings
    {
		public const int DefaultServerPort = 2020;
		public const string SettingClientId = "ClientId";
		public const string SettingNetManagerServerHostName = "NetManagerServerHostName";
		public const string SettingNetManagerServerPort = "NetManagerServerPort";
		public const string SettingUseProxy = "SettingUseProxy";
		public const string SettingProxy = "SettingProxy";
		public const string SettingProxyType = "SettingProxyType";
		public const string SettingProxyUsername = "SettingProxyUsername";
		public const string SettingProxyPassword = "SettingProxyPassword";

		public ClientSystemSettings(string filePath)
            : base(filePath)
        {
        }

		//public new ClientAppContext AppContext
		//{
		//	get { return base.AppContext as ClientAppContext; }
		//}

		public int ClientId
		{
			get { return this.GetValue<int>(SettingClientId, 0); }
			set { this.SetValue(SettingClientId, value); }
		}

		public string ServerHostname
		{
			get { return this.GetValue<string>(SettingNetManagerServerHostName, defaultValue: IPAddress.Loopback.ToString()); }
			set { this.SetValue(SettingNetManagerServerHostName, value, defaultValue: IPAddress.Loopback.ToString()); }
		}

		public int ServerPort
		{
			get { return this.GetValue<int>(SettingNetManagerServerPort, defaultValue: DefaultServerPort); }
			set { this.SetValue(SettingNetManagerServerPort, value, defaultValue: DefaultServerPort); }
		}


		public bool UseProxy
		{
			get { return this.GetValue<bool>(SettingUseProxy, defaultValue: false); }
			set { this.SetValue(SettingUseProxy, value, defaultValue: false); }
		}

		public string Proxy
		{
			get { return this.GetValue<string>(SettingProxy, defaultValue: String.Empty); }
			set { this.SetValue(SettingProxy, value, defaultValue: String.Empty); }
		}

		public ProxyTypes ProxyType
		{
			get { return this.GetValue<ProxyTypes>(SettingProxyType, defaultValue: ProxyTypes.None); }
			set { this.SetValue(SettingProxyType, value, defaultValue: ProxyTypes.None); }
		}

		public string ProxyUsername
		{
			get { return this.GetValue<string>(SettingProxyUsername, defaultValue: String.Empty); }
			set { this.SetValue(SettingProxyUsername, value, defaultValue: String.Empty); }
		}

		public string ProxyPassword
		{
			get { return this.GetValue<string>(SettingProxyPassword, defaultValue: String.Empty); }
			set { this.SetValue(SettingProxyPassword, value, defaultValue: String.Empty); }
		}
	}
}
