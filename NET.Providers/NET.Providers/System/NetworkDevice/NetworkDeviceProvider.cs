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
	[ProviderType(ProviderGroup.NetworkDevice)]
	public class NetworkDeviceProvider : Provider, IProviderConnection, IDisposable
	{
		private SnmpClient snmp = null;
		private TerminalClient terminal = null;
		private WebClient web = null;

		private INetworkDeviceProviderSystem system = null;
		private INetworkDeviceProviderManagement management = null;
		private INetworkDeviceProviderVlans vlans = null;
		private INetworkDeviceProviderInterfaces interfaces = null;
		private INetworkDeviceProviderAcls acls = null;
		private INetworkDeviceProviderSockets sockets = null;

		public new DeviceProviderType DeviceManagementType
		{
			get { return (DeviceProviderType)base.DeviceManagementType; }
		}

		public bool UseSnmp { get; set; }
		public SnmpClient Snmp
		{
			get
			{
				if (this.snmp == null)
				{
					this.snmp = ProviderFactory.CreateProviderClient<SnmpClient>(ProviderGroup.NetworkDevice, (int)this.DeviceManagementType);
					this.snmp.Owner = this;
				}

				return this.snmp;
			}
		}

		public bool UseTerminal { get; set; }
		public TerminalClient Terminal
		{
			get
			{
				if (this.terminal == null)
				{
					this.terminal = ProviderFactory.CreateProviderClient<TerminalClient>(ProviderGroup.NetworkDevice, (int)this.DeviceManagementType);
					this.terminal.Owner = this;
				}

				return this.terminal;
			}
		}

		public bool UseWeb { get; set; }
		public WebClient Web
		{
			get
			{
				if (this.web == null)
				{
					this.web = ProviderFactory.CreateProviderClient<WebClient>(ProviderGroup.NetworkDevice, (int)this.DeviceManagementType);
					this.web.Owner = this;
				}

				return this.web;
			}
		}

		public INetworkDeviceProviderSystem System
		{
			get
			{
				if (this.system == null)
					this.system = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.System) as INetworkDeviceProviderSystem;

				return this.system;
			}
		}

		public INetworkDeviceProviderManagement Management
		{
			get
			{
				if (this.management == null)
					this.management = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.Management) as INetworkDeviceProviderManagement;

				return this.management;
			}
		}

		public INetworkDeviceProviderVlans Vlans
		{
			get
			{
				if (this.vlans == null)
					this.vlans = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.Vlans) as INetworkDeviceProviderVlans;

				return this.vlans;
			}
		}

		public INetworkDeviceProviderInterfaces Interfaces
		{
			get
			{
				if (this.interfaces == null)
					this.interfaces = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.Interfaces) as INetworkDeviceProviderInterfaces;

				return this.interfaces;
			}
		}

		public INetworkDeviceProviderAcls Acls
		{
			get
			{
				if (this.acls == null)
					this.acls = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.Acls) as INetworkDeviceProviderAcls;

				return this.acls;
			}
		}

		public INetworkDeviceProviderSockets Sockets
		{
			get
			{
				if (this.sockets == null)
					this.sockets = ProviderFactory.CreateProviderModule(this, base.DeviceManagementType, (int)NetworkDeviceModule.Sockets) as INetworkDeviceProviderSockets;

				return this.sockets;
			}
		}

		//public override IRequestResult Connect()
		//{
		//	return this.Connect(wc => this.Snmp.Connect(), wc => this.Terminal.Connect(), wc => this.Web.Connect(), workerContext: null);
		//}

		public override async ValueTask CloseAsync()
		{
			//if (this.snmp != null)
			//	this.snmp.CloseAsync();

			if (this.terminal != null)
				await this.terminal.CloseAsync(CloseReason.LocalClosing);

			if (this.web != null)
				await this.web.CloseAsync();
		}

		public override async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
		{
			return await this.ConnectAsync(this.Snmp.TestConnectionAsync, this.Terminal.TestConnectionAsync, this.Web.TestConnectionAsync, workerContext);
		}

		//public override bool IsConnected
		//{
		//	get { return (this.snmp != null && this.snmp.IsConnected) || (this.terminal != null && this.terminal.IsConnected) || (this.web != null && this.web.IsConnected); }
		//}

		public override void SetLogging(string logFileName)
		{
			if (this.UseSnmp)
				this.Snmp.SetLogging(logFileName);

			if (this.UseTerminal)
				this.Terminal.SetLogging(logFileName);

			if (this.UseWeb)
				this.Web.SetLogging(logFileName);
		}

		public override async ValueTask FinishUpdateAsync()
		{
			if (this.snmp != null)
				await this.snmp.FinishUpdateAsync();

			if (this.terminal != null)
				await this.terminal.FinishUpdateAsync();

			if (this.web != null)
				await this.web.FinishUpdateAsync();
		}

		public override void Dispose()
		{
			if (this.snmp != null)
				this.snmp.Dispose();

			if (this.terminal != null)
				this.terminal.Dispose();

			if (this.web != null)
				this.web.Dispose();

			if (this.system != null)
				this.system.Dispose();

			if (this.management != null)
				this.management.Dispose();

			if (this.interfaces != null)
				this.interfaces.Dispose();

			if (this.vlans != null)
				this.vlans.Dispose();

			if (this.sockets != null)
				this.sockets.Dispose();
		}

		//private IRequestResult ConnectOrTestConnection(Func<IRequestResult> snmpConnectMethod, Func<IRequestResult> terminalConnectMethod, Func<IRequestResult> webConnectMethod,
		//											   Func<WorkerContext, IRequestResult> snmpTestConnectionMethod, Func<WorkerContext, IRequestResult> terminalTestConnectionMethod,
		//											   Func<WorkerContext, IRequestResult> webTestConnectionMethod, WorkerContext workerContext, bool isTest)

		private async ValueTask<TaskInfo<string>> ConnectAsync(Func<WorkerContext, ValueTask<TaskInfo<string>>> snmpConnectAction, 
															   Func<WorkerContext, ValueTask<TaskInfo<string>>> terminalConnectAction, 
															   Func<WorkerContext, ValueTask<TaskInfo<string>>> webConnectAction, 
															   WorkerContext workerContext)
		{
			StringBuilder connectionMessage = new StringBuilder();
			string breakLine = "----------------------------------------------------------------";
			//bool breakLineRequired = false;
			bool snmpSecceeded = false;
			bool terminalSucceeded = false;
			bool webSucceeded = false;
			string systemDescription = String.Empty;
			//string systemName = null;

			if (this.UseSnmp)
			{
				if (connectionMessage.Length > 0)
					connectionMessage.AppendLine(breakLine);

				try
				{
					TaskInfo<string> snmpConnectionInfo = await snmpConnectAction(workerContext);

					//if (isTest)
					//{
					//	snmpConnectAction(workerContext);
					//	snmpConnectionInfo = workerContext.Result as IRequestResult;
					//}
					//else
					//{
					//	snmpConnectionInfo = snmpConnectMethod();
					//}

					if (!String.IsNullOrEmpty(snmpConnectionInfo.Message))
						connectionMessage.AppendLine(snmpConnectionInfo.Message);

					snmpSecceeded = snmpConnectionInfo.Succeeded; //this.Snmp.IsConnected;
																  //systemName = snmpConnectionInfo.SystemName;

					if (snmpSecceeded)
					{
						systemDescription = snmpConnectionInfo.ResultValue.ToString();
						connectionMessage.AppendLine(systemDescription);
					}

					//if (isTest)
					//	this.Snmp.Disconnect();
				}
				catch (Exception ex)
				{
					connectionMessage.AppendLine(String.Format("SNMP Connection Error: {0}", ex.Message));
				}
			}
			else
			{
				snmpSecceeded = true;
			}

			if (this.UseTerminal)
			{
				if (connectionMessage.Length > 0)
					connectionMessage.AppendLine(breakLine);

				try
				{
					TaskInfo<string> terminalConnectionInfo = await terminalConnectAction(workerContext);

					//if (isTest)
					//{
					//	terminalConnectAction(workerContext);
					//	terminalConnectionInfo = workerContext.Result as IRequestResult;
					//}
					//else
					//{
					//	terminalConnectionInfo = snmpConnectMethod();
					//}

					connectionMessage.AppendLine(terminalConnectionInfo.Message);
					terminalSucceeded = terminalConnectionInfo.Succeeded; //this.Terminal.IsConnected;

					//if (systemName.IsNullOrEmpty())
					//	systemName = terminalConnectionInfo.SystemName;

					//if (systemDescription.IsNullOrEmpty())
					//	systemDescription = terminalConnectionInfo.Message;


					//if (isTest)
					//	this.Terminal.Disconnect();
				}
				catch (Exception ex)
				{
					connectionMessage.AppendLine(String.Format("Terminal Connection Error: {0}", ex.Message));

				}
			}
			else
			{
				terminalSucceeded = true;
			}

			if (this.UseWeb)
			{
				if (connectionMessage.Length > 0)
					connectionMessage.AppendLine(breakLine);

				try
				{
					TaskInfo<string> webConnectionInfo = await webConnectAction(workerContext);

					//if (isTest)
					//{
					//	webConnectAction(workerContext);
					//	webConnectionInfo = workerContext.Result as IRequestResult;
					//}
					//else
					//{
					//	webConnectionInfo = snmpConnectMethod();
					//}

					connectionMessage.AppendLine(webConnectionInfo.Message);
					webSucceeded = webConnectionInfo.Succeeded; //this.Web.IsConnected;

					//if (systemName.IsNullOrEmpty())
					//	systemName = webConnectionInfo.SystemName;

					//if (systemDescription.IsNullOrEmpty())
					//	systemDescription = webConnectionInfo.Message;

					//if (isTest)
					//	this.Web.Disconnect();
				}
				catch (Exception ex)
				{
					connectionMessage.AppendLine(String.Format("Web Connection Error: {0}", ex.Message));
				}
			}
			else
			{
				webSucceeded = true;
			}

			if (connectionMessage.Length == 0)
				connectionMessage.AppendLine("No any Device Connection specified.");


			TaskResultInfo actionResult = (snmpSecceeded || terminalSucceeded || webSucceeded) ? TaskResultInfo.Succeeded : TaskResultInfo.Error;
			string message = connectionMessage.ToString();
			TaskInfo<string> result = new TaskInfo<string>(message, actionResult, systemDescription);

			if (workerContext != null)
				workerContext.Result = result;

			return result;
		}
	}

	//public struct InterfaceStatusInfo 
	//{
	//	public InterfaceStatusInfo(string interfaceName, InterfaceOperationalStatus operationalStatus)
	//	{
	//		this.InterfaceName = interfaceName;
	//		this.OperationalStatus = operationalStatus;
	//	}

	//	public string InterfaceName { get; set; }
	//	public InterfaceOperationalStatus OperationalStatus { get; set; }
	//}

	public struct TcpConnectionInfo
	{
		public TcpConnectionInfo(string localAddress, int localPort, string remoteAddress, int remotePort)
		{
			this.LocalAddress = localAddress;
			this.LocalPort = localPort;
			this.RemoteAddress = remoteAddress;
			this.RemotePort = remotePort;
		}

		public string LocalAddress { get; set; }
		public int LocalPort { get; set; }
		public string RemoteAddress { get; set; }
		public int RemotePort { get; set; }
	}

	public struct UdpListeningPortInfo
	{
		public UdpListeningPortInfo(string localAddress, int localPort)
		{
			this.LocalAddress = localAddress;
			this.LocalPort = localPort;
		}

		public string LocalAddress { get; set; }
		public int LocalPort { get; set; }
	}

	public struct VlanInfo : IEquatable<VlanInfo>
	{
		public static readonly VlanInfo Empty = new VlanInfo(default, default); 
		
		public VlanInfo(int vlanId, string vlanName)
		{
			this.VlanId = vlanId;
			this.VlanName = vlanName;
		}

		public int VlanId { get; set; }
		public string VlanName { get; set; }

		public override bool Equals(object obj) => (obj is VlanInfo vlanInfo) && this.Equals(vlanInfo);

		public bool Equals(VlanInfo other) => this.VlanId == other.VlanId && this.VlanName == other.VlanName;
	
		public override int GetHashCode() => base.GetHashCode();

		public static bool operator ==(VlanInfo left, VlanInfo right) => left.Equals(right);

		public static bool operator !=(VlanInfo left, VlanInfo right) => !left.Equals(right);
	}
}
