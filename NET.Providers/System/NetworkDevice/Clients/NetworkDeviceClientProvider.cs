using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Providers.Snmp;
using NET.Providers.Terminal;
using NET.Providers.Web;

namespace NET.Providers
{
	public class NetworkDeviceClientProvider : ClientProvider, IProviderConnection, IDisposable
	{
		private NetworkDeviceClientProviderSystem? system = null;
		private NetworkDeviceClientProviderManagement? management = null;
		private NetworkDeviceClientProviderVlans? vlans = null;
		private NetworkDeviceClientProviderInterfaces? interfaces = null;
		private NetworkDeviceClientProviderAcls? acls = null;
		private NetworkDeviceClientProviderSockets? sockets = null;

		public NetworkDeviceClientProvider(NetworkDeviceProvider provider)
		{
			this.Provider = provider;
		}

		private NetworkDeviceProvider Provider { get; set; }

		public new DeviceProviderType ProviderType => (DeviceProviderType)base.ProviderType;

		public bool UseSnmp
		{
			get { return this.Provider.UseSnmp; }
			set { this.Provider.UseSnmp = value; }
		}

		public SnmpClient Snmp => this.Provider.Snmp;

		public bool UseTerminal
		{
			get { return this.Provider.UseTerminal; }
			set { this.Provider.UseTerminal = value; }
		}

		public TerminalClient Terminal =>  this.Provider.Terminal;

		public bool UseWeb
		{
			get { return this.Provider.UseWeb; }
			set { this.Provider.UseWeb = value; }
		}

		public WebClient Web => this.Provider.Web;

		public NetworkDeviceClientProviderSystem System => this.system ??= new NetworkDeviceClientProviderSystem(this.Provider.System);

		public NetworkDeviceClientProviderManagement Management => this.management ??= new NetworkDeviceClientProviderManagement(this.Provider.Management);

		public NetworkDeviceClientProviderVlans Vlans => this.vlans ??= new NetworkDeviceClientProviderVlans(this.Provider.Vlans);

		public NetworkDeviceClientProviderInterfaces Interfaces => this.interfaces ??= new NetworkDeviceClientProviderInterfaces(this.Provider.Interfaces);

		public NetworkDeviceClientProviderAcls Acls => this.acls ??= new NetworkDeviceClientProviderAcls(this.Provider.Acls);

		public NetworkDeviceClientProviderSockets Sockets => this.sockets ??= new NetworkDeviceClientProviderSockets(this.Provider.Sockets);

		//public override bool IsConnected
		//{
		//	get { return this.Provider.IsConnected; }
		//}

		//public override IRequestResult Connect()
		//{
		//	return this.Provider.Connect();
		//}

		public override async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext) =>  await this.Provider.TestConnectionAsync(workerContext);

		public override async ValueTask CloseAsync() => await this.Provider.CloseAsync();

		public override async ValueTask FinishUpdateAsync() =>  await this.Provider.FinishUpdateAsync();

		public override void SetLogging(string logFileName) => this.Provider.SetLogging(logFileName);

		public override void Dispose() => this.Provider.Dispose();
	}
}
