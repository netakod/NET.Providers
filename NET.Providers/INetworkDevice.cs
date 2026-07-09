using NET.Providers.Snmp;
using NET.Providers.Terminal;
using NET.Providers.Web;
using Simple.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Providers
{
    public interface INetworkDevice
    {
		DeviceProviderType ProviderType { get; }
        string Hostname { get; }
        

		bool UseSnmp { get; }

		int SnmpRemotePort { get; }

		SnmpVersion SnmpVersion { get; }

		string SnmpCommunity { get; }

		string NewSnmpCommunity { get; }

		SnmpAuthenticationProtocol SnmpAuthenticationProtocol { get; }

		string SnmpAuthenticationPassword { get; }

		string SnmpUsername { get; }

		string SnmpPassword { get; }

		int SnmpEncryptionProtocol { get; }

		string SnmpEncryptionPassword { get; }

		int SnmpRemoteEngineBoots { get; }

		int SnmpRemoteEngineTime { get; }

		int SnmpTimeout { get; }

		int SnmpNumOfRetries { get; }

		int SnmpMaximumBulkRepetitions { get; }


		bool UseTerminal { get; }

		TerminalProtocol TerminalProtocol { get; }

		int TerminalRemotePort { get; }

		string TerminalUsername { get; }

		string TerminalPassword { get; }

		string TerminalEnableSecret	{ get; }

		int TerminalTimeout { get; }

		int TerminalSendingInterval { get; }

		string TerminalPromptSeparator { get; }

		string TerminalUsernamePrompts {  get; }

		string TerminalPasswordPrompts { get; }

		string TerminalEnableSecretPrompts { get; }

		string TerminalMorePrompts	{ get; }

		string TerminalNonPrivilegeModePrompts { get; }

		string TerminalPrivilegeModeCommand {  get; }

		string TerminalPrivilegeModePrompts { get; }

		string TerminalConfigModeCommand { get; }

		string TerminalExitConfigModeCommand { get; }

		string TerminalVlanDatabaseConfigCommand { get; }

		string TerminalExitVlanDatabaseConfigCommand { get; }

		string TerminalInterfaceConfigCommand { get; }

		string TerminalExitInterfaceConfigCommand { get; }

		bool TerminalMatchCase { get; }

		
		bool UseWeb { get; }

		WebProtocol WebProtocol { get; }

		int WebRemotePort { get; }

		string WebUsername { get; }

		string WebPassword { get; }

		int WebTimeout { get; }

		int WebSendingInterval { get; }

		bool WebUseProxy { get; }

		string WebProxy {  get; }

		int WebProxyPort { get; }

		string WebProxyUsername { get; }

		string WebProxyPassword { get; }


		int MonitorTimeout { get; }

		int MonitorNumOfRetries { get; }

		DeviceMonitoringMethod MonitoringMethod { get; }
		int MonitorPollingInterval { get; }

		bool IsDeleted { get; }
		int SystemStatus { get; set; }

		//void StatusIsChanged(int newStatus);

		void InterfacesStatusIsChanged(IDictionary<string, InterfaceOperationalStatus> newInterfacesStatusByInterfaceName);
		//void SetMultipleStatusChange(IDictionary<SimpleObject, int> newStatusesBySimpleObject);
	}
}
