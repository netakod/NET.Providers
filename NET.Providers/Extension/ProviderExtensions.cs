using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Providers
{
	public static class ProviderExtensions
	{
		public static void SetSnmpSettings(this NetworkDeviceClientProvider provider, INetworkDevice networkDevice)
		{
			provider.Snmp.Community = networkDevice.SnmpCommunity;
			provider.Snmp.RemoteHost = networkDevice.Hostname;
			provider.Snmp.RemotePort = networkDevice.SnmpRemotePort;
			provider.Snmp.SnmpVersion = networkDevice.SnmpVersion;
			provider.Snmp.AuthenticationProtocol = networkDevice.SnmpAuthenticationProtocol;
			provider.Snmp.AuthenticationPassword = networkDevice.SnmpAuthenticationPassword;
			provider.Snmp.EncryptionPassword = networkDevice.SnmpEncryptionPassword;
			provider.Snmp.RemoteEngineBoots = networkDevice.SnmpRemoteEngineBoots;
			provider.Snmp.RemoteEngineTime = networkDevice.SnmpRemoteEngineTime;
			provider.Snmp.Timeout = networkDevice.SnmpTimeout;
			provider.Snmp.NumOfRetries = networkDevice.SnmpNumOfRetries;
			provider.Snmp.MaximumBulkRepetitions = networkDevice.SnmpMaximumBulkRepetitions;
			provider.Snmp.Timeout = networkDevice.MonitorTimeout;
			provider.Snmp.NumOfRetries = networkDevice.MonitorNumOfRetries;
		}

		//public static void SetSnmpSettingsForMonitor(this NetworkDeviceClientProvider provider, INetworkDevice networkDevice)
		//{
		//	provider.SetSnmpSettings(networkDevice);
		//	provider.SnmpConnection.Timeout = networkDevice.MonitorTimeout;
		//	provider.SnmpConnection.NumOfRetries = networkDevice.MonitorNumOfRetries;
		//}

		public static void SetTerminalSettings(this NetworkDeviceClientProvider provider, INetworkDevice networkDevice)
		{
			provider.Terminal.TerminalProtocol = networkDevice.TerminalProtocol;
			provider.Terminal.RemoteHost = networkDevice.Hostname;
			provider.Terminal.RemotePort = networkDevice.TerminalRemotePort;
			provider.Terminal.Username = networkDevice.TerminalUsername;
			provider.Terminal.Password = networkDevice.TerminalPassword;
			provider.Terminal.EnableSecret = networkDevice.TerminalEnableSecret;
			provider.Terminal.Timeout = networkDevice.TerminalTimeout;
			provider.Terminal.SendDelay = networkDevice.TerminalSendingInterval;
			provider.Terminal.PromptSeparator = networkDevice.TerminalPromptSeparator;
			provider.Terminal.UsernamePrompts = networkDevice.TerminalUsernamePrompts;
			provider.Terminal.PasswordPrompts = networkDevice.TerminalPasswordPrompts;
			provider.Terminal.EnableSecretPrompts = networkDevice.TerminalEnableSecretPrompts;
			provider.Terminal.NonPrivilegeModePrompts = networkDevice.TerminalNonPrivilegeModePrompts;
			provider.Terminal.MorePrompts = networkDevice.TerminalMorePrompts;
			provider.Terminal.PrivilegeModeCommand = networkDevice.TerminalPrivilegeModeCommand;
			provider.Terminal.PrivilegeModePrompts = networkDevice.TerminalPrivilegeModePrompts;
			provider.Terminal.ConfigModeCommand = networkDevice.TerminalConfigModeCommand;
			provider.Terminal.ExitConfigModeCommand = networkDevice.TerminalExitConfigModeCommand;
			provider.Terminal.VlanDatabaseConfigCommand = networkDevice.TerminalVlanDatabaseConfigCommand;
			provider.Terminal.ExitVlanDatabaseConfigCommand = networkDevice.TerminalExitVlanDatabaseConfigCommand;
			provider.Terminal.InterfaceConfigCommand = networkDevice.TerminalInterfaceConfigCommand;
			provider.Terminal.ExitInterfaceConfigCommand = networkDevice.TerminalExitInterfaceConfigCommand;
			provider.Terminal.MatchCase = networkDevice.TerminalMatchCase;
		}

		public static void SetWebSettings(this NetworkDeviceClientProvider provider, INetworkDevice networkDevice)
		{
			provider.Web.WebProtocol = networkDevice.WebProtocol;
			provider.Web.RemoteHost = networkDevice.Hostname;
			provider.Web.RemotePort = networkDevice.WebRemotePort;
			provider.Web.Username = networkDevice.WebUsername;
			provider.Web.Password = networkDevice.WebPassword;
			provider.Web.Timeout = networkDevice.WebTimeout;
			provider.Web.SendingInterval = networkDevice.WebSendingInterval;
			provider.Web.UseProxy = networkDevice.WebUseProxy;
			provider.Web.Proxy = networkDevice.WebProxy;
			provider.Web.ProxyPort = networkDevice.WebProxyPort;
			provider.Web.ProxyUsername = networkDevice.WebProxyUsername;
			provider.Web.ProxyPassword = networkDevice.WebProxyPassword;
		}
	}
}