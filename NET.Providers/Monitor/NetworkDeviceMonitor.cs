//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Timers;
//using System.Net;
//using System.Net.NetworkInformation;
//using System.Net.Sockets;
//using System.Threading;
//using System.Threading.Tasks;
//using Simple;
//using Simple.Network;
//using NET.Providers;
//using NET.Providers.Snmp;

//namespace NET.Provider
//{
//	public class NetworkDeviceMonitor : IDisposable
//	{
//		private const DeviceMonitoringMethod defaultMonitoringMethod = DeviceMonitoringMethod.Ping;
//		private const int defaultPullingInterval = 120; // in seconds
//		private const int defaultTimeout = 4; // in seconds
//		private const int defaultNumberOfRetries = 4;
//		private const int defaultPauseBeforeSetUnknownStatusWhenStopping = 10;
//		//private int pullingInterval; // in seconds
//		//private int timeout; // in seconds
//		//private int numberOfRetries = 4;
//		//private int maximumBulkRepetitions = 20;
//		private int retryCount = 0;
//		private PingOptions pingOptions = new PingOptions(128, true);
//		private Ping ping = new Ping();
//		private  byte[] pingBuffer = new byte[32]; //32 byte buffer (create empty)
//												   //private SnmpControl snmpControl = new SnmpControl();
//		private NetworkDeviceClientProvider? provider = null;
//		private System.Timers.Timer pullingTimer = new System.Timers.Timer();
//		private object lockUnknownStatusesBySimpleObject = new object();
//		private object lockStatusesBySimpleObject = new object();
//		//private Thread setStatusThread = null;
//		private bool collectingDataIsInProgress = false;
//		//private bool startRequested = false;
//		private bool stopRequested = false;
//		//private bool deactivateProvider = false;

//		private static readonly Dictionary<string, InterfaceOperationalStatus> EmptyInterfacesOperationalStatu = new();

//		public NetworkDeviceMonitor(INetworkDevice networkDevice)
//		{
//			this.NetworkDevice = networkDevice;

//			//this.MonitoringMethod = defaultMonitoringMethod;
//			//this.pullingInterval = defaultPullingInterval;
//			//this.timeout = defaultTimeout;
//			//this.numberOfRetries = defaultNumberOfRetries;

//			//if (this.NetworkDevice != null)
//			//{
//			//	this.MonitoringMethod = this.NetworkDevice.MonitoringMethod;
//			//	this.PullingInterval = this.NetworkDevice.MonitorPoolingInterval;
//			//	this.Timeout = this.NetworkDevice.MonitorTimeout;
//			//	this.NumberOfRetries = this.NetworkDevice.MonitorNumOfRetries;
//			//	this.MaximumBulkRepetitions = this.NetworkDevice.SnmpMaximumBulkRepetitions;
//			//}

//			//this.ping.PingCompleted += new PingCompletedEventHandler(ping_PingCompleted);
//			//this.snmpManagerControl.OnResponse += new SnmpResponseEventHandler(snmpManagerControl_OnResponse);

//			this.pullingTimer.Elapsed += new ElapsedEventHandler(this.PullingTimer_Elapsed);
//			this.ping.PingCompleted += new PingCompletedEventHandler(this.Ping_PingCompleted);
//		}

//		public INetworkDevice NetworkDevice { get; private set; }

//		public NetworkDeviceClientProvider Provider => this.provider ??= ProviderFactory.CreateNetworkDeviceClientProvider(this.NetworkDevice);

//		public DeviceMonitoringMethod MonitoringMethod => this.NetworkDevice.MonitoringMethod;

//		/// <summary>
//		/// Pulling interval in seconds
//		/// </summary>
//		public int PollingInterval => this.NetworkDevice.MonitorPollingInterval;

//		/// <summary>
//		/// Timeout in seconds.
//		/// </summary>
//		public int Timeout => this.NetworkDevice.MonitorTimeout;

//		public int NumberOfRetries => this.NetworkDevice.MonitorNumOfRetries; 

//		public int MaximumBulkRepetitions => this.NetworkDevice.SnmpMaximumBulkRepetitions;

//		public async ValueTask<bool> StartAsync(bool delayStart = false)
//		{
//			if (delayStart)
//			{
//				this.pullingTimer.Interval = this.NetworkDevice.MonitorPollingInterval * 1000;
//				this.pullingTimer.Start();

//				return true; // return Task.FromResult(true);
//			}
//			else
//			{
//				return await this.CollectDataAsync();
//			}
//		}

//		//public void StartAsyncUsingNewThread()
//		//{
//		//	//this.StartAsyncInternal();

//		//	Thread setStatusThread = new Thread(async () => await this.CollectDataAsync())
//		//	{
//		//		IsBackground = true,
//		//		Priority = ThreadPriority.BelowNormal
//		//	};

//		//	setStatusThread.Start();

//		//	this.pullingTimer.Interval = this.NetworkDevice.MonitorPollingInterval * 1000;
//		//	this.pullingTimer.Start();
//		//}

//		public void Stop()
//		{
//			//int numOfRetryForWaitForMonitorToStop = 40;
//			this.stopRequested = true;
//			//this.startRequested = false;

//			this.pullingTimer.Stop();

//			//if (this.isMonitoringInProgress)
//			//{
//			//    while (this.isMonitoringInProgress)
//			//    {
//			//        Thread.Sleep(40);

//			//        if (numOfRetryForWaitForMonitorToStop-- == 0)
//			//            break;
//			//    }
//			//}

//			if (this.collectingDataIsInProgress)
//				Thread.Sleep(100);

//			this.collectingDataIsInProgress = false;
//			this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);

//			//Thread.Sleep(defaultPauseBeforeSetUnknownStatusWhenStopping);

//			//this.SetUnknownStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//		}

//		public Task<bool> StopAsync()
//		{
//			this.stopRequested = true;
//			//this.startRequested = false;

//			this.pullingTimer.Stop();

//			if (this.collectingDataIsInProgress)
//				Thread.Sleep(200);

//			this.collectingDataIsInProgress = false;
//			this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);

//			//if (!this.isMonitoringInProgress)
//			//    return;

//			//this.NetworkDevice.Status = (int)AlarmStatus.Unknown;

//			//Thread setStatusThread = new Thread(() =>
//			//{
//			//	//while (this.isMonitoringInProgress)
//			//	//{
//			//	//    Thread.Sleep(40);
//			//	//}

//			//	Thread.Sleep(50);

//			//	this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//			//});

//			//setStatusThread.IsBackground = false;
//			//setStatusThread.Priority = ThreadPriority.Normal;
//			//setStatusThread.Start();

//			//if (this.networkDeviceClientProvider != null)
//			//    this.networkDeviceClientProvider.Deactivate();

//			return Task.FromResult(true);
//		}

//		public void Dispose()
//		{
//			this.pullingTimer.Stop();
//			this.pullingTimer.Dispose();
//			//this.pullingTimer = null;

//			//this.provider.Disconnect();
//			this.provider?.Dispose();
//			this.provider = null;

//			//this.snmpControl.Disconnect();
//			//this.snmpControl.Dispose();
//			//this.snmpControl = null;

//			this.ping.Dispose();
//			//this.ping = null;

//			//this.pingOptions = null;
//		}

//		private async ValueTask<bool> CollectDataAsync()
//		{
//			if (this.collectingDataIsInProgress)
//				return false;

//			if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//				return false;

//			this.stopRequested = false;
//			this.collectingDataIsInProgress = true;
//			int deviceOldStatus = this.NetworkDevice.SystemStatus;
//			int deviceStatus = deviceOldStatus;
//			bool succedded = false;
//			int i = 0;

//			try
//			{
//				while (!succedded && ++i < this.NumberOfRetries)
//				{
//					if (this.NetworkDevice.Hostname == null || this.NetworkDevice.Hostname.Length <= 0)
//					{
//						this.NetworkDevice.SystemStatus = (int)SnmpAlarmStatus.Critical;

//						//break;
//					}
//					else if (this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Ping)
//					{
//						try
//						{
//							succedded = await this.PingAsync(this.NetworkDevice.Hostname);
//						}
//						catch
//						{
//							succedded = false;
//						}

//						//try
//						//{
//						//	this.ping.SendAsync(this.NetworkDevice.Hostname, this.Timeout, this.pingBuffer, this.pingOptions, userToken: null);
//						//}
//						//catch
//						//{
//						//	this.snmpManagerControl_OnAsyncResponse(succedded: false);
//						//	this.isMonitoringInProgress = false;
//						//}
//					}
//					else if (this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Provider)
//					{
//						if (this.Provider.UseSnmpConnection)
//							this.Provider.SetSnmpSettings(this.NetworkDevice);

//						if (this.Provider.UseTerminalConnection)
//							this.Provider.SetTerminalSettings(this.NetworkDevice);

//						if (this.Provider.UseWebConnection)
//							this.Provider.SetWebSettings(this.NetworkDevice);

//						try
//						{
//							var task = await this.Provider.System.GetName();

//							succedded = true; // task.IsCompleted;

//							//break;
//						}
//						catch
//						{
//							succedded = false;
//						}
//					}
//				}

//				if (this.stopRequested)
//					return false;

//				deviceStatus = (int)((succedded) ? SnmpAlarmStatus.Normal : SnmpAlarmStatus.Critical);

//				if (deviceStatus != deviceOldStatus)
//					this.NetworkDevice.StatusIsChanged(deviceStatus);

//				if (succedded || this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Ping)
//				{
//					var interfaceOperationalStatusResult = await this.GetInterfacesOperationalStatusAsync();

//					if (this.stopRequested)
//						return false;

//					if (interfaceOperationalStatusResult.Succeeded && interfaceOperationalStatusResult.ResultValue is Dictionary<string, InterfaceOperationalStatus> interfacesStatusByInterfaceName)
//						this.NetworkDevice.InterfacesStatusIsChanged(interfacesStatusByInterfaceName);
//				}
//			}
//			catch
//			{
//			}
//			finally
//			{
//				this.collectingDataIsInProgress = false;
//			}

//			return true;
//		}

//		//private async ValueTask StartInternalAsync2()
//		//{
//		//	if (this.isMonitoringInProgress)
//		//		return;

//		//	if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//		//		return ;

//		//	this.stopRequested = false;
//		//	this.isMonitoringInProgress = true;
//		//	int deviceOldStatus = this.NetworkDevice.Status;
//		//	int deviceStatus = deviceOldStatus;
//		//	bool succedded = false;
//		//	int i = 0;

//		//	try
//		//	{
//		//		while (!succedded && ++i < this.NumberOfRetries)
//		//		{
//		//			if (this.NetworkDevice.Hostname == null || this.NetworkDevice.Hostname.Length <= 0)
//		//			{
//		//				this.NetworkDevice.Status = (int)SnmpAlarmStatus.Critical;

//		//				//break;
//		//			}
//		//			else if (this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Ping)
//		//			{
//		//				try
//		//				{
//		//					var result = this.ping.Send(this.NetworkDevice.Hostname, this.Timeout * 1000);

//		//					succedded = result.Buffer.Length > 0;
//		//				}
//		//				catch (Exception ex)
//		//				{
//		//					succedded = false;
//		//				}
//		//			}
//		//			else if (this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Provider)
//		//			{
//		//				if (this.Provider.UseSnmp)
//		//				{
//		//					this.Provider.SetSnmpSettingsForMonitor(this.NetworkDevice);
//		//					//this.Provider.Snmp.Timeout = this.Timeout;
//		//					//this.Provider.Snmp.GetAsync(SnmpOIDs.System.sysName + ".0", null, this.snmpManagerControl_OnAsyncResponse);
//		//					//var result = this.Provider.TestConnection(new WorkerContext());
//		//				}

//		//				if (this.Provider.UseTerminal)
//		//					this.Provider.SetTerminalSettings(this.NetworkDevice);

//		//				if (this.Provider.UseWeb)
//		//					this.Provider.SetWebSettings(this.NetworkDevice);

//		//				try
//		//				{
//		//					var result = await this.Provider.System.GetName();

//		//					succedded = result.Succeeded; // task.IsCompleted;

//		//					//break;
//		//				}
//		//				catch
//		//				{
//		//					succedded = false;
//		//				}
//		//			}
//		//		}

//		//		if (this.stopRequested)
//		//			return;

//		//		deviceStatus = (int)((succedded) ? SnmpAlarmStatus.Normal : SnmpAlarmStatus.Critical);

//		//		if (deviceStatus != deviceOldStatus)
//		//			(this.NetworkDevice.Manager as ObjectManager).DeviceMonitorStatusIsChanged(this.NetworkDevice, deviceStatus);

//		//		if (succedded || this.NetworkDevice.MonitoringMethod == DeviceMonitoringMethod.Ping)
//		//		{
//		//			var interfaceOperationalStatusResult = await this.GetInterfacesOperationalStatusAsync();

//		//			if (this.stopRequested)
//		//				return;

//		//			if (interfaceOperationalStatusResult.Succeeded)
//		//			{
//		//				Dictionary<string, InterfaceOperationalStatus> interfacesStatusByInterfaceName = interfaceOperationalStatusResult.ResultValue;

//		//				(this.NetworkDevice.Manager as ObjectManager).DeviceMonitorInterfacesStatusIsChanged(this.NetworkDevice, interfacesStatusByInterfaceName);
//		//			}
//		//		}
//		//	}
//		//	catch
//		//	{
//		//	}
//		//	finally
//		//	{
//		//		this.isMonitoringInProgress = false;
//		//	}

//		//	return;
//		//}

//		/// <summary>
//		/// Send Async ping
//		/// </summary>
//		/// <param name="host">The host to ping.</param>
//		/// <param name="timeout">Timeout in milliseconds.</param>
//		/// <returns></returns>
//		private async ValueTask<bool> PingAsync(string host)
//		{
//			Exception? exception = null;

//			for (int i = 0; i < this.NumberOfRetries; i++)
//			{
//				try
//				{
//					var ping = new Ping();
//					var reply = await ping.SendPingAsync(host, this.Timeout * 1000, this.pingBuffer, this.pingOptions);

//					if (reply.Status == IPStatus.Success)
//						return true;
//				}
//				catch (Exception ex)
//				{
//					exception = ex;
//				}
//			}

//			if (exception != null)
//				throw exception;

//			return false;
//		}

//		private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
//		{
//			bool isAlive = false;

//			if (!this.collectingDataIsInProgress || this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//				return;

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;
				
//				return;
//			}

//			this.retryCount++;

//			//make sure we dont have a null reply
//			if (!(e.Reply == null))
//			{
//				switch (e.Reply.Status)
//				{
//					case IPStatus.Success:

//						isAlive = true; // returnMessage = string.Format("Reply from {0}: bytes={1} time={2}ms TTL={3}", pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl);

//						break;

//					case IPStatus.TimedOut:

//						//returnMessage = "Connection has timed out...";
//						break;

//					default:

//						//returnMessage = string.Format("Ping failed: {0}", pingReply.Status.ToString());
//						break;
//				}
//			}

//			if (isAlive)
//			{
//				var result = this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Normal);
//				this.collectingDataIsInProgress = false;
//			}
//			else if (this.retryCount < this.NumberOfRetries)
//			{
//				if (this.NetworkDevice.Hostname == null || this.NetworkDevice.Hostname.Trim().Length == 0)
//				{
//					var result = this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Critical);
					
//					this.collectingDataIsInProgress = false;
//				}
//				else
//				{
//					try
//					{
//						this.ping.SendAsync(this.NetworkDevice.Hostname, this.Timeout * 1000, this.pingBuffer, this.pingOptions, userToken: null);
//					}
//					catch
//					{
//						var result = this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Critical);
						
//						this.collectingDataIsInProgress = false;
//					}
//				}
//			}
//			else
//			{
//				var result = this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Critical);
				
//				this.collectingDataIsInProgress = false;
//			}

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;
//			}
//		}

//		private async Task SetDeviceAndInterfacesStatus(bool succedded)
//		{
//			if (!this.collectingDataIsInProgress || this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//				return;

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;

//				return;
//			}

//			SnmpAlarmStatus alarmStatus = (succedded) ? SnmpAlarmStatus.Normal : SnmpAlarmStatus.Critical;

//			var result = await this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(alarmStatus);

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;
//			}

//			return;
//		}

//		private async Task<bool> SetDeviceAndInterfacesStatusAsync(bool succedded)
//		{
//			if (!this.collectingDataIsInProgress || this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//				return false;

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;

//				return false;
//			}

//			SnmpAlarmStatus alarmStatus = (succedded) ? SnmpAlarmStatus.Normal : SnmpAlarmStatus.Critical;

//			var result = await this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(alarmStatus);

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;
//			}

//			return result;
//		}

//		private async ValueTask snmpManagerControl_OnAsyncResponse(SnmpRequestInfo snmpAsyncResult, SnmpResponseEventArgs snmpResponse)
//		{
//			if (!this.collectingDataIsInProgress || this.NetworkDevice == null || this.NetworkDevice.IsDeleted)
//				return;

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;

//				return;
//			}

//			this.retryCount++;

//			if (snmpResponse.ErrorStatus == 0 && snmpResponse.Values != null && snmpResponse.Values.Length > 0 && !snmpResponse.Values[0].IsNullOrNoSuchObject() &&
//				snmpResponse.Values[0].ObjectValueType == SnmpObjectValueType.OctetString)
//			{
//				// SNMP Succeeded
//				await this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Normal);
//				this.collectingDataIsInProgress = false;
//			}
//			else if (this.retryCount < this.NumberOfRetries)
//			{
//				try
//				{
//					await this.Provider.Snmp.GetAsync(SnmpOIDs.System.sysName); //, null, this.snmpManagerControl_OnAsyncResponse);
//				}
//				catch
//				{
//					await this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Critical);
//					this.collectingDataIsInProgress = false;
//				}
//			}
//			else
//			{
//				await this.SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus.Critical);
				
//				this.collectingDataIsInProgress = false;
//			}

//			if (this.stopRequested)
//			{
//				this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//				this.collectingDataIsInProgress = false;
//				this.stopRequested = false;
//			}
//		}

//		//private void SetNetworkDeviceStatusAndTakeFurtherActions(SnmpAlarmStatus alatmStatus)
//		//{
//		//	if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted || this.stopRequested)
//		//		return;

//		//	try
//		//	{
//		//		Dictionary<SimpleObject, int> interfaceStatusesBySimpleObject = null;

//		//		this.NetworkDevice.Status = (int)alatmStatus;

//		//		if (this.stopRequested)
//		//			return;

//		//		if (alatmStatus == SnmpAlarmStatus.Critical)
//		//		{
//		//			// Sets device interfaces status to unknown as default value (we don't know is up or down and we so we set its value to unknown)
//		//			this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//		//		}
//		//		else
//		//		{
//		//			// Gets the interface status and set its values
//		//			// Update connection settings, if some network device changes were occurred meanwhile
//		//			if (this.Provider.UseSnmp)
//		//				this.Provider.SetSnmpSettingsForMonitor(this.NetworkDevice);

//		//			if (this.Provider.UseTerminal)
//		//				this.Provider.SetTerminalSettings(this.NetworkDevice);

//		//			if (this.Provider.UseWeb)
//		//				this.Provider.SetWebSettings(this.NetworkDevice);

//		//			if (this.stopRequested)
//		//				return;

//		//			// No provider connection require since this.Provider.Interfaces.GetBulkOperationalStatus() rely on SnmpControl which is connectionless
//		//			if (this.NetworkDevice.Interfaces.Count > 0) // || this.NetworkDevice.L3Interfaces.Count() > 0)
//		//			{
//		//				IRequestResult<IEnumerable<InterfaceStatusInfo>> statusResult = null;

//		//				for (int i = 0; i < this.NumberOfRetries; i++)
//		//				{
//		//					statusResult = this.Provider.Interfaces.GetBulkOperationalStatus();

//		//					if (statusResult.Succeeded)
//		//					{
//		//						IEnumerable<InterfaceStatusInfo> providerInterfaceStatusInfoList = statusResult.ResultValue;

//		//						if (this.stopRequested)
//		//							return;

//		//						lock (this.lockStatusesBySimpleObject)
//		//						{
//		//							interfaceStatusesBySimpleObject = new Dictionary<SimpleObject, int>();

//		//							foreach (InterfaceStatusInfo providerInterfaceStatusInfo in providerInterfaceStatusInfoList)
//		//							{
//		//								Interface deviceInterface = this.FindDeviceInterface(providerInterfaceStatusInfo.InterfaceName);

//		//								if (deviceInterface != null && deviceInterface.Status != providerInterfaceStatusInfo.OperationalStatus)
//		//									interfaceStatusesBySimpleObject.Add(deviceInterface, (int)providerInterfaceStatusInfo.OperationalStatus);

//		//								if (this.stopRequested)
//		//									return;
//		//							}

//		//							break;
//		//						}
//		//					}
//		//				}

//		//				if (!statusResult.Succeeded)
//		//					this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//		//			}

//		//			if (interfaceStatusesBySimpleObject.Count > 0)
//		//				this.NetworkDevice.Manager.SetMultipleStatusChange(interfaceStatusesBySimpleObject);
//		//		}
//		//	}
//		//	catch
//		//	{
//		//	}
//		//	finally
//		//	{
//		//		if (this.Provider != null)
//		//			this.Provider.Close();
//		//	}
//		//}

//		//private async Task<IDictionary<string, InterfaceOperationalStatus>> GetInterfacesStatusAsync()
//		//{
//		//	Dictionary<string, InterfaceOperationalStatus> result = null;

//		//	if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted || this.stopRequested)
//		//		return null;

//		//	try
//		//	{
//		//		if (this.stopRequested)
//		//			return null;

//		//		// Gets the interface status and set its values. Update connection settings, if some network device changes were occurred meanwhile
//		//		if (this.Provider.UseSnmp)
//		//			this.Provider.SetSnmpSettingsForMonitor(this.NetworkDevice);

//		//		if (this.Provider.UseTerminal)
//		//			this.Provider.SetTerminalSettings(this.NetworkDevice);

//		//		if (this.Provider.UseWeb)
//		//			this.Provider.SetWebSettings(this.NetworkDevice);

//		//		if (this.stopRequested)
//		//			return null;

//		//		// No provider connection require since this.Provider.Interfaces.GetBulkOperationalStatus() rely on SnmpControl which is connectionless
//		//		for (int i = 0; i < this.NumberOfRetries; i++)
//		//		{
//		//			try
//		//			{
//		//				var bulkOperationalStatusResult = await this.Provider.Interfaces.GetBulkOperationalStatus();

//		//				result = bulkOperationalStatusResult.ResultValue;

//		//				if (this.stopRequested)
//		//					return null;

//		//				break;
//		//			}
//		//			catch //(Exception ex)
//		//			{
//		//				// this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//		//				//result = null;
//		//			}
//		//		}
//		//	}
//		//	catch
//		//	{
//		//	}
//		//	finally
//		//	{
//		//		if (this.Provider != null)
//		//			this.Provider.Close();
//		//	}

//		//	return result;
//		//}

//		private async ValueTask<TaskInfo<Dictionary<string, InterfaceOperationalStatus>>> GetInterfacesOperationalStatusAsync()
//		{
//			Dictionary<string, InterfaceOperationalStatus> result = EmptyInterfacesOperationalStatu;

//			if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted || this.stopRequested)
//				return new TaskInfo<Dictionary<string, InterfaceOperationalStatus>>(null, TaskResultInfo.NoSuchData, "NetworkDevice is null or is deleted");

//			try
//			{
//				if (this.stopRequested)
//					return new TaskInfo<Dictionary<string, InterfaceOperationalStatus>>(null, TaskResultInfo.Cancelled, "Stop is requested");

//				// Gets the interface status and set its values. Update connection settings, if some network device changes were occurred meanwhile
//				if (this.Provider.UseSnmpConnection)
//					this.Provider.SetSnmpSettings(this.NetworkDevice);

//				if (this.Provider.UseTerminalConnection)
//					this.Provider.SetTerminalSettings(this.NetworkDevice);

//				if (this.Provider.UseWebConnection)
//					this.Provider.SetWebSettings(this.NetworkDevice);

//				if (this.stopRequested)
//					return new TaskInfo<Dictionary<string, InterfaceOperationalStatus>>(null, TaskResultInfo.Cancelled, "Stop is requested");

//				// No provider connection require since this.Provider.Interfaces.GetBulkOperationalStatus() rely on SnmpClient which is connectionless
//				for (int i = 0; i < this.NumberOfRetries; i++)
//				{
//					try
//					{
//						var requestResult = await this.Provider.Interfaces.GetBulkOperationalStatus();

//						if (this.stopRequested)
//							return new TaskInfo<Dictionary<string, InterfaceOperationalStatus>>(null, TaskResultInfo.Cancelled, "Stop is requested");

//						if (requestResult.ResultValue != null)
//							result = requestResult.ResultValue;

//						break;
//					}
//					catch //(Exception ex)
//					{
//						// this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//						//result = null;
//					}
//				}
//			}
//			catch
//			{
//			}
//			finally
//			{
//				if (this.Provider != null)
//					await this.Provider.CloseAsync();
//			}

//			return new TaskInfo<Dictionary<string, InterfaceOperationalStatus>>(result);
//		}

//		private async ValueTask<bool> SetNetworkDeviceStatusAndTakeFurtherActionsAsync(SnmpAlarmStatus alatmStatus)
//		{
//			if (this.NetworkDevice == null || this.NetworkDevice.IsDeleted || this.stopRequested)
//				return false;

//			try
//			{
//				this.NetworkDevice.SystemStatus = (int)alatmStatus;

//				if (this.stopRequested)
//					return false;

//				if (alatmStatus == SnmpAlarmStatus.Critical)
//				{
//					// Sets device interfaces status to unknown as default value (we don't know is up or down and we so we set its value to unknown)
//					this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//				}
//				else
//				{
//					var interfaceNamesByOperationalStatusResult = await this.GetInterfacesOperationalStatusAsync();

//					if (interfaceNamesByOperationalStatusResult.Succeeded && interfaceNamesByOperationalStatusResult.ResultValue != null)
//					{
//						Dictionary<string, InterfaceOperationalStatus> interfaceStatusByInterfaceName = interfaceNamesByOperationalStatusResult.ResultValue;
//						Dictionary<INetworkDevice, int> interfaceStatusesBySimpleObject;

//						lock (this.lockStatusesBySimpleObject)
//						{
//							interfaceStatusesBySimpleObject = new Dictionary<INetworkDevice, int>();

//							foreach (var item in interfaceStatusByInterfaceName)
//							{
//								string interfaceName = item.Key;
//								InterfaceOperationalStatus operationalStatus = item.Value;

//								Interface deviceInterface = this.FindInterfaceByName(interfaceName);

//								if (deviceInterface != null && deviceInterface.SystemStatus != operationalStatus)
//									interfaceStatusesBySimpleObject.Add(deviceInterface, (int)operationalStatus);

//								if (this.stopRequested)
//									return false;
//							}
//						}

//						if (interfaceStatusesBySimpleObject.Count > 0)
//							this.NetworkDevice.Manager.SetMultipleStatusChange(interfaceStatusesBySimpleObject);
//					}
//					else
//					{
//						this.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: false);
//					}
//				}
//			}
//			catch
//			{
//				return false;
//			}

//			return true;
//		}

//		public void SetDefaultStatusForDeviceAndItsInterfaces(bool setStatusForNetworkDevice)
//		{
//			try
//			{
//				Dictionary<SimpleObject, int> interfaceStatusesBySimpleObject = new Dictionary<SimpleObject, int>();

//				if (setStatusForNetworkDevice)
//					this.NetworkDevice.SystemStatus = (int)SnmpAlarmStatus.Unknown;

//				lock (this.lockUnknownStatusesBySimpleObject)
//				{
//					List<InterfaceBase> allDeviceInterfaces = new List<InterfaceBase>();

//					allDeviceInterfaces.AddRange(this.NetworkDevice.Interfaces);
//					//allDeviceInterfaces.AddRange(this.NetworkDevice.L2Interfaces);
//					//allDeviceInterfaces.AddRange(this.NetworkDevice.L3Interfaces);

//					foreach (InterfaceBase deviceInterface in allDeviceInterfaces)
//						if (deviceInterface.SystemStatus != InterfaceOperationalStatus.Default)
//							interfaceStatusesBySimpleObject.Add(deviceInterface, (int)InterfaceOperationalStatus.Default);

//					if (interfaceStatusesBySimpleObject.Count > 0)
//						this.NetworkDevice.Manager.SetMultipleStatusChange(interfaceStatusesBySimpleObject);
//				}
//			}
//			catch
//			{
//			}
//			//finally
//			//{
//			//	if (this.Provider != null)
//			//		this.Provider.Close();
//			//}
//		}

//		private Interface FindInterfaceByName(string interfaceName)
//		{
//			foreach (Interface deviceInterface in this.NetworkDevice.Interfaces)
//			{
//				string standardizedInterfaceName = this.GetStandardizedInterfaceName(deviceInterface.Name);

//				if (standardizedInterfaceName == interfaceName)
//					return deviceInterface;
//			}

//			//foreach (Layer2Interface layer2Interface in this.NetworkDevice.Layer2Interfaces)
//			//{
//			//	string standardizedInterfaceName = this.GetStandardizedInterfaceName(layer2Interface.Name);

//			//	if (standardizedInterfaceName == interfaceName)
//			//		return layer2Interface;
//			//}

//			//foreach (Layer3Interface layer3Interface in this.NetworkDevice.Layer3Interfaces)
//			//{
//			//	string standardizedInterfaceName = this.GetStandardizedInterfaceName(layer3Interface.Name);

//			//	if (standardizedInterfaceName == interfaceName)
//			//		return layer3Interface;
//			//}

//			return null;
//		}

//		private string GetStandardizedInterfaceName(string interfaceName)
//		{
//			string standardizedInterfaceName = interfaceName;
//			TaskInfo<string> standardizedInterfaceNameProviderRequestResult = this.Provider.Interfaces.GetStandardizedName(interfaceName);

//			if (standardizedInterfaceNameProviderRequestResult.Succeeded && standardizedInterfaceNameProviderRequestResult.ResultValue != null)
//				standardizedInterfaceName = standardizedInterfaceNameProviderRequestResult.ResultValue;

//			return standardizedInterfaceName;
//		}

//		private void PullingTimer_Elapsed(object? sender, ElapsedEventArgs e)
//		{
//			//_ = this.StartInternalAsync();
//			_ = this.CollectDataAsync();
//			this.pullingTimer.Interval = this.NetworkDevice.MonitorPollingInterval * 1000;
//		}
//	}
//}