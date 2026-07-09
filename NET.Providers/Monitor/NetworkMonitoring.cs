//using Logaritam.Objects;
//using Logaritam.Services;
//using NET.Providers.Snmp;
//using Simple;
//using Simple.Network;
//using Simple.Objects;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace NET.Provider
//{
//	public class NetworkMonitoring
//	{
//		private ObjectManager? objectManager = null;
//		private Dictionary<Device, NetworkDeviceMonitor> deviceMonitorsByNetworkDevics = new Dictionary<Device, NetworkDeviceMonitor>();
//		Task? monitoeThread = null;
//		private int startDelay = 5000; // in milliseconds
//		private int startingInterval = 500; // in milliseconds
//		private bool stopIsRequested = false;
//		private bool startIsRequested = false;
//		private MonitoringStatus status = MonitoringStatus.Shutdown;
//		private CancellationTokenSource? cancellationTokenSource = null;
//		private object lockObject = new object();

//		public NetworkMonitoring(ObjectManager objectManager)
//		{
//			this.ObjectManager = objectManager;
//			//this.startDelayTimer.Elapsed += new System.Timers.ElapsedEventHandler(startDelayTimer_Elapsed);
//			//this.startDelayTimer.Interval = this.startDelay;
//			this.Status = Services.MonitoringStatus.Shutdown;
//		}

//		public event MonitorStatusChangeEventHandler? StatusChange;

//		public MonitoringStatus Status
//		{
//			get => this.status;

//			private set
//			{
//				if (value != this.status)
//				{
//					MonitoringStatus oldStatus = this.status;

//					this.status = value;
//					this.StatusChange?.Invoke(this, new MonitoringStatusChangeEventArgs(this.status, oldStatus));
//				}
//			}
//		}

//		public ObjectManager? ObjectManager
//		{
//			get { return this.objectManager; }

//			set
//			{
//				if (this.objectManager != null)
//				{
//					this.objectManager.NewObjectCreated -= ObjectManager_NewClientObjectCreated;
//					this.objectManager.BeforeDelete -= ObjectManager_BeforeDeleting;
//					this.objectManager.PropertyValueChange -= ObjectManager_PropertyValueChange;
//				}

//				this.objectManager = value;

//				if (this.objectManager != null)
//				{
//					this.objectManager.NewObjectCreated += ObjectManager_NewClientObjectCreated;
//					this.objectManager.BeforeDelete += ObjectManager_BeforeDeleting;
//					this.objectManager.PropertyValueChange += ObjectManager_PropertyValueChange;
//				}
//			}
//		}

//		/// <summary>
//		/// Gets or sets device monitor start delay.
//		/// </summary>
//		public int StartDelay
//		{
//			get { return this.startDelay; }
//			set { this.startDelay = value; }
//		}

//		/// <summary>
//		/// Gets or sets the device monitoring start interval in milliseconds.
//		/// </summary>
//		public int StartingInterval
//		{
//			get { return this.startingInterval; }
//			set { this.startingInterval = value; }
//		}

//		public void StartAsyncWithDelay()
//		{
//			if (!this.startIsRequested) // If startIsRequested == True, start is already requested.
//			{
//				this.cancellationTokenSource ??= new CancellationTokenSource();

//				this.monitoeThread ??= new Task(() => this.StartWithStartDelayInternal(), this.cancellationTokenSource.Token);

//				//this.monitoeThread.IsBackground = true;
//				//this.monitoeThread.Priority = ThreadPriority.BelowNormal;
//				this.monitoeThread.Start();
//			}
//		}

//		/// <summary>
//		/// Start moniroting engine async
//		/// </summary>
//		/// <param name="delay">The start delay in milliseconds</param>
//		public async ValueTask StartAsync()
//		{
//			if (this.Status == MonitoringStatus.Started || this.Status == MonitoringStatus.StartingUp)
//				return;

//			//	if (!this.startIsRequested) // If startIsRequested == True, start is already requested.
//			//	{
//			//		this.monitoeThread ??= new Task(() => this.StartInternal(), this.cancellationTokenSource.Token);

//			//		//this.monitoeThread.IsBackground = true;
//			//		//this.monitoeThread.Priority = ThreadPriority.BelowNormal;
//			//		this.monitoeThread.Start();
//			//	}
//			//}

//			//private void StartInternal()
//			//{
//			this.startIsRequested = true;
//			this.stopIsRequested = false;
//			this.Status = MonitoringStatus.StartingUp;

//			//if (this.StartDelay > 0)
//			//	Thread.Sleep(this.StartDelay);

//			try
//			{
//				List<Device> networkDevicesForMonitor = new List<Device>();
//				//this.deviceMonitorsByNetworkDeviceId.Clear();

//				if (this.ObjectManager?.Devices != null)
//				{
//					foreach (Device networkDevice in this.ObjectManager.Devices.ToArray())
//					{
//						try
//						{
//							if (networkDevice != null && networkDevice.MonitorDevice && !networkDevice.DeleteStarted)
//								networkDevicesForMonitor.Add(networkDevice);
//						}
//						catch
//						{
//						}
//					}
//				}

//				//ParallelLoopResult result = Parallel.ForEach(networkDevicesForMonitor, (networkDevice) =>
//				//																	   {
//				//																			if (this.stopIsRequested)
//				//																				return;

//				//																			if (networkDevice != null && networkDevice.MonitorDevice && !networkDevice.IsDeleting)
//				//																			{
//				//																				try
//				//																				{
//				//																					this.CreateAndRunDeviceMonitoring(networkDevice, startAsyncWithNewThread: false, delayStart: false);
//				//																				}
//				//																				catch
//				//																				{
//				//																				}
//				//																			}

//				//																			if (this.stopIsRequested)
//				//																				return;

//				//																			Thread.Sleep(this.StartingInterval);
//				//																	   });

//				//Thread.Sleep(this.StartDelay);

//				foreach (Device networkDevice in networkDevicesForMonitor)
//				{
//					if (this.stopIsRequested)
//						return;

//					if (networkDevice != null && networkDevice.MonitorDevice && !networkDevice.DeleteStarted)
//					{
//						//ThreadPool.QueueUserWorkItem(new WaitCallback(delegate { this.CreateAndRunDeviceMonitoring(networkDevice, startAsyncWithNewThread: false, delayStart: false); }));

//						try
//						{
//							_ = this.CreateAndRunDeviceMonitoring(networkDevice, delayStart: false); // startAsyncWithNewThread: true,
//						}
//						catch
//						{
//						}
//					}

//					if (this.stopIsRequested)
//						return;

//					//Thread.Sleep(this.StartingInterval);
//				}

//			}
//			catch
//			{
//			}
//			finally
//			{
//				this.startIsRequested = false;

//				if (!this.stopIsRequested)
//					this.Status = MonitoringStatus.Started;
//			}
//		}

//		//public void Stop()
//		//{
//		//	if (this.Status == MonitoringStatus.Shutdown || this.Status == MonitoringStatus.ShuttingDown)
//		//		return;

//		//	this.stopIsRequested = true;

//		//	this.ObjectManager.BeginLargeUpdate();
//		//	this.StopInternal();
//		//	this.ObjectManager.EndLargeUpdate();
//		//}

//		public async ValueTask StopAsync()
//		{
//			if (this.Status == MonitoringStatus.Shutdown || this.Status == MonitoringStatus.ShuttingDown)
//				return;

//			//	if (!this.stopIsRequested) // If stopIsRequested == True, stop is already requested.
//			//	{
//			//		this.stopIsRequested = true;

//			//		Thread setStatusThread = new Thread(this.StopInternal);

//			//		setStatusThread.IsBackground = true;
//			//		setStatusThread.Priority = ThreadPriority.BelowNormal;
//			//		setStatusThread.Start();
//			//	}
//			//}

//			//private async ValueTask StopInternal()
//			//{
//			this.stopIsRequested = true;
//			this.startIsRequested = false;
//			this.Status = MonitoringStatus.ShuttingDown;

//			try
//			{
//				//this.ObjectManager.BeginLargeUpdate();
//				//lock (this.lockObject)
//				//{
//				if (this.startIsRequested)
//					return;

//				var deviceDictionary = this.deviceMonitorsByNetworkDevics.ToArray();

//				foreach (var item in deviceDictionary)
//				{
//					Device networkDevice = item.Key;
//					NetworkDeviceMonitor deviceMonitor = item.Value;

//					await deviceMonitor.StopAsync();
//					//Thread.Sleep(10);

//					if (this.startIsRequested)
//						return;
//				}
//				//}

//				for (int i = 0; i < deviceDictionary.Count(); i++)
//				{
//					//while (deviceDictionary.Count > 0)
//					//{
//					//lock (this.lockObject)
//					//{
//					if (this.startIsRequested)
//						return;

//					//IDictionaryEnumerator dictionaryEnumerator = this.deviceMonitorsByNetworkDevics.GetEnumerator();
//					//dictionaryEnumerator.Reset();
//					//dictionaryEnumerator.MoveNext();

//					//long networkDeviceId = (long)dictionaryEnumerator.Key;

//					Device networkDevice = deviceDictionary.ElementAt(i).Key; // this.deviceMonitorsByNetworkDevics.ElementAt(0).Key;
//					NetworkDeviceMonitor deviceMonitor = deviceDictionary.ElementAt(i).Value; // this.deviceMonitorsByNetworkDevics.ElementAt(0).Value;

//					_ = deviceMonitor.StopAsync();

//					networkDevice.SystemStatus = (int)SnmpAlarmStatus.Unknown;
//					//deviceMonitor.SetDefaultStatusForDeviceAndItsInterfaces(setStatusForNetworkDevice: true);
//					deviceMonitor.Dispose();
//					//deviceMonitor = null;

//					if (this.deviceMonitorsByNetworkDevics.ContainsKey(networkDevice))
//						this.deviceMonitorsByNetworkDevics.Remove(networkDevice);

//					//deviceDictionary.Remove(networkDevice);
//					//}
//				}

//				if (this.startIsRequested)
//					return;

//				//if (this.deviceMonitorsByNetworkDevics.Count == 0)
//				//	Thread.Sleep(this.StartingInterval * 2);
//				//else
//				//	Thread.Sleep(10);
//				//}
//			}
//			catch
//			{
//			}
//			finally
//			{
//				//this.stopIsRequested = false;
//				//this.ObjectManager.EndLargeUpdate();

//				if (!this.startIsRequested)
//					this.Status = MonitoringStatus.Shutdown;

//				if (this.cancellationTokenSource != null)
//				{
//					this.cancellationTokenSource.Cancel(); //this.monitoeThread.Abort();
//					this.monitoeThread = null;
//				}
//			}
//		}

//		private void ObjectManager_NewClientObjectCreated(object sender, SimpleObjectChangeContainerContextRequesterEventArgs e)
//		{
//			if (e.SimpleObject is Device device)
//			{
//				if (device.MonitorDevice)
//				{
//					if (this.Status == MonitoringStatus.Started || this.Status == MonitoringStatus.StartingUp)
//					{
//						try
//						{
//							_ = this.CreateAndRunDeviceMonitoring(device, delayStart: true); // startAsyncWithNewThread: true
//						}
//						catch
//						{
//						}
//					}
//				}
//			}
//		}

//		private void ObjectManager_BeforeDeleting(object sender, SimpleObjectChangeContainerContextRequesterEventArgs e)
//		{
//			if (e.SimpleObject is Device device)
//			{
//				if (this.Status == MonitoringStatus.Started || this.Status == MonitoringStatus.StartingUp)
//				{
//					NetworkDeviceMonitor? deviceMonitor;

//					if (this.deviceMonitorsByNetworkDevics.TryGetValue(device, out deviceMonitor))
//					{
//						deviceMonitor.StopAsync();
//						deviceMonitor = null;
//						this.deviceMonitorsByNetworkDevics.Remove(device);
//					}
//				}
//			}
//		}

//		private void ObjectManager_PropertyValueChange(object sender, ChangePropertyValuePertyModelSimpleObjectChangeContainerContextRequesterEventArgs e)
//		{
//			if (e.SimpleObject is Device device)
//			{
//				if (e.PropertyModel?.PropertyIndex == DeviceModel.PropertyModel.MonitorDevice.PropertyIndex)
//				{
//					if (device.MonitorDevice)
//					{
//						if (this.Status == MonitoringStatus.Started || this.Status == MonitoringStatus.StartingUp)
//						{
//							if (!this.deviceMonitorsByNetworkDevics.ContainsKey(device))
//							{
//								try
//								{
//									bool delayStart = device.IsNew;

//									_ = this.CreateAndRunDeviceMonitoring(device, delayStart);

//									//if (networkDevice.IsNew)
//									//{
//									//	this.CreateAndRunDeviceMonitoring(networkDevice, startAsyncWithNewThread: true, delayStart: true);
//									//}
//									//else
//									//{
//									//	//this.CreateAndRunDeviceMonitoring(networkDevice, startAsyncWithNewThread: true, delayStart: false);
//									//	this.CreateAndRunDeviceMonitoring(networkDevice, startAsyncWithNewThread: true, delayStart: false);
//									//}
//								}
//								catch
//								{
//								}
//							}
//						}
//					}
//					else
//					{
//						NetworkDeviceMonitor? deviceMonitor;

//						if (this.deviceMonitorsByNetworkDevics.TryGetValue(device, out deviceMonitor))
//						{
//							deviceMonitor.StopAsync();
//							deviceMonitor = null;
//							this.deviceMonitorsByNetworkDevics.Remove(device);
//							device.Manager.AsAppObjectManager().DeviceMonitorIsStopped(device);
//						}
//					}
//				}
//				//else if (e.PropertyModel.Index == DeviceModel.PropertyModel.MonitoringMethod.Index)
//				//{
//				//	DeviceMonitor deviceMonitor;

//				//	if (this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				//		deviceMonitor.MonitoringMethod = networkDevice.MonitoringMethod;
//				//}
//				//else if (e.PropertyModel.Index == DeviceModel.PropertyModel.MonitorPoolingInterval.Index)
//				//{
//				//	DeviceMonitor deviceMonitor;

//				//	if (this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				//		deviceMonitor.PullingInterval = networkDevice.MonitorPoolingInterval;
//				//}
//				//else if (e.PropertyModel.Index == DeviceModel.PropertyModel.MonitorTimeout.Index)
//				//{
//				//	DeviceMonitor deviceMonitor;

//				//	if (this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				//		deviceMonitor.Timeout = networkDevice.MonitorTimeout;
//				//}
//				//else if (e.PropertyModel.Index == DeviceModel.PropertyModel.MonitorNumOfRetries.Index)
//				//{
//				//	DeviceMonitor deviceMonitor;

//				//	if (this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				//		deviceMonitor.NumberOfRetries = networkDevice.MonitorNumOfRetries;
//				//}
//				//else if (e.PropertyModel.Index == DeviceModel.PropertyModel.SnmpMaximumBulkRepetitions.Index)
//				//{
//				//	DeviceMonitor deviceMonitor;

//				//	if (this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				//		deviceMonitor.MaximumBulkRepetitions = networkDevice.SnmpMaximumBulkRepetitions;
//				//}
//			}
//		}

//		private async ValueTask CreateAndRunDeviceMonitoring(Device networkDevice, bool delayStart) //  bool startAsyncWithNewThread
//		{
//			if (networkDevice == null || networkDevice.DeleteStarted)
//				return;

//			NetworkDeviceMonitor? deviceMonitor;

//			lock (this.lockObject)
//			{
//				if (!this.deviceMonitorsByNetworkDevics.TryGetValue(networkDevice, out deviceMonitor))
//				{
//					deviceMonitor = new DeviceMonitor(networkDevice);
//					this.deviceMonitorsByNetworkDevics.Add(networkDevice, deviceMonitor);
//				}
//			}
//			//if (startAsyncWithNewThread)
//			//	deviceMonitor.StartAsyncUsingNewThread();
//			//else
//			await deviceMonitor.StartAsync(delayStart);

//			return;
//		}

//		private async void StartWithStartDelayInternal()
//		{
//			if (this.StartDelay > 0)
//				Thread.Sleep(this.StartDelay);

//			await this.StartAsync();
//		}

//		~NetworkMonitoring()
//		{
//			this.objectManager = null;
//		}

//		public delegate void MonitorStatusChangeEventHandler(object sender, MonitoringStatusChangeEventArgs e);
//	}

//	public class MonitoringStatusChangeEventArgs
//	{
//		public MonitoringStatusChangeEventArgs(MonitoringStatus status, MonitoringStatus oldStatus)
//		{
//			this.Status = status;
//			this.OldStatus = oldStatus;
//		}

//		public MonitoringStatus Status { get; private set; }
//		public MonitoringStatus OldStatus { get; private set; }
//	}

//	public enum MonitoringStatus
//	{
//		Shutdown,
//		Started,
//		StartingUp,
//		ShuttingDown
//	}
//}