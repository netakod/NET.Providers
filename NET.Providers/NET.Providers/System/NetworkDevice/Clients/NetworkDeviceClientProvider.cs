using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Snmp;
using NET.Tools.Terminal;
using NET.Tools.Web;

namespace NET.Tools.Providers
{
	//[ProviderGroup(ProviderType.NetworkDevice)]

	public class NetworkDeviceClientProvider : Provider, IProviderConnection, IDisposable
	{
		private NetworkDeviceClientProviderSystem system = null;
		private NetworkDeviceClientProviderManagement management = null;
		private NetworkDeviceClientProviderVlans vlans = null;
		private NetworkDeviceClientProviderInterfaces interfaces = null;
		private NetworkDeviceClientProviderAcls acls = null;
		private NetworkDeviceClientProviderSockets sockets = null;

		public NetworkDeviceClientProvider(NetworkDeviceProvider provider)
		{
			this.Provider = provider;
		}

		private NetworkDeviceProvider Provider { get; set; }

		public new DeviceProviderType DeviceManagementType
		{
			get { return (DeviceProviderType)base.DeviceManagementType; }
		}

		public bool UseSnmp
		{
			get { return this.Provider.UseSnmp; }
			set { this.Provider.UseSnmp = value; }
		}

		public SnmpClient Snmp
		{
			get { return this.Provider.Snmp; }
		}

		public bool UseTerminal
		{
			get { return this.Provider.UseTerminal; }
			set { this.Provider.UseTerminal = value; }
		}

		public TerminalClient Terminal
		{
			get { return this.Provider.Terminal; }
		}

		public bool UseWeb
		{
			get { return this.Provider.UseWeb; }
			set { this.Provider.UseWeb = value; }
		}

		public WebClient Web
		{
			get { return this.Provider.Web; }
		}

		public NetworkDeviceClientProviderSystem System
		{
			get
			{
				if (this.system == null)
					this.system = new NetworkDeviceClientProviderSystem(this.Provider.System);

				return this.system;
			}
		}

		public NetworkDeviceClientProviderManagement Management
		{
			get
			{
				if (this.management == null)
					this.management = new NetworkDeviceClientProviderManagement(this.Provider.Management);

				return this.management;
			}
		}

		public NetworkDeviceClientProviderVlans Vlans
		{
			get
			{
				if (this.vlans == null)
					this.vlans = new NetworkDeviceClientProviderVlans(this.Provider.Vlans);

				return this.vlans;
			}
		}

		public NetworkDeviceClientProviderInterfaces Interfaces
		{
			get
			{
				if (this.interfaces == null)
					this.interfaces = new NetworkDeviceClientProviderInterfaces(this.Provider.Interfaces);

				return this.interfaces;
			}
		}

		public NetworkDeviceClientProviderAcls Acls
		{
			get
			{
				if (this.acls == null)
					this.acls = new NetworkDeviceClientProviderAcls(this.Provider.Acls);

				return this.acls;
			}
		}

		public NetworkDeviceClientProviderSockets Sockets
		{
			get
			{
				if (this.sockets == null)
					this.sockets = new NetworkDeviceClientProviderSockets(this.Provider.Sockets);

				return this.sockets;
			}
		}

		//public override bool IsConnected
		//{
		//	get { return this.Provider.IsConnected; }
		//}

		//public override IRequestResult Connect()
		//{
		//	return this.Provider.Connect();
		//}

		public override async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
		{
			return await this.Provider.TestConnectionAsync(workerContext);
		}

		public override async ValueTask CloseAsync()
		{
			await this.Provider.CloseAsync();
		}

		public override async ValueTask FinishUpdateAsync()
		{
			await this.Provider.FinishUpdateAsync();
		}

		public override void SetLogging(string logFileName)
		{
			this.Provider.SetLogging(logFileName);
		}

		public override void Dispose()
		{
			this.Provider.Dispose();
		}
	}
}
