//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
////using System.Timers;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Simple.Threading;
//using NET.Tools.Telnet;

//namespace NET.Tools.Terminal
//{
//	public class TerminalControl : IProviderConnection, IDisposable
//	{
//		#region |   Private Members   |

//		private bool autoLogin = true;
//		private TerminalProtocol terminalProtocol = TerminalProtocol.Telnet;
//		private TelnetProviderType telnetProviderType = TelnetProviderType.TelnetPipeClient;
//		private Ssh1ProviderType ssh1ProviderType = Ssh1ProviderType.SshGranados;
//		private Ssh2ProviderType ssh2ProviderType = Ssh2ProviderType.SshNet;
//		private string remoteHost = String.Empty;
//		private int remotePort = 23;
//		private string username = String.Empty;
//		private string password = String.Empty;
//		private string enableSecret = String.Empty;
//		private int timeout = 10; // in seconds
//		private int sendingInterval = 40; // in milliseconds
//		private string promptSeparator = "|";
//		private string usernamePrompts = "login|username|user name|user";
//		private string passwordPrompts = "password";
//		private string enableSecretPrompts = "password";
//		private string nonPrivilegeModePrompts = ">";
//		private string privilegeModeCommand = "enable";
//		private string morePrompts = "--More--|---- More ----|-- More --|More: <space>";
//		private string privilegeModePrompts = "#";
//		private string configModeCommand = "configure terminal";
//		private string exitConfigModeCommand = "exit";
//		private string vlanDatabaseConfigCommand = "vlan database";
//		private string exitVlanDatabaseConfigCommand = "exit";
//		private string interfaceConfigCommand = "interface";
//		private string exitInterfaceConfigCommand = "exit";
//		private string logoutCommand = "logout";

//		private object providerControl;
//		private List<string> usernamePromptList = null;
//		private List<string> passwordPromptList = null;
//		private List<string> enableSecretPromptList = null;
//		private List<string> morePromptList = null;
//		private List<string> nonPrivilegeModePromptList = null;
//		private List<string> privilegeModePromptList = null;
//		private List<string> falsePrivilegeModePromptList = new List<string>() { "[Y/N]:" };
//		private string privilegeModePrompt = String.Empty;
//		private System.Timers.Timer timeoutTimer = null;
//		private string logFileName = String.Empty;
//		private TextWriter logTextWriter = null;
//		private bool isTerminalTypeSent = false;
//		private string lastSentSyncCommand = String.Empty;
//		private string response = String.Empty;
//		private int token = 0;

//		#endregion |   Private Members   |

//		#region |   Protected Members   |

//		protected bool logging = false;
//		//protected string receivedData = String.Empty;
//		protected string processedLogInReceivedData = String.Empty;
//		protected string processedReceivedData = String.Empty;
//		protected TerminalConnectionState terminalConnectionStateStatus = TerminalConnectionState.Disconnected;
//		protected string[] waitFor = null;
//		protected ThreadSync<int> threadSync = new ThreadSync<int>();

//		//protected ThreadSyncToken threadSyncToken = new ThreadSyncToken("Terminal");
//		//protected string connectionMessage = String.Empty;
//		//protected string lastSendingSyncCommand = String.Empty;
//		protected string connectionLog = String.Empty;
//		protected TerminalConfigMode configMode = TerminalConfigMode.NonConfigMode;
//		protected string configInterfaceName = String.Empty;

//		#endregion |   Protected Members   |

//		#region |   Public Static Members   |

//		public static TerminalControl Default = new TerminalControl();

//		#endregion |   Public Static Members   |

//		#region |   Constructors and Initialization   |

//		public TerminalControl()
//		{
//			this.timeoutTimer = new System.Timers.Timer();
//			this.timeoutTimer.Enabled = false;
//			this.timeoutTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.timeoutTimer_Elapsed);
//		}

//		#endregion |   Constructors and Initialization   |

//		#region |   Events   |

//		public event ConnectedEventHandler Connected;
//		public event DisconnectedEventHandler Disconnected;
//		public event TextEventHandler TextReceived;
//		public event TextEventHandler TextSending;

//		#endregion

//		#region |   Public Properties   |

//		private TextWriter LogTextWriter
//		{
//			get
//			{
//				if (this.logTextWriter == null)
//				{

//					try
//					{
//						this.logTextWriter = new StreamWriter(this.logFileName, append: false);

//						//if (this.logging && !this.connectionMessage.IsNullOrEmpty())
//						//{
//						//	this.logTextWriter.Write(this.connectionMessage);
//						//	this.logTextWriter.Flush();
//						//}
//					}
//					catch
//					{

//					}
//				}

//				return this.logTextWriter;
//			}
//		}

//		public bool AutoLogin
//		{
//			get { return this.autoLogin; }
//			set { this.autoLogin = value; }
//		}

//		//public string ConnectionString
//		//{
//		//    get { return this.connectionStringBuilder.BuildConnectionString(); }
//		//    set
//		//    {
//		//        this.connectionStringBuilder.SetConnectionString(value);
//		//        this.SetSettingsToProviderControl();
//		//        this.SetLocalSettings();
//		//    }
//		//}

//		//public bool UseTerminalConnection
//		//{
//		//    get { return this.connectionStringBuilder.UseTerminalConnection; }
//		//    set { this.connectionStringBuilder.UseTerminalConnection = value; }
//		//}

//		public TerminalProtocol TerminalProtocol
//		{
//			get { return this.terminalProtocol; }
//			set { this.terminalProtocol = value; }
//		}

//		public TelnetProviderType TelnetProviderType
//		{
//			get { return this.telnetProviderType; }
//			set { this.telnetProviderType = value; }
//		}

//		public Ssh1ProviderType Ssh1ProviderType
//		{
//			get { return this.ssh1ProviderType; }
//			set { this.ssh1ProviderType = value; }
//		}

//		public Ssh2ProviderType Ssh2ProviderType
//		{
//			get { return this.ssh2ProviderType; }
//			set { this.ssh2ProviderType = value; }
//		}

//		public string RemoteHost
//		{
//			get { return this.remoteHost; }
//			set { this.remoteHost = value; }
//		}

//		public int RemotePort
//		{
//			get { return this.remotePort; }
//			set { this.remotePort = value; }
//		}

//		public string Username
//		{
//			get { return this.username; }

//			set
//			{
//				if (value == null)
//					value = String.Empty;

//				this.username = value;
//			}
//		}

//		public string Password
//		{
//			get { return this.password; }

//			set
//			{
//				if (value == null)
//					value = String.Empty;

//				this.password = value;
//			}
//		}

//		public string EnableSecret
//		{
//			get { return this.enableSecret; }

//			set
//			{
//				if (value == null)
//					value = String.Empty;

//				this.enableSecret = value;
//			}
//		}

//		/// <summary>
//		/// Timeout in seconds.
//		/// </summary>
//		public int Timeout
//		{
//			get { return this.timeout; }

//			set
//			{
//				this.timeout = value;
//				this.timeoutTimer.Interval = value * 1000;
//			}
//		}

//		/// <summary>
//		/// Data sending interval in milliseconds.
//		/// </summary>
//		public int SendingInterval
//		{
//			get { return this.sendingInterval; }
//			set
//			{
//				this.sendingInterval = value;
//				//this.SetSendingTimerInterval();
//			}
//		}

//		public bool IsConnected
//		{
//			get { return this.terminalConnectionStateStatus == TerminalConnectionState.Authenticated; }
//		}

//		public TerminalConnectionState ConnectionStatus
//		{
//			get { return this.terminalConnectionStateStatus; }
//		}

//		public string PromptSeparator
//		{
//			get { return this.promptSeparator; }

//			set
//			{
//				this.promptSeparator = value;
//				this.usernamePromptList = null;
//				this.passwordPromptList = null;
//				this.enableSecretPromptList = null;
//				this.nonPrivilegeModePromptList = null;
//				this.privilegeModePromptList = null;
//				this.morePromptList = null;
//			}
//		}

//		public string UsernamePrompts
//		{
//			get { return this.usernamePrompts; }
//			set
//			{
//				this.usernamePrompts = value;
//				this.usernamePromptList = null;
//			}
//		}

//		public string PasswordPrompts
//		{
//			get { return this.passwordPrompts; }
//			set
//			{
//				this.passwordPrompts = value;
//				this.passwordPromptList = null;
//			}
//		}

//		public string EnableSecretPrompts
//		{
//			get { return this.enableSecretPrompts; }
//			set
//			{
//				this.enableSecretPrompts = value;
//				this.enableSecretPromptList = null;
//			}
//		}

//		public string NonPrivilegeModePrompts
//		{
//			get { return this.nonPrivilegeModePrompts; }
//			set
//			{
//				this.nonPrivilegeModePrompts = value;
//				this.nonPrivilegeModePromptList = null;
//			}
//		}

//		public string PrivilegeModeCommand
//		{
//			get { return this.privilegeModeCommand; }
//			set { this.privilegeModeCommand = value; }
//		}

//		public string PrivilegeModePrompts
//		{
//			get { return this.privilegeModePrompts; }
//			set
//			{
//				this.privilegeModePrompts = value;
//				this.privilegeModePromptList = null;
//			}
//		}

//		public string MorePrompts
//		{
//			get { return this.morePrompts; }
//			set
//			{
//				this.morePrompts = value;
//				this.morePromptList = null;
//			}
//		}

//		public string ConfigModeCommand
//		{
//			get { return this.configModeCommand; }
//			set { this.configModeCommand = value; }
//		}

//		public string ExitConfigModeCommand
//		{
//			get { return this.exitConfigModeCommand; }
//			set { this.exitConfigModeCommand = value; }
//		}

//		public string VlanDatabaseConfigCommand
//		{
//			get { return this.vlanDatabaseConfigCommand; }
//			set { this.vlanDatabaseConfigCommand = value; }
//		}

//		public string ExitVlanDatabaseConfigCommand
//		{
//			get { return this.exitVlanDatabaseConfigCommand; }
//			set { this.exitVlanDatabaseConfigCommand = value; }
//		}

//		public string InterfaceConfigCommand
//		{
//			get { return this.interfaceConfigCommand; }
//			set { this.interfaceConfigCommand = value; }
//		}

//		public string ExitInterfaceConfigCommand
//		{
//			get { return this.exitInterfaceConfigCommand; }
//			set { this.exitInterfaceConfigCommand = value; }
//		}

//		public string LogoutCommand
//		{
//			get { return this.logoutCommand; }
//			set { this.logoutCommand = value; }
//		}


//		public string PromptLine { get; private set; }

//		public object Owner { get; set; }

//		#endregion |   Public Properties   |

//		#region |   Protected Properties   |

//		protected ITelnetControl ProviderControl
//		{
//			get
//			{
//				if (this.providerControl == null)
//					this.SetProviderControl();

//				return this.providerControl as ITelnetControl;
//			}
//		}

//		protected List<string> UsernamePromptList
//		{
//			get
//			{
//				if (this.usernamePromptList == null)
//					this.usernamePromptList = this.ConvertStringToPromptList(this.UsernamePrompts);

//				return this.usernamePromptList;
//			}
//		}

//		protected List<string> PasswordPromptList
//		{
//			get
//			{
//				if (this.passwordPromptList == null)
//					this.passwordPromptList = this.ConvertStringToPromptList(this.PasswordPrompts);

//				return this.passwordPromptList;
//			}
//		}

//		protected List<string> EnableSecretPromptList
//		{
//			get
//			{
//				if (this.enableSecretPromptList == null)
//					this.enableSecretPromptList = this.ConvertStringToPromptList(this.EnableSecretPrompts);

//				return this.enableSecretPromptList;
//			}
//		}

//		protected List<string> NonPrivilegeModePromptList
//		{
//			get
//			{
//				if (this.nonPrivilegeModePromptList == null)
//					this.nonPrivilegeModePromptList = this.ConvertStringToPromptList(this.NonPrivilegeModePrompts);

//				return this.nonPrivilegeModePromptList;
//			}
//		}

//		protected List<string> PrivilegeModePromptList
//		{
//			get
//			{
//				if (this.privilegeModePromptList == null)
//					this.privilegeModePromptList = this.ConvertStringToPromptList(this.PrivilegeModePrompts);

//				return this.privilegeModePromptList;
//			}
//		}

//		protected List<string> MorePromptList
//		{
//			get
//			{
//				if (this.morePromptList == null)
//					this.morePromptList = this.ConvertStringToPromptList(this.MorePrompts);

//				return this.morePromptList;
//			}
//		}

//		#endregion |   Protected Properties   |

//		#region |   Public Methods   |

//		public virtual IRequestResult Connect()
//		{
//			this.InitializeConnection();

//			IRequestResult result = this.TryConnect();

//			if (result.Succeeded)
//			{
//				if (this.AutoLogin)
//					result = this.LogIn();
//			}
//			else
//			{
//				result = new RequestResult<bool>(false, TaskResultInfo.ConnectionError, result.Message);
//				// throw new Exception(result.Message);
//			}

//			if (!result.Succeeded)
//				this.Disconnect();

//			return result;
//		}

//		public TaskInfo<string> TestConnection(WorkerContext workerContext)
//		{
//			string testResultMessage = "Terminal Connection Test: \r\n";
//			bool wasConnected = this.IsConnected;

//			if (workerContext != null)
//				workerContext.ReportProgress(-1, "Testing Terminal connection...");

//			if (this.IsConnected)
//				this.Disconnect();

//			IRequestResult result = this.Connect();

//			if (!wasConnected)
//				this.Disconnect();

//			//testResultMessage += (result.Succeed) ? "Success" : result.Message;
//			testResultMessage += result.Message;
//			result.Message = testResultMessage;

//			if (workerContext != null)
//				workerContext.Result = result;

//			return result;
//		}

//		protected void InitializeConnection()
//		{
//			//this.connectionMessage = String.Empty;

//			this.DisposeControl();

//			this.SetProviderControl();
//			this.SetProviderControlSettings();
//			this.SetLocalSettings();

//			if (this.logging)
//				this.SetLogging(this.logFileName);
//			//this.SetLocalSettings();

//			this.terminalConnectionStateStatus = TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt;
//		}

//		protected IRequestResult TryConnect()
//		{
//			try
//			{
//				this.ProviderControl.Connect();
//				this.ProviderControl.AcceptData = true;

//				//this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.Connected);

//				////this.Send("", "Login");
//				//Thread.Sleep(1000);

//				//this.Send("admin", "Password");
//				//this.Send("", "] >");
//				//this.Send("quit");



//				//System.Text.ASCIIEncoding encoding = new ASCIIEncoding();
//				//this.Send(encoding.GetBytes("\033[J2").ToString());

//			}
//			catch (Exception ex)
//			{
//				return new RequestResult<bool>(this.IsConnected, TaskResultInfo.ExceptionIsCaught, ex.GetFullErrorMessage());
//			}

//			return new RequestResult<bool>(this.IsConnected, TaskResultInfo.Succeeded);
//		}

//		protected IRequestResult LogIn()
//		{
//			//try
//			//{
//			double originalInterval = this.timeoutTimer.Interval;

//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = true;
//			this.timeoutTimer.Interval = originalInterval * 2; // Give addition time login process to complete
//			this.timeoutTimer.Start();

//			this.threadSync.WaitFor(this.token); //, this.Timeout * 1000);

//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = false;
//			this.timeoutTimer.Interval = originalInterval;
//			//}
//			//catch (Exception ex)
//			//{
//			//    this.connectionMessage = TerminalHelper.RemoveEscapeSequences(this.connectionLog);
//			//    string message = this.connectionMessage.IsNullOrEmpty() ? ex.Message : this.connectionMessage;
//			//    this.Disconnect();
//			//    throw new Exception(message);
//			//}

//			if (this.IsConnected)
//			{
//				this.OnConnected();
//				this.RaiseConnected();
//			}

//			//this.connectionMessage += this.connectionLog; //
//			//TerminalHelper.RemoveEscapeSequences(this.connectionLog);

//			//if (this.logging)
//			//    this.WriteToLog(this.connectionMessage);

//			TaskResultInfo actionResult = (this.IsConnected) ? TaskResultInfo.Succeeded : TaskResultInfo.Error;

//			return new RequestResult<bool>(this.IsConnected, actionResult, this.connectionLog);
//		}

//		public virtual void Disconnect()
//		{

//			//if (this.IsConnected)
//			//{
//			//    this.ExitConfigMode();
//			//    this.SendAsync("logout");
//			//}
//			if (this.ConnectionStatus == TerminalConnectionState.Disconnected)
//				return;

//			if (this.threadSync != null && this.threadSync.ContainsToken(this.token))
//				this.threadSync.Release(this.token);

//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = false;

//			bool wasConnected = this.IsConnected;

//			if (this.IsConnected)
//			{
//				this.ExitConfigMode();

//				//            if (this.logging)
//				//            {
//				//                if (this.cumulativeProcessedReceivedData.TrimStart().StartsWith(this.lastSendingCommand))
//				//                    this.cumulativeProcessedReceivedData = this.cumulativeProcessedReceivedData.TrimStart().Substring(this.lastSendingCommand.Length);

//				//                //this.WriteToLog(this.cumulativeProcessedReceivedData);
//				//	//this.WriteToLog(this.LogoutCommand + "\r\n");
//				//}

//				string logoutCmd = this.LogoutCommand ?? "logout";
//				this.SendAsync(logoutCmd);

//				Thread.Sleep(100);
//				this.WriteToLog(logoutCmd);
//				//this.SendAsync("\r\n");
//				//            Thread.Sleep(100);
//			}

//			if (this.ProviderControl.Connected)
//				this.ProviderControl.Disconnect();

//			this.providerControl = null;

//			this.terminalConnectionStateStatus = TerminalConnectionState.Disconnected;

//			//if (this.logTextWriter != null)
//			//{
//			//    if (wasConnected)
//			//    {
//			//        if (this.receivedDataWithoutEscapeSequences.TrimStart().StartsWith(this.lastSendingCommand))
//			//            this.receivedDataWithoutEscapeSequences = this.receivedDataWithoutEscapeSequences.TrimStart().Substring(this.lastSendingCommand.Length);

//			//        this.WriteToLog(this.receivedDataWithoutEscapeSequences);
//			//    }

//			//    this.logTextWriter.Close();
//			//}

//			//this.lastSendingSyncCommand = String.Empty;
//			this.lastSentSyncCommand = String.Empty;
//			//this.receivingBuffer = String.Empty;
//			this.processedReceivedData = String.Empty;
//			//this.connectionMessage = String.Empty;
//			this.connectionLog = String.Empty;
//			this.isTerminalTypeSent = false;
//			this.OnDisconnected();
//			this.RaiseDisconnected();

//			if (this.logTextWriter != null)
//			{
//				this.logTextWriter.Flush();
//				this.logTextWriter.Close();
//				this.logTextWriter.Dispose();
//				this.logTextWriter = null;
//			}
//		}

//		public virtual void FinishUpdate()
//		{
//		}

//		public string Send(string command)
//		{
//			return this.Send(command, true);
//		}

//		public string Send(string command, bool sendCrLf)
//		{
//			return this.Send(command, sendCrLf, null);
//		}

//		public string Send(string command, params string[] waitFor)
//		{
//			return this.Send(command, true, waitFor);
//		}

//		public string Send(string command, bool sendCrLf, params string[] waitFor)
//		{
//			string result = String.Empty;
//			//this.receivingBuffer = String.Empty;

//			if (!this.IsConnected)
//			{
//				IRequestResult connectResult = this.Connect();

//				//if (!connectResult.Succeeded)
//				//	return result;
//			}

//			string commandToSend = sendCrLf ? String.Format("{0}\r\n", command) : command;
//			this.waitFor = waitFor;
//			this.lastSentSyncCommand = commandToSend;
//			this.SendAsync(commandToSend);

//			this.threadSync.WaitFor(this.token); //, this.Timeout * 1000);

//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = false;

//			result = this.response;
//			this.response = String.Empty;
//			//response = this.filteredReceivedDataWithoutEscapeSequences;

//			//this.sendingCommand = String.Empty;
//			this.waitFor = null;

//			if (this.isTimeout)
//			{
//				this.isTimeout = false;
//				throw new TimeoutException("Terminal connection timeout.");
//			}

//			return result;
//		}

//		//public void SendAsync(string command)
//		//{
//		//    this.SendAsync(command, null);
//		//}

//		public void SendAsync(string command) //, params string[] waitFor)
//		{
//			if (command.IsNullOrEmpty())
//				throw (new Exception("TerminalControl.SendAsync cannot send null or empty string."));

//			this.OnTerminalDataSending(command);
//			this.RaiseTextSending(command);

//			//Debug.WriteLine("SENT: " + command);

//			//if (waitFor != null)
//			//this.waitFor = waitFor;

//			if (this.SendingInterval > 0)
//			{
//				//// Wait for time to elapse between two sends
//				//if (!this.sendingIntervalTimeElapsed)
//				//{
//				//    this.threadSync.WaitFor(this.threadSendingIntervalToken);
//				//}

//				//// Fire sendingIntervalTimer to prevent sending another data without pausing between two sendings.
//				//this.sendingIntervalTimeElapsed = false;
//				//this.sendingIntervalTimer.Start();

//				Thread.Sleep(this.SendingInterval);
//			}

//			this.threadSync.Prepare(this.token);

//			//if (this.IsConnected)
//			//    this.WriteToLog(command);

//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = true;
//			this.timeoutTimer.Start();

//			this.ProviderControl.SendAsync(command);
//		}

//		public virtual string EnterConfigMode()
//		{
//			string response = String.Empty;

//			if (!this.IsConnected)
//				this.Connect();

//			if (this.configMode != TerminalConfigMode.ConfigMode)
//			{
//				switch (this.configMode)
//				{
//					case TerminalConfigMode.VlanDatabaseConfig:

//						response = this.ExitVlanDatabaseConfig();
//						response = this.Send(this.ConfigModeCommand);

//						break;

//					case TerminalConfigMode.NonConfigMode:

//						response = this.Send(this.ConfigModeCommand);

//						break;

//					case TerminalConfigMode.InterfaceConfig:

//						this.ExitInterfaceConfig();

//						break;
//				}

//				this.configMode = TerminalConfigMode.ConfigMode;
//			}

//			return response;
//		}

//		public virtual string ExitConfigMode()
//		{
//			string response = String.Empty;

//			if (!this.IsConnected)
//				this.Connect();

//			if (this.configMode != TerminalConfigMode.NonConfigMode)
//			{
//				switch (this.configMode)
//				{
//					case TerminalConfigMode.ConfigMode:

//						response = this.Send(this.ExitConfigModeCommand);

//						break;

//					case TerminalConfigMode.VlanDatabaseConfig:

//						response = this.ExitVlanDatabaseConfig();

//						break;

//					case TerminalConfigMode.InterfaceConfig:

//						response = this.ExitInterfaceConfig();
//						response = this.Send(this.ExitConfigModeCommand);

//						break;
//				}

//				this.configMode = TerminalConfigMode.NonConfigMode;
//			}

//			return response;
//		}

//		public virtual string EnterInterfaceConfig(string interfaceName)
//		{
//			string response = String.Empty;

//			if (!this.IsConnected)
//				this.Connect();

//			if (this.configMode == TerminalConfigMode.InterfaceConfig && this.configInterfaceName == interfaceName)
//				return response;

//			switch (this.configMode)
//			{
//				case TerminalConfigMode.NonConfigMode:

//					this.EnterConfigMode();

//					break;

//				case TerminalConfigMode.InterfaceConfig:

//					this.ExitInterfaceConfig();

//					break;

//				case TerminalConfigMode.VlanDatabaseConfig:

//					this.ExitVlanDatabaseConfig();
//					this.EnterConfigMode();

//					break;
//			}

//			response = this.EnterInterfaceConfigInternal(interfaceName);

//			this.configMode = TerminalConfigMode.InterfaceConfig;
//			this.configInterfaceName = interfaceName;

//			return response;
//		}

//		public virtual string EnterVlanDatabaseConfig()
//		{
//			string response = String.Empty;

//			if (this.configMode != TerminalConfigMode.VlanDatabaseConfig)
//			{
//				response = this.ExitConfigMode();
//				response = this.Send(this.VlanDatabaseConfigCommand);
//				this.configMode = TerminalConfigMode.VlanDatabaseConfig;
//			}

//			return response;
//		}

//		public void SetLogging(string logFileName)
//		{
//			if (this.logTextWriter != null && this.logFileName == logFileName)
//				return;

//			this.logFileName = logFileName;
//			this.logging = false;

//			if (!logFileName.IsNullOrEmpty())
//			{
//				this.logging = true;

//				if (this.logTextWriter != null)
//				{
//					this.logTextWriter.Flush();
//					this.logTextWriter.Close();
//					this.logTextWriter.Dispose();
//					this.logTextWriter = null;
//				}
//			}
//		}

//		public void Dispose()
//		{
//			this.DisposeControl();

//			this.providerControl = null;

//			if (this.usernamePromptList != null)
//				this.usernamePromptList.Clear();

//			this.usernamePromptList = null;

//			if (passwordPromptList != null)
//				this.passwordPromptList.Clear();

//			this.passwordPromptList = null;

//			if (this.enableSecretPromptList != null)
//				this.enableSecretPromptList.Clear();

//			this.enableSecretPromptList = null;

//			if (this.morePromptList != null)
//				this.morePromptList.Clear();

//			this.morePromptList = null;

//			if (nonPrivilegeModePromptList != null)
//				this.nonPrivilegeModePromptList.Clear();

//			this.nonPrivilegeModePromptList = null;

//			if (this.privilegeModePromptList != null)
//				this.privilegeModePromptList.Clear();

//			this.privilegeModePromptList = null;

//			if (this.timeoutTimer != null)
//			{
//				this.timeoutTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.timeoutTimer_Elapsed);
//				this.timeoutTimer.Dispose();
//				this.timeoutTimer = null;
//			}

//			if (this.logTextWriter != null)
//			{
//				this.logTextWriter.Close();
//				this.logTextWriter.Dispose();
//			}

//			this.logTextWriter = null;

//			this.waitFor = null;
//			//this.threadSync.Dispose();
//			this.threadSync = null;
//			//this.threadSyncToken = null;
//		}

//		#endregion |   Public Methods   |

//		#region |   Protected Methods   |

//		protected virtual string EnterInterfaceConfigInternal(string interfaceName)
//		{
//			return this.Send(String.Format("{0} {1}", this.InterfaceConfigCommand, interfaceName));
//		}

//		protected virtual string ExitInterfaceConfig()
//		{
//			return this.Send(this.ExitInterfaceConfigCommand);
//		}

//		protected virtual string ExitVlanDatabaseConfig()
//		{
//			return this.Send(this.ExitVlanDatabaseConfigCommand);
//		}

//		protected List<string> ConvertStringToPromptList(string prompts)
//		{
//			List<string> result = new List<string>();
//			string[] resultArray = prompts.Split(new string[] { this.PromptSeparator }, StringSplitOptions.RemoveEmptyEntries);

//			foreach (string text in resultArray)
//			{
//				string element = text.Trim();

//				if (element.Length > 0)
//				{
//					result.Add(text.Trim());
//				}
//			}

//			return result;
//		}

//		protected string ConvertPromptListToString(List<string> promptList)
//		{
//			string result = String.Empty;
//			string separator = String.Empty;
//			string emptySpace = String.Empty;

//			for (int i = 0; i < promptList.Count; i++)
//			{
//				if (i > 0)
//					result += this.PromptSeparator + " ";

//				result += promptList[i];
//			}

//			return result;
//		}

//		protected void ChangeTerminalConnectionStateStatus(TerminalConnectionState terminalConnectionStateStatus) //, bool resetCumulativeReceivedData = false)
//		{
//			//bool isConnectedRightNow = this.terminalConnectionStateStatus != TerminalControlConnectionStatus.Connected && terminalConnectionStateStatus == TerminalControlConnectionStatus.Connected;
//			this.terminalConnectionStateStatus = terminalConnectionStateStatus;
//			//this.receivingBuffer = String.Empty;

//			//if (resetCumulativeReceivedData)
//			//	this.receivedData = String.Empty;

//			//this.ProviderControl.AcceptData = true;

//			//if (isConnectedRightNow)
//			//    this.threadSync.Release(this.threadSyncToken);
//		}

//		protected void OnConnected()
//		{
//		}

//		protected void OnDisconnected()
//		{
//		}

//		protected void OnTerminalDataSending(string data)
//		{
//		}

//		protected void OnTerminalDataReceived(string data)
//		{
//		}

//		protected virtual string ProcessTerminalData(string data)
//		{
//			int escPos = data.IndexOf((char)27); // ESC sequence number (0x1B)

//			while (escPos >= 0)
//			{
//				if (data.Substring(escPos + 1).StartsWith("[c")) // Identify what terminal type
//				{
//					this.SendAsync("\x1B[?1;0c"); // 0x1B is included
//					this.isTerminalTypeSent = true;
//				}
//				else if (data.Substring(escPos + 1).StartsWith("[5n")) // Identify terminal status
//				{
//					this.SendAsync("\x1B[0n"); // Terminal status OK
//				}
//				else if (data.Substring(escPos + 1).StartsWith("[6n")) // Identify cursor position
//				{
//					if (!isTerminalTypeSent)
//					{
//						this.SendAsync("\x1B[?1;0c"); // 0x1B is included
//						this.isTerminalTypeSent = true;
//					}

//					this.SendAsync("\x1B[120;80R");
//				}

//				escPos = data.IndexOf((char)27, escPos + 1); // ESC sequence number (0x1B)
//			}

//			return TerminalHelper.RemoveEscapeSequences(data);
//		}

//		protected string ProcessTerminalData2(string data)
//		{
//			StringBuilder result = new StringBuilder();
//			char[] chars = data.ToCharArray();

//			bool isSequence = false;
//			int index = 0;

//			char beginChar = '\0';
//			char endChar = '\0';

//			while (index < chars.Length)
//			{
//				if (chars[index] == (char)27) // ESC sequence number (0x1B)
//				{
//					isSequence = true;
//					index++;

//					if (index == chars.Length)
//						break;

//					switch (chars[index])
//					{

//						//Codes for use in VT52 compatibility mode

//						case 'A':

//							this.OnEscapeSequenceCursorUp();
//							break;

//						case 'B':

//							this.OnEscapeSequenceCursorDown();
//							break;

//						case 'C':

//							this.OnEscapeSequenceCursorRight();
//							break;

//						case 'D':

//							this.OnEscapeSequenceCursorLeft();
//							break;

//						case 'F':

//							this.OnEscapeSequenceEnterGraphicsMode();
//							break;

//						case 'G':

//							this.OnEscapeSequenceExitGraphicsMode();
//							break;

//						case 'H':

//							this.OnEscapeSequenceCursorToHome();
//							break;

//						case 'I':

//							this.OnEscapeSequenceReverseLineFeed();
//							break;

//						case 'J':

//							this.OnEscapeSequenceEraseToEndOfScreen();
//							break;

//						case 'K':

//							this.OnEscapeSequenceEraseToEndOfLine();
//							break;

//						case 'Y':

//							index++;

//							if (index == chars.Length)
//								break;

//							int line = chars[index] - 31;

//							index++;

//							if (index == chars.Length)
//								break;

//							int column = chars[index] - 31;

//							this.OnEscapeSequenceDirectCursorAddress(line, column);
//							break;

//						case 'Z':

//							this.OnEscapeSequenceIdentify();
//							break;

//						case '=':

//							this.OnEscapeSequenceEnterAlternateKeypadMode();
//							break;

//						case '>':

//							this.OnEscapeSequenceExitAlternateKeypadMode();
//							break;

//						case '<':

//							this.OnEscapeSequenceEnterAnsiMode();
//							break;


//						// ANSI Compatible Mode
//						// Cursor Movement Commands

//						default:
//							break;
//					}

//					{
//						beginChar = chars[index + 1];

//						// Find sequence with single character
//						string beginEnd = beginChar.ToString() + beginChar.ToString();
//						bool sequenceExists = true; // TerminalHelper.SequencesByBeginEndCharacters.Contains(beginEnd);

//						if (sequenceExists)
//						{
//							isSequence = false;
//						}

//						index = index + 2;

//						continue;
//					}
//				}

//				if (isSequence)
//				{
//					endChar = chars[index];

//					string beginEnd = beginChar.ToString() + endChar.ToString();
//					bool sequenceExists = true; // TerminalHelper.SequencesByBeginEndCharacters.Contains(beginEnd);

//					if (sequenceExists)
//					{
//						isSequence = false;
//						index++;
//						continue;
//					}
//				}
//				else
//				{
//					result.Append(chars[index]);
//				}

//				index++;
//			}

//			return TerminalHelper.RemoveEscapeSequences(data);
//		}

//		//protected virtual string GetCommandOnSeparateReceivedTerminalData(string receivedFirstLine, string sentCommand)
//		//{
//		//	return sentCommand;
//		//}

//		protected virtual TerminalDataResponse SeparateReceivedTerminalData(string terminalData, string sentCommand)
//		{
//			string command = sentCommand;
//			string dataBody = String.Empty;
//			string promptLine = String.Empty;
//			bool isPreviusLineMorePrompt = false;
//			string[] lines = terminalData.ToLines(StringSplitOptions.None);

//			for (int i = 0; i < lines.Length; i++)
//			{
//				bool appendLine = true;
//				string line = lines[i];

//				if (line.ContainsAny(this.MorePromptList, ignoreCase: true))
//				{
//					appendLine = false;
//					isPreviusLineMorePrompt = true;
//				}

//				// If previous line had more prompt and this line is empty - nothing to appending
//				if (appendLine && isPreviusLineMorePrompt)
//				{
//					if (line.Trim().Length == 0)
//						appendLine = false;

//					isPreviusLineMorePrompt = false;
//				}

//				// If line is last, has no false privilege mode prompts and contain privilege mode prompt - no appending
//				if (appendLine && i == lines.Length - 1 && !line.ContainsAny(this.falsePrivilegeModePromptList, ignoreCase: true) && line.ContainsAny(this.PrivilegeModePromptList, ignoreCase: true))
//				{
//					appendLine = false;
//					promptLine = line;
//				}

//				if (appendLine)
//				{
//					dataBody += line;

//					if (i < lines.Length - 1)
//						dataBody += "\r\n";
//				}
//			}

//			return new TerminalDataResponse(command, dataBody, promptLine);
//		}

//		#endregion |   Protected Methods   |

//		#region |   Terminal Escape Sequence Handling   |

//		/// <summary>
//		/// EscA: Move the active position upward one position without altering the horizontal position. If an attempt is made to move the cursor above the top margin, the cursor stops at the top margin.
//		/// </summary>
//		protected virtual void OnEscapeSequenceCursorUp()
//		{
//		}

//		/// <summary>
//		/// EscB: Move the active position downward one position without altering the horizontal position.If an attempt is made to move the cursor below the bottom margin, the cursor stops at the bottom margin.
//		/// </summary>
//		protected virtual void OnEscapeSequenceCursorDown()
//		{
//		}

//		/// <summary>
//		/// EscC: Move the active position to the right. If an attempt is made to move the cursor to the right of the right margin, the cursor stops at the right margin.
//		/// </summary>
//		protected virtual void OnEscapeSequenceCursorRight()
//		{
//		}

//		/// <summary>
//		/// EscD: Move the active position one position to the left. If an attempt is made to move the cursor to the left of the left margin, the cursor stops at the left margin.
//		/// </summary>
//		protected virtual void OnEscapeSequenceCursorLeft()
//		{
//		}

//		/// <summary>
//		/// EscF: Causes the special graphics character set to be used.
//		/// NOTE: The special graphics characters in the VT100 are different from those in the VT52.
//		/// </summary>
//		protected virtual void OnEscapeSequenceEnterGraphicsMode()
//		{
//		}

//		/// <summary>
//		/// EscG: This sequence causes the standard ASCII character set to be used.
//		/// </summary>
//		protected virtual void OnEscapeSequenceExitGraphicsMode()
//		{
//		}

//		/// <summary>
//		/// EscH: Move the cursor to the home position.
//		/// </summary>
//		protected virtual void OnEscapeSequenceCursorToHome()
//		{
//		}

//		/// <summary>
//		/// EscI: Move the active position upward one position without altering the column position.If the active position is at the top margin, a scroll down is performed.
//		/// </summary>
//		protected virtual void OnEscapeSequenceReverseLineFeed()
//		{
//		}

//		/// <summary>
//		/// EscJ: Erase all characters from the active position to the end of the screen. The active position is not changed.
//		/// </summary>
//		protected virtual void OnEscapeSequenceEraseToEndOfScreen()
//		{
//		}

//		/// <summary>
//		/// EscK: Erase all characters from the active position to the end of the current line.The active position is not changed.
//		/// </summary>
//		protected virtual void OnEscapeSequenceEraseToEndOfLine()
//		{
//		}

//		/// <summary>
//		/// EscYLineColumn: Move the cursor to the specified line and column. The line and column numbers are sent as ASCII codes whose values are the number plus \037; 
//		/// e.g., \040 refers to the first line or column, \050 refers to the eighth line or column, etc.
//		/// </summary>
//		protected virtual void OnEscapeSequenceDirectCursorAddress(int line, int column)
//		{
//		}

//		/// <summary>
//		/// EscZ: This sequence causes the terminal to send its identifier escape sequence to the host.This sequence is: Esc/Z
//		/// </summary>
//		protected virtual void OnEscapeSequenceIdentify()
//		{
//			this.SendAsync("\033/Z");
//		}

//		/// <summary>
//		/// Esc=: The optional auxiliary keypad keys will send unique identifiable escape sequences for use by applications programs.
//		/// NOTE: Information regarding options must be obtained in ANSI mode, using the device attributes(DA) control sequences.
//		/// </summary>
//		protected virtual void OnEscapeSequenceEnterAlternateKeypadMode()
//		{
//		}

//		/// <summary>
//		/// Esc&lt;: The optional auxiliary keypad keys send the ASCII codes for the functions or characters engraved on the key.
//		/// </summary>
//		protected virtual void OnEscapeSequenceExitAlternateKeypadMode()
//		{
//		}

//		/// <summary>
//		/// Esc&gt;: All subsequent escape sequences will be interpreted according to ANSI Standards X3.64-1977 and X3.41-1974. 
//		/// The VT52 escape sequence designed in this section will not be recognized.
//		/// </summary>
//		protected virtual void OnEscapeSequenceEnterAnsiMode()
//		{
//		}

//		#endregion |   Terminal Escape Sequence Handling   |

//		#region |   Private Raise Events Methods   |

//		private void RaiseConnected()
//		{
//			this.Connected?.Invoke(this, new EventArgs());
//		}

//		private void RaiseDisconnected()
//		{
//			this.Disconnected?.Invoke(this, new EventArgs());
//		}

//		private void RaiseTextSending(string text)
//		{
//			this.TextSending?.Invoke(text);
//		}

//		private void RaiseTextReceived(string text)
//		{
//			this.TextReceived?.Invoke(text);
//		}

//		#endregion |   Private Raise Events Methods   |

//		#region |   Private Methods   |

//		private void SetProviderControl()
//		{
//			if (this.providerControl != null)
//			{
//				this.ProviderControl.TextReceived -= new TextEventHandler(ProviderControl_TextReceived);
//				(this.ProviderControl as IDisposable).Dispose();
//			}

//			switch (this.TerminalProtocol)
//			{
//				case TerminalProtocol.Telnet:

//					switch (this.TelnetProviderType)
//					{
//						case TelnetProviderType.TelnetPipeClient:

//							this.providerControl = new TelnetPipeClient(); // TelnetControlSimpleTelnet();
//							break;

//						case TelnetProviderType.TelnetSocketClient:

//							this.providerControl = new TelnetSocketClient(); // TelnetControlSimpleTelnet();
//							break;

//						case TelnetProviderType.ThoughtNetTelnet:

//							this.providerControl = new TelnetControlThoughtNetTelnet();
//							break;

//						default: throw new ArgumentException("For the Telnet Provider Type " + this.TelnetProviderType.ToString() + " provider class is not specified.");
//					}

//					break;

//				case TerminalProtocol.SSH1:

//					switch (this.Ssh1ProviderType)
//					{
//						case Ssh1ProviderType.SshGranados:

//							this.providerControl = new Ssh1ControlSshGranados();
//							break;

//						default: throw new ArgumentException("For the SSH1 Provider Type " + this.Ssh1ProviderType.ToString() + " provider class is not specified.");
//					}

//					break;

//				case TerminalProtocol.SSH2:

//					switch (this.Ssh2ProviderType)
//					{
//						case Ssh2ProviderType.SshNet:

//							this.providerControl = new Ssh2ControlSshNet();
//							break;

//						default: throw new ArgumentException("For the SSH2 Provider Type " + this.Ssh2ProviderType.ToString() + " provider class is not specified.");
//					}

//					break;

//				default: throw new ArgumentException("For the Terminal Protocol " + this.TerminalProtocol.ToString() + " provider class is not specified.");
//			}

//			if (this.ProviderControl != null)
//				this.ProviderControl.TextReceived += new TextEventHandler(ProviderControl_TextReceived);
//		}

//		//private bool isMoreDetected = false;

//		//protected virtual string[] SplitToLines(string processedReceivedData)
//		//{
//		//	return processedReceivedData.SplitToLines();
//		//}

//		protected virtual bool RemoveCommandFromReceivedDataIfExists(string receivedData, string sentCommand, out string result)
//		{
//			bool isRemoved = false;
//			result = receivedData;

//			if (receivedData.IsNullOrEmpty() || sentCommand.IsNullOrEmpty())
//				return false;

//			if (receivedData.Length == 0 || sentCommand.Length == 0)
//				return false;

//			//string sentCommandWithoutSplitters = sentCommand.Replace("\r\n", "").Replace("\n\r", "").Replace("\r", "").Replace("\b", "").Replace("\n", "").Trim();

//			if (sentCommand.Length > 0)
//			{
//				int startIndex = receivedData.IndexOf(sentCommand);

//				if (startIndex >= 0)
//				{
//					result = receivedData.Substring(startIndex + sentCommand.Length);
//					isRemoved = true;
//				}
//			}

//			return isRemoved;
//		}

//		private void ProviderControl_TextReceived(string text)
//		{
//			const string strPressAnyKey = "Press any key";
//			this.ProviderControl.AcceptData = false;

//			//			Debug.WriteLine("RECEIVED: " + e.Text);

//			//this.receivingBuffer += e.Text;
//			//this.receivingBuffer = TerminalHelper.RemoveEscapeSequences(this.receivingBuffer);
//			TerminalConnectionState oldConnectionStatus = this.ConnectionStatus;

//			if (this.timeoutTimer != null && this.timeoutTimer.Enabled)
//			{
//				this.timeoutTimer.Stop();
//				this.timeoutTimer.Start(); // restart the timer;
//			}

//			//if (!this.isMoreDetected && !this.lastSendingText.IsNullOrEmpty() && this.receivingBuffer.StartsWith(this.lastSendingText))
//			//{
//			//    this.receivedData = this.receivingBuffer.Substring(this.lastSendingText.Length, this.receivingBuffer.Length - this.lastSendingText.Length);
//			//    this.receivingBuffer = String.Empty;
//			//}
//			//else
//			//{
//			//this.receivedData += e.Text;

//			//if (!this.isMoreDetected && !this.lastSendingText.IsNullOrEmpty() && this.receivingBuffer.StartsWith(this.lastSendingText))
//			//{
//			//    this.receivedData = this.receivingBuffer.Substring(this.lastSendingText.Length, this.receivingBuffer.Length - this.lastSendingText.Length);
//			//    this.receivingBuffer = String.Empty;
//			//}
//			//}

//			//this.isMoreDetected = false;

//			string processedTerminalData = this.ProcessTerminalData(text); //  TerminalHelper.RemoveEscapeSequences(this.receivedData);
//			this.processedReceivedData += processedTerminalData;

//			if (this.AutoLogin && this.ConnectionStatus != TerminalConnectionState.Authenticated && this.ConnectionStatus != TerminalConnectionState.Disconnected)
//			{
//				bool resetProcessedReceivedData = false;

//				if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt)
//				{
//					if (this.processedReceivedData.Contains(strPressAnyKey, ignoreCase: true))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						string textToSend = "\r\n";
//						this.SendAsync(textToSend);
//					}
//					else if (this.processedReceivedData.ContainsAny(this.UsernamePromptList, ignoreCase: true))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt);
//						String textToSend = String.Format("{0}\r\n", this.Username);
//						this.SendAsync(textToSend);
//					}
//					else if (this.processedReceivedData.ContainsAny(this.PasswordPromptList, ignoreCase: true))
//					{
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt);
//					}
//				}

//				if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt)
//				{
//					if (this.processedReceivedData.ContainsAny(this.PasswordPromptList, ignoreCase: true))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt);
//						this.connectionLog += "**********";
//						this.SendAsync(String.Format("{0}\r\n", this.Password));
//					}
//				}

//				// In the case of the ssh connection authorization can be automated and you will get command prompt at first
//				if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt) //||
//																											  //this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForPasswordPrompt)
//				{
//					if (this.processedReceivedData.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressReadyToBeConnected);
//					}
//					else if (this.processedReceivedData.EndsWithAny(this.NonPrivilegeModePromptList, trim: true, ignoreCase: true))
//					{
//						this.terminalConnectionStateStatus = TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt;
//					}
//				}

//				if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt)
//				{
//					if (this.processedReceivedData.ContainsAny(this.NonPrivilegeModePromptList, ignoreCase: true)) // EndsWithAny(this.NonPrivilegeModePromptList, true, false))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;

//						if (this.processedReceivedData.EndsWithAny(this.NonPrivilegeModePromptList, trim: true, ignoreCase: true))
//						{
//							this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressWaitingForEnableSecretPasswordPrompt);
//							String textToSend = String.Format("{0}\r\n", this.PrivilegeModeCommand);
//							//this.connectionLog += textToSend;
//							this.SendAsync(textToSend);
//						}
//						else
//						{
//							this.SendAsync("\r\n");
//						}
//					}
//					else if (this.processedReceivedData.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressReadyToBeConnected);
//					}

//					//if (!isAsteriskInsertedAsPassword)
//					//{
//					//    this.connectionLog += String.Format("{0}\r\n", "********");
//					//    isAsteriskInsertedAsPassword = true;
//					//}
//				}
//				else if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressWaitingForEnableSecretPasswordPrompt)
//				{
//					if (this.processedReceivedData.ContainsAny(this.EnableSecretPromptList, true))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressAfterEnableSecretIsSent);
//						this.connectionLog += "**********";
//						this.SendAsync(String.Format("{0}\r\n", this.EnableSecret));
//						//this.isAsteriskInsertedAsEnableSecretPassword = false;
//					}
//					else if (this.processedReceivedData.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressReadyToBeConnected);
//					}
//				}
//				else if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressAfterEnableSecretIsSent)
//				{
//					//if (!isAsteriskInsertedAsEnableSecretPassword)
//					//{
//					//    this.connectionLog += String.Format("{0}\r\n", "********");
//					//    isAsteriskInsertedAsEnableSecretPassword = true;
//					//}

//					if (this.processedReceivedData.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//					{
//						this.connectionLog += this.processedReceivedData;
//						resetProcessedReceivedData = true;
//						this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.LoginInProgressReadyToBeConnected);
//					}
//				}

//				if (resetProcessedReceivedData)
//					this.processedReceivedData = String.Empty;
//			}

//			this.OnTerminalDataReceived(text);
//			this.RaiseTextReceived(text);

//			if (this.DoesTextEndsWithAnyMorePrompt(this.processedReceivedData, ignoreCase: true))
//			{
//				this.processedReceivedData = this.processedReceivedData.RemoveLastLine(StringSplitOptions.None); // this.RemoveLastLineInText(this.processedReceivedData) + "\r\n";
//				this.SendAsync(" "); // Send space to continue after -- More --
//			}

//			if (this.ConnectionStatus == TerminalConnectionState.LoginInProgressReadyToBeConnected)
//			{
//				TerminalDataResponse dataResponse = this.SeparateReceivedTerminalData(this.connectionLog, sentCommand: String.Empty);

//				this.PromptLine = dataResponse.PromptLine;
//				this.WriteToLog(this.connectionLog);

//				this.ChangeTerminalConnectionStateStatus(TerminalConnectionState.Authenticated);

//				this.response = this.connectionLog;
//				this.threadSync.Release(this.token); //, this.connectionLog); // privilege mode prompt is received -> release Send thread.
//				this.processedReceivedData = String.Empty;
//			}
//			else if (this.ConnectionStatus == TerminalConnectionState.Authenticated)
//			{

//				string processedReceivedDataWithoutSentCommand = String.Empty;
//				bool isCommandRemoved = this.RemoveCommandFromReceivedDataIfExists(this.processedReceivedData, this.lastSentSyncCommand, out processedReceivedDataWithoutSentCommand);

//				if (((this.waitFor == null || this.waitFor[0].IsNullOrEmpty()) && processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true)) ||
//					(this.waitFor != null && !this.waitFor[0].IsNullOrEmpty() && processedReceivedDataWithoutSentCommand.ContainsAny(this.waitFor, ignoreCase: true)))
//				{
//					TerminalDataResponse dataResponse = this.SeparateReceivedTerminalData(processedReceivedDataWithoutSentCommand, this.lastSentSyncCommand);

//					//this.PromptLine = dataResponse.PromptLine;
//					//this.WriteToLog(this.processedReceivedData);

//					if (isCommandRemoved)
//					{
//						this.WriteToLog(dataResponse.Command);
//						this.WriteToLog(dataResponse.DataBody);
//						this.WriteToLog(dataResponse.PromptLine);
//					}
//					else
//					{
//						this.WriteToLog(processedReceivedDataWithoutSentCommand);
//					}

//					this.response = dataResponse.DataBody;
//					this.threadSync.Release(this.token); //, dataResponse.DataBody); // privilege mode prompt is received -> release Send thread.
//					this.processedReceivedData = String.Empty;
//				}
//			}

//			this.ProviderControl.AcceptData = true;
//		}

//		private bool DoesTextEndsWithAnyPrivilegedModePrompt(string text)
//		{
//			string val = text.TrimEnd();

//			if (val.Length == 0)
//				return false;

//			foreach (string privilegeModePrompt in this.PrivilegeModePromptList)
//			{
//				if (text.TrimEnd().EndsWith(privilegeModePrompt))
//				{
//					this.privilegeModePrompt = privilegeModePrompt;
//					return true;
//				}
//			}

//			return false;
//		}

//		private bool DoesTextEndsWithAnyMorePrompt(string text, bool ignoreCase)
//		{
//			bool result = false;

//			if (text.Length > 0)
//			{
//				string[] lines = text.Split(new string[] { "\r\n", "\n\r", "\r", "\b" }, StringSplitOptions.RemoveEmptyEntries);

//				if (lines.Count() > 0)
//				{
//					string lastLine = lines[lines.Length - 1].Trim();
//					result = lastLine.ContainsAny(this.MorePromptList, ignoreCase);
//				}

//				//if (removeLastLine)
//				//{
//				//    bool isFirstPass = true;
//				//    text = String.Empty;

//				//    for (int i = 0; i < lines.Length - 2; i++)
//				//    {
//				//        if (!isFirstPass)
//				//            text += "\r\n";

//				//        text += lines[i];
//				//        isFirstPass = false;
//				//    }
//				//}
//			}

//			return result;
//		}


//		private void SetProviderControlSettings()
//		{
//			this.ProviderControl.RemoteHost = this.RemoteHost;
//			this.ProviderControl.RemotePort = this.RemotePort;

//			if (this.ProviderControl is ISshControl)
//			{
//				(this.ProviderControl as ISshControl).Username = this.Username;
//				(this.ProviderControl as ISshControl).Password = this.Password;
//			}
//		}

//		private void SetLocalSettings()
//		{
//			this.timeoutTimer.Interval = this.Timeout * 1000;
//		}

//		//private void SetLocalSettings()
//		//{
//		//    this.SetSendingTimerInterval();
//		//}

//		//private void SetSendingTimerInterval()
//		//{
//		//    this.sendingIntervalTimer.Interval = this.SendingInterval > 0 ? this.SendingInterval : 1;
//		//}

//		//private void sendingIntervalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//		//{
//		//    this.sendingIntervalTimeElapsed = true;

//		//    if (this.threadSync.ContainsToken(this.threadSendingIntervalToken))
//		//    {
//		//        this.threadSync.Release(this.threadSendingIntervalToken);
//		//    }
//		//}

//		private void WriteToLog(string text)
//		{
//			if (this.logging && this.LogTextWriter != null) // If log file cannot be opened or path not exists this.LogTextWriter will be null
//			{
//				this.LogTextWriter.Write(text);
//				this.LogTextWriter.Flush();
//			}
//		}

//		bool isTimeout = false;

//		private void timeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//		{
//			this.timeoutTimer.Stop();
//			this.timeoutTimer.Enabled = false;

//			this.isTimeout = true;
//			this.threadSync.Release(this.token);
//		}

//		private void DisposeControl()
//		{
//			if (this.providerControl != null)
//			{
//				(this.providerControl as IDisposable).Dispose();
//				this.providerControl = null;
//			}
//		}

//		#endregion |   Private Methods   |
//	}

//	#region |   Delegates   |

//	public delegate void ConnectedEventHandler(object sender, EventArgs e);
//	public delegate void DisconnectedEventHandler(object sender, EventArgs e);
//	//public delegate void TerminalTextEventHandler(object sender, TerminalTextEventArgs e);

//	#endregion |   Delegates   |


//	#region |   Event Args & Helper Classes   |

//	public struct TerminalDataResponse
//	{
//		public TerminalDataResponse(string command, string dataBody, string promptLine)
//		{
//			this.Command = command;
//			this.DataBody = dataBody;
//			this.PromptLine = promptLine;
//		}

//		public string Command { get; set; }
//		public string DataBody { get; set; }
//		public string PromptLine { get; set; }
//	}

//	//public class TerminalTextEventArgs : EventArgs
//	//   {
//	//	public TerminalTextEventArgs(string text)
//	//       {
//	//           this.Text = text;
//	//       }

//	//       public string Text { get; set; }
//	//   }

//	#endregion |   Event Args & Helper Classes   |

//	#region |   Interfaces   |

//	public interface ISshControl : ITelnetControl
//	{
//		string Username { get; set; }
//		string Password { get; set; }
//	}

//	public interface ITelnetControl
//	{
//		string RemoteHost { get; set; }
//		int RemotePort { get; set; }
//		//int Timeout { get; set; }
//		bool AcceptData { get; set; }
//		void Connect();
//		void Disconnect();
//		bool Connected { get; }
//		void SendAsync(string command);

//		event TextEventHandler TextReceived;
//	}

//	#endregion |   Interfaces   |
//}
