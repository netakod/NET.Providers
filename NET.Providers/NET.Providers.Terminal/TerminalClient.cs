using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Timers;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Telnet;

namespace NET.Tools.Terminal
{
	public class TerminalClient : IProviderConnection, IDisposable
    {
		#region |   Private Members   |

		private bool autoLogin = true;
		private TerminalProtocol terminalProtocol = TerminalProtocol.Telnet;
		private TelnetProviderType telnetProviderType = TelnetProviderType.TelnetPipeClient;
		private Ssh1ProviderType ssh1ProviderType = Ssh1ProviderType.SshGranados;
		private Ssh2ProviderType ssh2ProviderType = Ssh2ProviderType.SshNet;
		private string remoteHost = String.Empty;
		private int remotePort = 23;

		private string username = String.Empty;
		private string password = String.Empty;
		private string enableSecret = String.Empty;
		private int timeout = 10; // in seconds
		private int sendDelay = 40; // in milliseconds
		private string promptSeparator = "|";
		private string usernamePrompts = "login|username|user name|user";
		private string passwordPrompts = "password";
		private string enableSecretPrompts = "password";
		private string nonPrivilegeModePrompts = ">";
		private string privilegeModeCommand = "enable";
		private string morePrompts = "--More--|---- More ----|-- More --|More: <space>";
		private string privilegeModePrompts = "#";
		private string configModeCommand = "configure terminal";
		private string exitConfigModeCommand = "exit";
		private string vlanDatabaseConfigCommand = "vlan database";
		private string exitVlanDatabaseConfigCommand = "exit";
		private string interfaceConfigCommand = "interface";
		private string exitInterfaceConfigCommand = "exit";
		private string logoutCommand = "logout";
        private bool matchCase = false;

		private ITelnetClient provider;
		private List<string> usernamePromptList = null;
		private List<string> passwordPromptList = null;
		private List<string> enableSecretPromptList = null;
		private List<string> morePromptList = null;
		private List<string> nonPrivilegeModePromptList = null;
		private List<string> privilegeModePromptList = null;
		private List<string> falsePrivilegeModePromptList = new List<string>() { "[Y/N]:" };
        private string promptLine = String.Empty;
		private string privilegeModePrompt = String.Empty;
		//private System.Timers.Timer timeoutTimer = null;
		private string logFileName = String.Empty;
		private TextWriter logTextWriter = null;
		private bool isTerminalTypeSent = false;
		private string lastSentSyncCommand = String.Empty;
		private string response = String.Empty;
		private string receivedText = String.Empty;
		private ManualResetEvent logInManualResetEvent = new ManualResetEvent(false);
        private ManualResetEvent waitForManualResetEvent = new ManualResetEvent(false);
        //private ThreadSync<int> threadSync = new ThreadSync<int>();
        //private int token = 0;
        private CloseReason closeReason;

        //private int token = 0;
        private object owner = null;

        #endregion |   Private Members   |

        #region |   Protected Members   |

        protected bool logging = false;
        //protected string receivedData = String.Empty;
        protected string processedLogInReceivedData = String.Empty;
		protected string processedReceivedData = String.Empty;
		protected TerminalConnectionState connectionState = TerminalConnectionState.Disconnected;
		protected string[] waitFor = null;
		//protected ThreadSync<int> threadSync = new ThreadSync<int>();


		//protected ThreadSyncToken threadSyncToken = new ThreadSyncToken("Terminal");
		//protected string connectionMessage = String.Empty;
		//protected string lastSendingSyncCommand = String.Empty;
		protected string connectionLog = String.Empty;
		protected TerminalConfigMode configMode = TerminalConfigMode.NonConfigMode;
		protected string configInterfaceName = String.Empty;

		#endregion |   Protected Members   |

		#region |   Public Static Members   |

		public static TerminalClient Default = new TerminalClient();

		#endregion |   Public Static Members   |

		#region |   Constructors and Initialization   |

		public TerminalClient() 
        { 
        }

        #endregion |   Constructors and Initialization   |

        #region |   Events   |

        public event EndPointEventHandler Connected;
        public event AsyncAction Authenticated;
        public event CloseEventHandlerAsync Closed;
        public event Telnet.TextEventHandlerAsync TextReceived;
        public event Telnet.TextEventHandlerAsync TextSent;

        #endregion

        #region |   Private Properties   |

        //private TextWriter LogTextWriter
        //{
        //    get
        //    {
        //        if (this.logTextWriter == null)
        //        {

        //            try
        //            {
        //                this.logTextWriter = new StreamWriter(this.logFileName, append: false);

        //                //if (this.logging && !this.connectionMessage.IsNullOrEmpty())
        //                //{
        //                //	this.logTextWriter.Write(this.connectionMessage);
        //                //	this.logTextWriter.Flush();
        //                //}
        //            }
        //            catch
        //            {

        //            }
        //        }

        //        return this.logTextWriter;
        //    }
        //}

        #endregion |   Private Properties   |

        #region |   Public Properties   |

        public bool AutoLogin { get => this.autoLogin; set => this.autoLogin = value; }
 
        public TerminalProtocol TerminalProtocol { get => this.terminalProtocol; set => this.terminalProtocol = value; }
        
        public TelnetProviderType TelnetProviderType { get => this.telnetProviderType; set => this.telnetProviderType = value; }
        
        public Ssh1ProviderType Ssh1ProviderType { get => this.ssh1ProviderType; set => this.ssh1ProviderType = value; }
        
        public Ssh2ProviderType Ssh2ProviderType { get => this.ssh2ProviderType; set => this.ssh2ProviderType = value; }
        
        public string RemoteHost { get => this.remoteHost; set => this.remoteHost = value; }
        
        public int RemotePort { get => this.remotePort; set => this.remotePort = value; }

        public string Username
        {
            get => this.username; 
			set
			{
				if (value == null)
					value = String.Empty;

				this.username = value;
			}
        }

        public string Password
        {
            get => this.password;
			set
			{
				if (value == null)
					value = String.Empty;

				this.password = value;
			}
        }

        public string EnableSecret
        {
            get => this.enableSecret;
			set
			{
				if (value == null)
					value = String.Empty;

				this.enableSecret = value;
			}
        }

        /// <summary>
        /// Timeout in seconds.
        /// </summary>
        public int Timeout { get => this.timeout; set => this.timeout = value; }

        /// <summary>
        /// Sending delay in milliseconds. If zero, no delay when sending.
        /// </summary>
        public int SendDelay { get => this.sendDelay; set => this.sendDelay = value; }
        
		public TerminalConnectionState ConnectionState => this.connectionState;
		public bool IsAuthenticated => this.ConnectionState == TerminalConnectionState.Authenticated;

        public bool IsConnected => this.provider?.Connected ?? false;

        public bool Logging => this.logging;
        public string LogFileName => this.logFileName;

        public CloseReason CloseReason => this.closeReason;


        public string PromptSeparator
        {
            get => this.promptSeparator;
			set
            {
                this.promptSeparator = value;
                this.usernamePromptList = null;
                this.passwordPromptList = null;
                this.enableSecretPromptList = null;
                this.nonPrivilegeModePromptList = null;
                this.privilegeModePromptList = null;
                this.morePromptList = null;
            }
        }

        public string UsernamePrompts
        {
            get => this.usernamePrompts;
            set
            {
                this.usernamePrompts = value;
                this.usernamePromptList = null;
            }
        }

        public string PasswordPrompts
        {
            get => this.passwordPrompts;
            set
            {
                this.passwordPrompts = value;
                this.passwordPromptList = null;
            }
        }

        public string EnableSecretPrompts
        {
            get => this.enableSecretPrompts;
            set
            {
                this.enableSecretPrompts = value;
                this.enableSecretPromptList = null;
            }
        }

        public string NonPrivilegeModePrompts
        {
            get => this.nonPrivilegeModePrompts;
            set
            {
                this.nonPrivilegeModePrompts = value;
                this.nonPrivilegeModePromptList = null;
            }
        }

        public string PrivilegeModeCommand { get => this.privilegeModeCommand; set => this.privilegeModeCommand = value; }
        
        public string PrivilegeModePrompts
        {
            get => this.privilegeModePrompts;
            set
            {
                this.privilegeModePrompts = value;
                this.privilegeModePromptList = null;
            }
        }

		public string MorePrompts
		{
			get => this.morePrompts;
			set
			{
				this.morePrompts = value;
				this.morePromptList = null;
			}
		}

		public string ConfigModeCommand { get => this.configModeCommand; set => this.configModeCommand = value; }
        
        public string ExitConfigModeCommand { get => this.exitConfigModeCommand; set => this.exitConfigModeCommand = value; }
        

        public string VlanDatabaseConfigCommand { get => this.vlanDatabaseConfigCommand; set => this.vlanDatabaseConfigCommand = value; }
        
        public string ExitVlanDatabaseConfigCommand { get => this.exitVlanDatabaseConfigCommand; set => this.exitVlanDatabaseConfigCommand = value; }
        
        public string InterfaceConfigCommand { get => this.interfaceConfigCommand; set => this.interfaceConfigCommand = value; }
        
        public string ExitInterfaceConfigCommand { get => this.exitInterfaceConfigCommand; set => this.exitInterfaceConfigCommand = value; }
        
		public string LogoutCommand { get => this.logoutCommand; set => this.logoutCommand = value; }

		public string PromptLine { get => this.promptLine; set => this.promptLine = value; }

        public bool MatchCase { get => this.matchCase; set => this.matchCase = value; }

		public object Owner { get => this.owner; set => this.owner = value; }

		#endregion |   Public Properties   |

		#region |   Protected Properties   |

		protected ITelnetClient Provider
        {
            get
            {
                if (this.provider == null)
                    this.SetProvider();

                return this.provider;
            }
        }

        protected List<string> UsernamePromptList
        {
            get
            {
                if (this.usernamePromptList == null)
                    this.usernamePromptList = this.ConvertStringToPromptList(this.UsernamePrompts);

                return this.usernamePromptList;
            }
        }

        protected List<string> PasswordPromptList
        {
            get
            {
                if (this.passwordPromptList == null)
                    this.passwordPromptList = this.ConvertStringToPromptList(this.PasswordPrompts);

                return this.passwordPromptList;
            }
        }

        protected List<string> EnableSecretPromptList
        {
            get
            {
                if (this.enableSecretPromptList == null)
                    this.enableSecretPromptList = this.ConvertStringToPromptList(this.EnableSecretPrompts);

                return this.enableSecretPromptList;
            }
        }

        protected List<string> NonPrivilegeModePromptList
        {
            get
            {
                if (this.nonPrivilegeModePromptList == null)
                    this.nonPrivilegeModePromptList = this.ConvertStringToPromptList(this.NonPrivilegeModePrompts);

                return this.nonPrivilegeModePromptList;
            }
        }

        protected List<string> PrivilegeModePromptList
        {
            get
            {
                if (this.privilegeModePromptList == null)
                    this.privilegeModePromptList = this.ConvertStringToPromptList(this.PrivilegeModePrompts);

                return this.privilegeModePromptList;
            }
        }

        protected List<string> MorePromptList
        {
            get
            {
                if (this.morePromptList == null)
                    this.morePromptList = this.ConvertStringToPromptList(this.MorePrompts);

                return this.morePromptList;
            }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        public virtual async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
        {
			await this.InitializeConnection();

            if (cancellationToken == default || cancellationToken == CancellationToken.None)
            {
                var tokenSource = new CancellationTokenSource();
                int timeOutInMilliseconds = this.Timeout * 1000;

                if (this.AutoLogin)
                    timeOutInMilliseconds *= 2;  // Give double time when additionally doing login procedure

				tokenSource.CancelAfter(timeOutInMilliseconds);
                cancellationToken = tokenSource.Token;
            }

            IPEndPoint remoteEndpoint = new IPEndPoint(DnsHelper.ResolveIPAddressFromHostname(this.RemoteHost), this.RemotePort);
            bool isConnected = await this.TryConnectAsync(remoteEndpoint, cancellationToken);

			if (isConnected)
			{
				//this.logInManualResetEvent.Close();
				//this.waitForManualResetEvent.Close();

				if (cancellationToken != default && cancellationToken.IsCancellationRequested)
                    return;

                await this.OnConnected(remoteEndpoint);
                await this.RaiseConnected(remoteEndpoint);

                if (this.AutoLogin)
                {
                    this.logInManualResetEvent.Reset();
                    await this.logInManualResetEvent.WaitOneAsync(cancellationToken);
                }

				if (this.IsAuthenticated)
				{
					await this.OnAuthenticated();
					await this.RaiseAuthenticated();
				}
				
     //           if (this.AutoLogin)
					//await this.LogInAsync(cancellationToken);
			}
			else
			{
                throw new Exception("Error connecting to remote endpoint");
			}
		}

        protected virtual async ValueTask<bool> TryConnectAsync(IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
		{
            var connection = this.Provider.ConnectAsync(remoteEndpoint, cancellationToken);
            
            await connection;

            return connection.IsCompletedSuccessfully;
        }

        //public async ValueTask TestConnectionAsync(WorkerContext workerContext) => await this.TestConnectionAsync(workerContext, cancellationToken: CancellationToken.None);

        public async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
		{
			string testResultMessage = "Terminal Connection Test: ";
            TaskResultInfo resultInfo = TaskResultInfo.Succeeded;

			if (workerContext != null)
				workerContext.ReportProgress(-1, "Testing Terminal connection...");

            // TODO: Integrate CancelationToken into WorkerContext to be easly passed to the methods
            //await this.ConnectAsync(cancellationToken);

            try
            {
                await this.CloseAsync(CloseReason.LocalClosing);

                if (workerContext.ShouldCancel())
                    return new TaskInfo<string>(testResultMessage, TaskResultInfo.Cancelled);

                await this.ConnectAsync();

				testResultMessage += this.connectionLog; // processedReceivedData;
			}
			catch (Exception ex)
            {
                testResultMessage += ex.Message;
                testResultMessage += this.receivedText;
				resultInfo = TaskResultInfo.ConnectionError;
                await this.WriteToLogAsync(this.receivedText);
                //return new TaskInfo<string>(testResultMessage, TaskResultInfo.Cancelled, ex.Message);
            }
            finally
            {
                if (workerContext != null)
                {
                    workerContext.Result = testResultMessage;
                    workerContext.Message = testResultMessage;
                }

				await this.CloseAsync(CloseReason.LocalClosing);
			}

			return new TaskInfo<string>(testResultMessage, resultInfo, testResultMessage);
        }

        protected async ValueTask InitializeConnection()
        {
            //this.connectionMessage = String.Empty;

            await this.DisposeProvider();

            this.SetProvider();
            this.SetProviderSettings();

            if (this.logging)
                await this.SetLoggingAsync(this.logFileName);
            //this.SetLocalSettings();

            this.connectionState = TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt;
        }

      //  protected async ValueTask<IRequestResult> TryConnect(IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
      //  {
		    //try
		    //{
			   // await this.Provider.ConnectAsync(remoteEndPoint, cancellationToken);
		    //}
		    //catch (Exception ex)
		    //{
			   // return new RequestResult<bool>(this.IsConnected, RequestResultInfo.ExceptionIsCaught, ex.GetFullErrorMessage());
		    //}

		    //return new RequestResult<bool>(this.IsConnected, RequestResultInfo.RequestSucceeded);
      //  }
        
   //     protected async ValueTask<bool> LogInAsync(CancellationToken cancellationToken)
   //     {
   //         this.logInManualResetEvent.WaitOne(cancellationToken);

			//if (this.IsAuthenticated)
   //         {
   //             await this.OnAuthenticated();
   //             await this.RaiseAuthenticated();
   //         }

   //         return this.IsAuthenticated;
   //     }

        public virtual async ValueTask CloseAsync(CloseReason closeReason)
        {
            this.closeReason = closeReason;

            if (this.IsConnected)
			{
                if (this.IsAuthenticated)
                {
                    await this.ExitConfigModeAsync();

                    string logoutCmd = this.LogoutCommand ?? "logout";
                    
                    await this.SendAsyncInternal(logoutCmd);
                    await this.WriteToLogAsync(logoutCmd);
                }

                try
                {
                    await this.Provider.CloseAsync(closeReason);
                }
                finally
                {
                    this.connectionState = TerminalConnectionState.Disconnected;

                    await this.OnClosed();
                    await this.RaiseClosed();

                    this.logInManualResetEvent.Close();
                    //this.waitForManualResetEvent.Close();
                }
            }

            //this.logInManualResetEvent.Close();
            this.lastSentSyncCommand = String.Empty;
            this.processedReceivedData = String.Empty;
            this.connectionLog = String.Empty;
            this.isTerminalTypeSent = false;

            if (this.logTextWriter != null)
            {
                await this.logTextWriter.FlushAsync();
                this.logTextWriter.Close();
                await this.logTextWriter.DisposeAsync();
                this.logTextWriter = null;
            }

            this.provider = null;
        }

        //public async ValueTask<string> SendLineAsync(string text, bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
        //{
        //    return await this.Provider.SendAsync(text + "\r\n", ignoreCase, cancellationToken, waitFor);
        //}
        public async ValueTask<string> SendAsync(string text)
        {
            return await this.SendAsync(text, sendCrLf: true);
        }

        public async ValueTask<string> SendAsync(string text, bool sendCrLf)
        {
            return await this.SendAsync(text, sendCrLf, ignoreCase: false);
        }

        public async ValueTask<string> SendAsync(string text, params string[] waitFor)
        {
            return await this.SendAsync(text, sendCrLf: true, ignoreCase: false, waitFor);
        }

        public async ValueTask<string> SendAsync(string text, bool sendCrLf, params string[] waitFor)
        {
            return await this.SendAsync(text, sendCrLf, ignoreCase: false, waitFor);
        }

        public async ValueTask<string> SendAsync(string text, bool sendCrLf, bool ignoreCase, params string[] waitFor)
        {
            return await this.SendAsync(text, sendCrLf, cancellationToken: default, ignoreCase: false, waitFor);
        }

        public async ValueTask<string> SendAsync(string text, bool sendCrLf = true, CancellationToken cancellationToken = default, bool ignoreCase = false, params string[] waitFor)
        {
            if (!this.IsConnected)
			{
                await this.ConnectAsync(cancellationToken);

                if (!this.IsConnected)
                    return null;

                //if (this.AutoLogin) // && this.IsAuthenticated) // Wait for automated authorization process to complete
                //{
                //    this.waitForManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls
                //    this.waitForManualResetEvent.WaitOne(cancellationToken);
                //}

                //this.threadSync.WaitFor(this.token, this.Timeout * 1000 + 10);
            }

            if (cancellationToken.IsCancellationRequested)
                return null;
            
            string result = String.Empty;
            string command = (sendCrLf) ? text + "\r\n" : text;

            await this.OnTextSending(command);
            await this.RaiseTextSending(command);



			//if (waitFor.Length == 0)
			//this.threadSync.Prepare(this.token); // this.waitForManualResetEvent.Close();

			//await this.SendAsyncInternal(command, cancellationToken).ConfigureAwait(false);
			


			//System.Diagnostics.Debug.WriteLine("SEND: " + command);


            if (cancellationToken.IsCancellationRequested)
                return result;

            result = this.response;
			this.lastSentSyncCommand = command;
			this.response = String.Empty;
            this.waitFor = (waitFor != null &&  waitFor.Length > 0) ? waitFor : null;

            //if (waitFor.Length > 0)
            //    this.waitFor = waitFor;
			
            _ = this.SendAsyncInternal(command, cancellationToken);
            
            this.waitForManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls
            await this.waitForManualResetEvent.WaitOneAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return result;

			result = this.response;
			this.response = String.Empty;
			this.waitFor = null;
			//this.waitForManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls

			//if (waitFor.Length == 0)
   //         {
   //             //this.waitFor = waitFor;

   //             //await this.waitForManualResetEvent.WaitOneAsync(this.Timeout * 1000, cancellationToken);

   //             result = this.response;

   //             this.response = String.Empty;
   //             this.waitFor = null;
   //             this.waitForManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls


   //             //System.Diagnostics.Debug.WriteLine("RECEIVED: " + result);
   //         }
   //         else
   //         {
   //             //_ = this.Provider.SendAsync(command, ignoreCase, cancellationToken).ConfigureAwait(false);
   //             // await this.waitForManualResetEvent.WaitOneAsync(cancellationToken);

   //             result = await this.Provider.WaitForAsync(ignoreCase, cancellationToken, waitFor);

   //             //result = this.response;
   //             //this.response = String.Empty;
   //             ////response = this.filteredReceivedDataWithoutEscapeSequences;
   //             //this.waitFor = null;
   //         }


            return result;
        }

		public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (this.SendDelay > 0)
                await Task.Delay(this.SendDelay);

            await this.Provider.SendAsync(buffer, cancellationToken);
        }

        public async ValueTask<string> WaitFor(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
            return await this.Provider.WaitForAsync(ignoreCase, cancellationToken, waitForText);
		}


        //     public string Send(string command)
        //     {
        //         return this.Send(command, true);
        //     }

        //     public string Send(string command, bool sendCrLf)
        //     {
        //         return this.Send(command, sendCrLf, null);
        //     }

        //     public string Send(string command, params string[] waitFor)
        //     {
        //         return this.Send(command, true, waitFor);
        //     }

        //     public string Send(string command, bool sendCrLf, params string[] waitFor)
        //     {
        //string result = String.Empty;
        ////this.receivingBuffer = String.Empty;

        //if (!this.IsConnected)
        //{
        //	IRequestResult connectResult = this.Connect();

        //	//if (!connectResult.Succeeded)
        //	//	return result;
        //}

        //string commandToSend = sendCrLf ? String.Format("{0}\r\n", command) : command;
        //         this.waitFor = waitFor;
        //this.lastSentSyncCommand = commandToSend;
        //this.SendAsync(commandToSend);

        //this.threadSync.WaitFor(this.token); //, this.Timeout * 1000);

        //this.timeoutTimer.Stop();
        //         this.timeoutTimer.Enabled = false;

        //result = this.response;
        //this.response = String.Empty;
        //         //response = this.filteredReceivedDataWithoutEscapeSequences;

        //         //this.sendingCommand = String.Empty;
        //         this.waitFor = null;

        //         if (this.isTimeout)
        //         {
        //             this.isTimeout = false;
        //             throw new TimeoutException("Terminal connection timeout.");
        //         }

        //         return result;
        //     }

        //public void SendAsync(string command)
        //{
        //    this.SendAsync(command, null);
        //}

        //       public void SendAsync(string command) //, params string[] waitFor)
        //       {
        //		if (command.IsNullOrEmpty())
        //			throw (new Exception("TerminalControl.SendAsync cannot send null or empty string."));

        //		this.OnTerminalDataSending(command);

        ////Debug.WriteLine("SENT: " + command);

        //           //if (waitFor != null)
        //           //this.waitFor = waitFor;

        //           if (this.SendingInterval > 0)
        //           {
        //               //// Wait for time to elapse between two sends
        //               //if (!this.sendingIntervalTimeElapsed)
        //               //{
        //               //    this.threadSync.WaitFor(this.threadSendingIntervalToken);
        //               //}

        //               //// Fire sendingIntervalTimer to prevent sending another data without pausing between two sendings.
        //               //this.sendingIntervalTimeElapsed = false;
        //               //this.sendingIntervalTimer.Start();

        //               Thread.Sleep(this.SendingInterval);
        //           }

        //		this.threadSync.Prepare(this.token);

        //           //if (this.IsConnected)
        //           //    this.WriteToLog(command);

        //           this.timeoutTimer.Stop();
        //           this.timeoutTimer.Enabled = true;
        //           this.timeoutTimer.Start();

        //           this.Provider.SendAsync(command);
        //       }

        public virtual async ValueTask EnterConfigModeAsync()
        {
            if (this.configMode != TerminalConfigMode.ConfigMode)
            {
                switch (this.configMode)
                {
                    case TerminalConfigMode.VlanDatabaseConfig:
                        
                        await this.ExitVlanDatabaseConfigAsync();
                        await this.SendAsync(this.ConfigModeCommand);
                        
                        break;

                    case TerminalConfigMode.NonConfigMode:
                        
                        await this.SendAsync(this.ConfigModeCommand);
                        
                        break;

                    case TerminalConfigMode.InterfaceConfig:
                        
                        await this.ExitInterfaceConfigAsync();
                        
                        break;
                }

                this.configMode = TerminalConfigMode.ConfigMode;
            }
        }

        public virtual async ValueTask ExitConfigModeAsync()
        {
            if (this.configMode != TerminalConfigMode.NonConfigMode)
            {
                switch (this.configMode)
                {
                    case TerminalConfigMode.ConfigMode:
                        
                        await this.SendAsync(this.ExitConfigModeCommand);
                        
                        break;

                    case TerminalConfigMode.VlanDatabaseConfig:
                        
                        await this.ExitVlanDatabaseConfigAsync();
                        
                        break;

                    case TerminalConfigMode.InterfaceConfig:
                        
                        await this.ExitInterfaceConfigAsync();
                        await this.SendAsync(this.ExitConfigModeCommand);
                        
                        break;
                }

                this.configMode = TerminalConfigMode.NonConfigMode;
            }
        }

        public virtual async ValueTask EnterInterfaceConfigAsync(string interfaceName)
        {
            if (this.configMode == TerminalConfigMode.InterfaceConfig && this.configInterfaceName == interfaceName)
                return;
            
            switch (this.configMode)
            {
                case TerminalConfigMode.NonConfigMode:
                    
                    await this.EnterConfigModeAsync();
                    
                    break;

                case TerminalConfigMode.InterfaceConfig:
                    
                    await this.ExitInterfaceConfigAsync();
                    
                    break;

                case TerminalConfigMode.VlanDatabaseConfig:
                    
                    await this.ExitVlanDatabaseConfigAsync();
                    await this.EnterConfigModeAsync();
                    
                    break;
            }

            await this.EnterInterfaceConfigInternalAsync(interfaceName);
            
            this.configMode = TerminalConfigMode.InterfaceConfig;
            this.configInterfaceName = interfaceName;
        }

        public virtual async ValueTask EnterVlanDatabaseConfigAsync()
        {
            string response = String.Empty;

            if (this.configMode != TerminalConfigMode.VlanDatabaseConfig)
            {
                await this.ExitConfigModeAsync();
                await this.SendAsync(this.VlanDatabaseConfigCommand);
                this.configMode = TerminalConfigMode.VlanDatabaseConfig;
            }
        }

        public void SetLogging(string logFileName) => this.SetLoggingAsync(logFileName).GetAwaiter().GetResult();

        private async ValueTask SetLoggingAsync(string logFileName)
        {
            if (this.logTextWriter != null && this.logFileName == logFileName)
                return;

            if (this.logTextWriter != null)
            {
                await this.logTextWriter.FlushAsync();
                this.logTextWriter.Close();
                await this.logTextWriter.DisposeAsync();
                this.logTextWriter = null;
            }

            this.logFileName = logFileName;
			this.logging = !logFileName.IsNullOrEmpty();

            try
            {
                this.logTextWriter = new StreamWriter(logFileName, append: false);
            }
            catch
			{
                this.logging = false;
			}
        }

        public virtual ValueTask FinishUpdateAsync() => new ValueTask();

        public void Dispose() => this.DisposeAsync().GetAwaiter().GetResult();

        public async ValueTask DisposeAsync()
        {
            if (this.IsConnected)
                await this.CloseAsync(CloseReason.LocalClosing);

            await this.DisposeProvider();

            if (this.usernamePromptList != null)
                this.usernamePromptList.Clear();

            this.usernamePromptList = null;

            if (passwordPromptList != null)
                this.passwordPromptList.Clear();

            this.passwordPromptList = null;

            if (this.enableSecretPromptList != null)
                this.enableSecretPromptList.Clear();

            this.enableSecretPromptList = null;

            if (this.morePromptList != null)
                this.morePromptList.Clear();

            this.morePromptList = null;

            if (nonPrivilegeModePromptList != null)
                this.nonPrivilegeModePromptList.Clear();

            this.nonPrivilegeModePromptList = null;

            if (this.privilegeModePromptList != null)
                this.privilegeModePromptList.Clear();

            this.privilegeModePromptList = null;

            if (this.logTextWriter != null)
            {
                this.logTextWriter.Close();
                await this.logTextWriter.DisposeAsync();
            }

            this.logTextWriter = null;
        }

        #endregion |   Public Methods   |

        #region |   Protected Methods   |

        protected virtual async ValueTask<string> EnterInterfaceConfigInternalAsync(string interfaceName) => await this.SendAsync(String.Format("{0} {1}", this.InterfaceConfigCommand, interfaceName));

        protected virtual async ValueTask<string> ExitInterfaceConfigAsync() => await this.SendAsync(this.ExitInterfaceConfigCommand);

        protected virtual async ValueTask<string> ExitVlanDatabaseConfigAsync() => await this.SendAsync(this.ExitVlanDatabaseConfigCommand);
        
        protected List<string> ConvertStringToPromptList(string prompts)
        {
            List<string> result = new List<string>();

            if (prompts.IsNullOrEmpty())
                return result;

            string[] resultArray = prompts.Split(new string[] { this.PromptSeparator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string text in resultArray)
            {
                string element = text.Trim();

                if (element.Length > 0)
                    result.Add(text.Trim());
            }

            return result;
        }

        protected string ConvertPromptListToString(List<string> promptList)
        {
            string result = String.Empty;
            string separator = String.Empty;
            string emptySpace = String.Empty;

            for (int i = 0; i < promptList.Count; i++)
            {
                if (i > 0)
                    result += this.PromptSeparator + " ";

                result += promptList[i];
            }

            return result;
        }

        protected void ChangeTerminalConnectionState(TerminalConnectionState connectionState) //, bool resetCumulativeReceivedData = false)
        {
            //bool isConnectedRightNow = this.terminalConnectionStateStatus != TerminalControlConnectionStatus.Connected && terminalConnectionStateStatus == TerminalControlConnectionStatus.Connected;
            this.connectionState = connectionState;
			//this.receivingBuffer = String.Empty;

			//if (resetCumulativeReceivedData)
			//	this.receivedData = String.Empty;

			//this.ProviderControl.AcceptData = true;

            //if (isConnectedRightNow)
            //    this.threadSync.Release(this.threadSyncToken);
        }

        protected virtual ValueTask OnConnected(IPEndPoint remoteEndpoint) => new ValueTask();
        protected virtual ValueTask OnAuthenticated() => new ValueTask();
        protected virtual ValueTask OnTextSending(string data) => new ValueTask();
        protected virtual ValueTask OnTextReceived(string data) => new ValueTask();
        protected virtual ValueTask OnClosed() => new ValueTask();

        protected virtual async ValueTask<string> ProcessTerminalData(string data)
		{
			int escPos = data.IndexOf((char)27); // ESC sequence number (0x1B)

			while (escPos >= 0)
			{
				if (data.Substring(escPos + 1).StartsWith("[c")) // Identify what terminal type
				{
					await this.SendAsync("\x1B[?1;0c"); // 0x1B is included
					this.isTerminalTypeSent = true;
				}
				else if (data.Substring(escPos + 1).StartsWith("[5n")) // Identify terminal status
				{
					await this.SendAsync("\x1B[0n"); // Terminal status OK
				}
				else if (data.Substring(escPos + 1).StartsWith("[6n")) // Identify cursor position
				{
					if (!isTerminalTypeSent)
					{
                        await this.SendAsync("\x1B[?1;0c"); // 0x1B is included
                        this.isTerminalTypeSent = true;
					}

                    await this.SendAsync("\x1B[120;80R");
					//await this.SendAsync("\x21[9001;1R");
				}

				escPos = data.IndexOf((char)27, escPos + 1); // ESC sequence number (0x1B)
			}

			return TerminalHelper.RemoveEscapeSequences(data);
		}

		protected virtual string ProcessTerminalData2(string data)
		{
			StringBuilder result = new StringBuilder();
			char[] chars = data.ToCharArray();

			bool isSequence = false;
			int index = 0;

			char beginChar = '\0';
			char endChar = '\0';

			while (index < chars.Length)
			{
				if (chars[index] == (char)27) // ESC sequence number (0x1B)
				{
					isSequence = true;
					index++;

					if (index == chars.Length)
						break;

					switch (chars[index])
					{
						//Codes for use in VT52 compatibility mode

						case 'A': 

							this.OnEscapeSequenceCursorUp();
							
                            break;

						case 'B':

							this.OnEscapeSequenceCursorDown();
							
                            break;

						case 'C':

							this.OnEscapeSequenceCursorRight();
							
                            break;

						case 'D':

							this.OnEscapeSequenceCursorLeft();
							
                            break;

						case 'F':

							this.OnEscapeSequenceEnterGraphicsMode();
							
                            break;

						case 'G':

							this.OnEscapeSequenceExitGraphicsMode();
							
                            break;

						case 'H':

							this.OnEscapeSequenceCursorToHome();
							
                            break;

						case 'I':

							this.OnEscapeSequenceReverseLineFeed();
							
                            break;

						case 'J':

							this.OnEscapeSequenceEraseToEndOfScreen();
							
                            break;

						case 'K':

							this.OnEscapeSequenceEraseToEndOfLine();
							
                            break;

						case 'Y':

							index++;

							if (index == chars.Length)
								break;

							int line = chars[index] - 31;

							index++;

							if (index == chars.Length)
								break;

							int column = chars[index] - 31;

							this.OnEscapeSequenceDirectCursorAddress(line, column);
							
                            break;

						case 'Z':

							this.OnEscapeSequenceIdentify();
							
                            break;

						case '=':

							this.OnEscapeSequenceEnterAlternateKeypadMode();
							
                            break;

						case '>':

							this.OnEscapeSequenceExitAlternateKeypadMode();
							
                            break;

						case '<':

							this.OnEscapeSequenceEnterAnsiMode();
							
                            break;

						// ANSI Compatible Mode
						// Cursor Movement Commands

						default:
							
                            break;
					}

					beginChar = chars[index + 1];

					// Find sequence with single character
					string beginEnd = beginChar.ToString() + beginChar.ToString();
					bool sequenceExists = true; // TerminalHelper.SequencesByBeginEndCharacters.Contains(beginEnd);

					if (sequenceExists)
						isSequence = false;

					index += 2;

					continue;
				}

				if (isSequence)
				{
					endChar = chars[index];

					string beginEnd = beginChar.ToString() + endChar.ToString();
					bool sequenceExists = true; // TerminalHelper.SequencesByBeginEndCharacters.Contains(beginEnd);

					if (sequenceExists)
					{
						isSequence = false;
						index++;
						
                        continue;
					}
				}
				else
				{
					result.Append(chars[index]);
				}

				index++;
			}

			return TerminalHelper.RemoveEscapeSequences(data); 
		}

		//protected virtual string GetCommandOnSeparateReceivedTerminalData(string receivedFirstLine, string sentCommand)
		//{
		//	return sentCommand;
		//}

		protected virtual TerminalDataResponse SeparateReceivedTerminalData(string terminalData, string sentCommand)
		{
			string command = sentCommand;
			string dataBody = String.Empty;
			string promptLine = String.Empty;
			bool isPreviusLineMorePrompt = false;
			string[] lines = terminalData.ToLines(StringSplitOptions.None);

			for (int i = 0; i < lines.Length; i++)
			{
				bool appendLine = true;
				string line = lines[i];

				if (line.ContainsAny(this.MorePromptList, ignoreCase: true))
				{
					appendLine = false;
					isPreviusLineMorePrompt = true;
				}

				// If previous line had more prompt and this line is empty - nothing to appending
				if (appendLine && isPreviusLineMorePrompt)
				{
					if (line.Trim().Length == 0)
						appendLine = false;

					isPreviusLineMorePrompt = false;
				}

				// If line is last, has no false privilege mode prompts and contain privilege mode prompt - no appending
				if (appendLine && i == lines.Length - 1 && !line.ContainsAny(this.falsePrivilegeModePromptList, ignoreCase: true) && line.ContainsAny(this.PrivilegeModePromptList, ignoreCase: true))
				{
					appendLine = false;
					promptLine = line;
				}

				if (appendLine)
				{
					dataBody += line;

					if (i < lines.Length - 1)
						dataBody += "\r\n";
				}
			}

			return new TerminalDataResponse(command, dataBody, promptLine);
		}

        protected virtual bool RemoveCommandFromReceivedDataIfExists(string receivedData, string sentCommand, out string result)
        {
            bool isRemoved = false;
            result = receivedData;

            if (receivedData.IsNullOrEmpty() || sentCommand.IsNullOrEmpty())
                return false;

            if (receivedData.Length == 0 || sentCommand.Length == 0)
                return false;

            //string sentCommandWithoutSplitters = sentCommand.Replace("\r\n", "").Replace("\n\r", "").Replace("\r", "").Replace("\b", "").Replace("\n", "").Trim();

            if (sentCommand.Length > 0)
            {
                int startIndex = receivedData.IndexOf(sentCommand);

                if (startIndex >= 0)
                {
                    result = receivedData.Substring(startIndex + sentCommand.Length);
                    isRemoved = true;
                }
            }

            return isRemoved;
        }

        #endregion |   Protected Methods   |

        #region |   Terminal Escape Sequence Handling   |

        /// <summary>
        /// EscA: Move the active position upward one position without altering the horizontal position. If an attempt is made to move the cursor above the top margin, the cursor stops at the top margin.
        /// </summary>
        protected virtual void OnEscapeSequenceCursorUp()
		{
		}

		/// <summary>
		/// EscB: Move the active position downward one position without altering the horizontal position.If an attempt is made to move the cursor below the bottom margin, the cursor stops at the bottom margin.
		/// </summary>
		protected virtual void OnEscapeSequenceCursorDown()
		{
		}

		/// <summary>
		/// EscC: Move the active position to the right. If an attempt is made to move the cursor to the right of the right margin, the cursor stops at the right margin.
		/// </summary>
		protected virtual void OnEscapeSequenceCursorRight()
		{
		}

		/// <summary>
		/// EscD: Move the active position one position to the left. If an attempt is made to move the cursor to the left of the left margin, the cursor stops at the left margin.
		/// </summary>
		protected virtual void OnEscapeSequenceCursorLeft()
		{
		}

		/// <summary>
		/// EscF: Causes the special graphics character set to be used.
		/// NOTE: The special graphics characters in the VT100 are different from those in the VT52.
		/// </summary>
		protected virtual void OnEscapeSequenceEnterGraphicsMode()
		{
		}

		/// <summary>
		/// EscG: This sequence causes the standard ASCII character set to be used.
		/// </summary>
		protected virtual void OnEscapeSequenceExitGraphicsMode()
		{
		}

		/// <summary>
		/// EscH: Move the cursor to the home position.
		/// </summary>
		protected virtual void OnEscapeSequenceCursorToHome()
		{
		}

		/// <summary>
		/// EscI: Move the active position upward one position without altering the column position.If the active position is at the top margin, a scroll down is performed.
		/// </summary>
		protected virtual void OnEscapeSequenceReverseLineFeed()
		{
		}

		/// <summary>
		/// EscJ: Erase all characters from the active position to the end of the screen. The active position is not changed.
		/// </summary>
		protected virtual void OnEscapeSequenceEraseToEndOfScreen()
		{
		}

		/// <summary>
		/// EscK: Erase all characters from the active position to the end of the current line.The active position is not changed.
		/// </summary>
		protected virtual void OnEscapeSequenceEraseToEndOfLine()
		{
		}

		/// <summary>
		/// EscYLineColumn: Move the cursor to the specified line and column. The line and column numbers are sent as ASCII codes whose values are the number plus \037; 
		/// e.g., \040 refers to the first line or column, \050 refers to the eighth line or column, etc.
		/// </summary>
		protected virtual void OnEscapeSequenceDirectCursorAddress(int line, int column)
		{
		}

		/// <summary>
		/// EscZ: This sequence causes the terminal to send its identifier escape sequence to the host.This sequence is: Esc/Z
		/// </summary>
		protected virtual void OnEscapeSequenceIdentify()
		{
			this.SendAsync("\033/Z").GetAwaiter().GetResult();
		}

		/// <summary>
		/// Esc=: The optional auxiliary keypad keys will send unique identifiable escape sequences for use by applications programs.
		/// NOTE: Information regarding options must be obtained in ANSI mode, using the device attributes(DA) control sequences.
		/// </summary>
		protected virtual void OnEscapeSequenceEnterAlternateKeypadMode()
		{
		}

		/// <summary>
		/// Esc&lt;: The optional auxiliary keypad keys send the ASCII codes for the functions or characters engraved on the key.
		/// </summary>
		protected virtual void OnEscapeSequenceExitAlternateKeypadMode()
		{
		}

		/// <summary>
		/// Esc&gt;: All subsequent escape sequences will be interpreted according to ANSI Standards X3.64-1977 and X3.41-1974. 
		/// The VT52 escape sequence designed in this section will not be recognized.
		/// </summary>
		protected virtual void OnEscapeSequenceEnterAnsiMode()
		{
		}

		#endregion |   Terminal Escape Sequence Handling   |

		#region |   Private Raise Events Methods   |

		private async ValueTask RaiseConnected(IPEndPoint remoteEndPoint)
		{
			if (this.Connected != null)
                await this.Connected(remoteEndPoint);
		}

        private async ValueTask RaiseAuthenticated()
        { 
            if (this.Authenticated != null)
                await this.Authenticated();
        }
        
        private async ValueTask RaiseTextSending(string text)
        {
            if (this.TextSent != null)
                await this.TextSent(text);
        }

        private async ValueTask RaiseTextReceived(string text)
        {
            if (this.TextReceived != null)
                await this.TextReceived(text);
        }

        private async ValueTask RaiseClosed()
        {
            var closed = this.Closed;

            if (closed == null)
                return;

            if (Interlocked.CompareExchange(ref this.Closed, null, closed) != closed)
                return;

            var closeReason = this.CloseReason; // ?? Telnet.CloseReason.Unknown;

            await closed(closeReason);
        }

		#endregion |   Private Raise Events Methods   |

		#region |   Private Methods   |

		private async ValueTask SendAsyncInternal(string text, CancellationToken cancellationToken = default)
		{
			System.Diagnostics.Debug.WriteLine("SENT: " + text);

            if (text.Contains("[?1;0c"))
			    await Task.Delay(1);

			if (this.SendDelay > 0)
				await Task.Delay(this.SendDelay);

			await this.Provider.SendAsync(text, cancellationToken);
		}


		private void SetProvider()
        {
            if (this.provider != null)
            {
                this.provider.TextReceived -= Provider_TextReceived;
                (this.provider as IDisposable).Dispose();
            }

            switch (this.TerminalProtocol)
            {
                case TerminalProtocol.Telnet:
                    
                    switch (this.TelnetProviderType)
                    {
						case TelnetProviderType.TelnetPipeClient:

                            this.provider = new TelnetPipeClient(); // TelnetControlSimpleTelnet();
							
                            break;

                        case TelnetProviderType.TelnetSocketClient:

                            this.provider = new TelnetSocketClient(); // TelnetControlSimpleTelnet();
                            
                            break;

                        case TelnetProviderType.ThoughtNetTelnet:

                            this.provider = new TelnetClientThoughtNetTelnet();

                            break;
                        
                        default: throw new ArgumentException("For the Telnet Provider Type " + this.TelnetProviderType.ToString() + " provider class is not specified.");
                    }

                    break;

                case TerminalProtocol.SSH1:

					switch (this.Ssh1ProviderType)
                    {
                        case Ssh1ProviderType.SshGranados:

							this.provider = new Ssh1ClientSshGranadosNew();
                            
                            break;

                        default: throw new ArgumentException("For the SSH1 Provider Type " + this.Ssh1ProviderType.ToString() + " provider class is not specified.");
                    }

                    break;

                case TerminalProtocol.SSH2:

                    switch (this.Ssh2ProviderType)
                    {
                        case Ssh2ProviderType.SshNet:

							this.provider = new Ssh2ClientSshNetNew();
                            
                            break;

                        default: throw new ArgumentException("For the SSH2 Provider Type " + this.Ssh2ProviderType.ToString() + " provider class is not specified.");
                    }

					break;            

                default: throw new ArgumentException("For the Terminal Protocol " + this.TerminalProtocol.ToString() + " provider class is not specified.");
            }

            if (this.provider != null)
                this.provider.TextReceived += Provider_TextReceived;
        }

        //private bool isMoreDetected = false;

        //protected virtual string[] SplitToLines(string processedReceivedData)
        //{
        //	return processedReceivedData.SplitToLines();
        //}

        private async ValueTask Provider_TextReceived(string text)
        {
            const string strPressAnyKey = "Press any key";
            TerminalConnectionState oldConnectionStatus = this.ConnectionState;
			string processedTerminalData = await this.ProcessTerminalData(text); //  TerminalHelper.RemoveEscapeSequences(this.receivedData);
            string textToSend = String.Empty;
            bool sendCrLf = true;

            System.Diagnostics.Debug.WriteLine("RECEIVED: " + text);


            this.processedReceivedData += processedTerminalData;
            this.receivedText += text;

			if (this.AutoLogin && this.ConnectionState != TerminalConnectionState.Authenticated && this.ConnectionState != TerminalConnectionState.Disconnected)
            {
				bool resetProcessedReceivedData = false;
				string processedReceivedDataWithoutSentCommand = String.Empty;
				bool authirized = false;

				this.RemoveCommandFromReceivedDataIfExists(this.processedReceivedData, this.lastSentSyncCommand, out processedReceivedDataWithoutSentCommand);

                if (processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase))
                    authirized = true;

				if (this.ConnectionState == TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt)
                {
					if (this.processedReceivedData.Contains(strPressAnyKey, ignoreCase: !this.MatchCase))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						textToSend = "\r\n"; // Pressing Enter
                        sendCrLf = false;
                        //await this.SendAsync(textToSend);
                    }
                    else if (this.processedReceivedData.ContainsAny(this.UsernamePromptList, ignoreCase: !this.MatchCase))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt);
                        textToSend = this.Username;
						//await this.SendAsync(textToSend);
                    }
                    else if (this.processedReceivedData.ContainsAny(this.PasswordPromptList, ignoreCase: !this.MatchCase))
                    {
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt);
                    }
                }

                if (this.ConnectionState == TerminalConnectionState.LoginInProgressWaitingForPasswordPrompt)
                {
                    if (this.processedReceivedData.ContainsAny(this.PasswordPromptList, ignoreCase: !this.MatchCase))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt);
                        this.connectionLog += "**********";
                        textToSend = this.Password;
                        //await this.SendAsync(String.Format("{0}\r\n", this.Password));
                    }
                }

				// In the case of the ssh connection authorization can be automated and you will get command prompt at first
				if (this.ConnectionState == TerminalConnectionState.LoginInProgressWaitingForUsernamePrompt) //||
					//this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForPasswordPrompt)
				{
					if (this.processedReceivedData.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase))
					{
						this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressReadyToBeConnected);
					}
					else if (this.processedReceivedData.EndsWithAny(this.NonPrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase))
					{
						this.connectionState = TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt;
					}
				}
				
				if (this.ConnectionState == TerminalConnectionState.LoginInProgressWaitingForEnableSecretPrompt)
                {
					if (processedReceivedDataWithoutSentCommand.ContainsAny(this.NonPrivilegeModePromptList, ignoreCase: !this.MatchCase)) // EndsWithAny(this.NonPrivilegeModePromptList, true, false))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;

						if (processedReceivedDataWithoutSentCommand.EndsWithAny(this.NonPrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase))
                        {
                            this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressWaitingForEnableSecretPasswordPrompt);
                            textToSend = this.PrivilegeModeCommand;
                            //this.connectionLog += textToSend;
                            //await this.SendAsync(textToSend);
                        }
                        else
                        {
                            textToSend = "\r\n";
                            sendCrLf = false;
							// await this.SendAsync("\r\n");
                        }
                    }
                    else if (processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressReadyToBeConnected);
                    }

                    //if (!isAsteriskInsertedAsPassword)
                    //{
                    //    this.connectionLog += String.Format("{0}\r\n", "********");
                    //    isAsteriskInsertedAsPassword = true;
                    //}
                }
                else if (this.ConnectionState == TerminalConnectionState.LoginInProgressWaitingForEnableSecretPasswordPrompt)
                {
                    if (processedReceivedDataWithoutSentCommand.ContainsAny(this.EnableSecretPromptList, ignoreCase: !this.MatchCase))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressAfterEnableSecretIsSent);
                        this.connectionLog += "**********";
                        textToSend = this.EnableSecret;
						//await this.SendAsync(String.Format("{0}\r\n", this.EnableSecret));
                        //this.isAsteriskInsertedAsEnableSecretPassword = false;
                    }
                    else if (processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressReadyToBeConnected);
                    }
                }
                else if (this.ConnectionState == TerminalConnectionState.LoginInProgressAfterEnableSecretIsSent)
                {
                    //if (!isAsteriskInsertedAsEnableSecretPassword)
                    //{
                    //    this.connectionLog += String.Format("{0}\r\n", "********");
                    //    isAsteriskInsertedAsEnableSecretPassword = true;
                    //}

                    if (processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: !this.MatchCase)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
                    {
                        this.connectionLog += this.processedReceivedData;
						resetProcessedReceivedData = true;
						this.ChangeTerminalConnectionState(TerminalConnectionState.LoginInProgressReadyToBeConnected);
                    }
                }

				if (resetProcessedReceivedData)
					this.processedReceivedData = String.Empty;
            }

            await this.OnTextReceived(text);
			await this.RaiseTextReceived(text);

			if (this.DoesTextEndsWithAnyMorePrompt(this.processedReceivedData, ignoreCase: !this.MatchCase))
			{
				this.processedReceivedData = this.processedReceivedData.RemoveLastLine(StringSplitOptions.None); // this.RemoveLastLineInText(this.processedReceivedData) + "\r\n";
                textToSend = " ";
                sendCrLf = false;
                //await this.SendAsync(" "); // Send space to continue after -- More --
			}

			if (this.ConnectionState == TerminalConnectionState.LoginInProgressReadyToBeConnected)
			{
				TerminalDataResponse dataResponse = this.SeparateReceivedTerminalData(this.connectionLog, sentCommand: String.Empty);

				this.PromptLine = dataResponse.PromptLine;
				await this.WriteToLogAsync(this.connectionLog);

				this.ChangeTerminalConnectionState(TerminalConnectionState.Authenticated);

				this.response = this.connectionLog;
                this.logInManualResetEvent.Set(); // LogIn suceeded => unblock curent LogIn
                //this.logInManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls
				this.processedReceivedData = String.Empty;
			}
			else if (this.ConnectionState == TerminalConnectionState.Authenticated)
            {
				string processedReceivedDataWithoutSentCommand = String.Empty;
				bool isCommandRemoved = this.RemoveCommandFromReceivedDataIfExists(this.processedReceivedData, this.lastSentSyncCommand, out processedReceivedDataWithoutSentCommand);

				if (((this.waitFor == null || this.waitFor.Length == 0 || this.waitFor[0].IsNullOrEmpty()) && processedReceivedDataWithoutSentCommand.TrimEnd().EndsWithAny(this.PrivilegeModePromptList, trim: true, ignoreCase: true)) ||
					(this.waitFor != null && this.waitFor.Length > 0 && !this.waitFor[0].IsNullOrEmpty() && processedReceivedDataWithoutSentCommand.ContainsAny(this.waitFor, ignoreCase: !this.MatchCase)))
				{
					TerminalDataResponse dataResponse = this.SeparateReceivedTerminalData(processedReceivedDataWithoutSentCommand, this.lastSentSyncCommand);

					//this.PromptLine = dataResponse.PromptLine;
					//this.WriteToLog(this.processedReceivedData);

					if (isCommandRemoved)
					{
						await this.WriteToLogAsync(dataResponse.Command);
						await this.WriteToLogAsync(dataResponse.DataBody);
						await this.WriteToLogAsync(dataResponse.PromptLine);
					}
					else
					{
						await this.WriteToLogAsync(processedReceivedDataWithoutSentCommand);
					}

					this.response = dataResponse.DataBody;
                    this.waitForManualResetEvent.Set(); // unblock curent waitFor (SendAsync)
                    //this.waitForManualResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another calls
					//this.threadSync.Release(this.token);
					this.processedReceivedData = String.Empty;
				}
            }

            if (!textToSend.IsNullOrEmpty())
            {
                string dataToSend = textToSend;

                if (sendCrLf)
                    dataToSend += "\r\n";

				//await this.SendAsyncInternal(dataToSend).ConfigureAwait(false);
				_ = this.SendAsyncInternal(dataToSend); 
            }
		}

        private bool DoesTextEndsWithAnyPrivilegedModePrompt(string text)
        {
            string val = text.TrimEnd();

            if (val.Length == 0)
                return false;

            foreach (string privilegeModePrompt in this.PrivilegeModePromptList)
            {
                if (text.TrimEnd().EndsWith(privilegeModePrompt))
                {
                    this.privilegeModePrompt = privilegeModePrompt;
                    
                    return true;
                }
            }

            return false;
        }

        private bool DoesTextEndsWithAnyMorePrompt(string text, bool ignoreCase)
        {
            bool result = false;
            
            if (text.Length > 0)
            {
                string[] lines = text.Split(new string[] { "\r\n", "\n\r", "\r", "\b" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Count() > 0)
                {
                    string lastLine = lines[lines.Length - 1].Trim();
                    result = lastLine.ContainsAny(this.MorePromptList, ignoreCase);
                }

                //if (removeLastLine)
                //{
                //    bool isFirstPass = true;
                //    text = String.Empty;

                //    for (int i = 0; i < lines.Length - 2; i++)
                //    {
                //        if (!isFirstPass)
                //            text += "\r\n";
                        
                //        text += lines[i];
                //        isFirstPass = false;
                //    }
                //}
            }

            return result;
        }


		private void SetProviderSettings()
        {
            //this.Provider.RemoteHost = this.RemoteHost;
            //this.Provider.RemotePort = this.RemotePort;

			if (this.Provider is ISshClient)
			{
				(this.Provider as ISshClient).Username = this.Username;
				(this.Provider as ISshClient).Password = this.Password;
			}
        }

        private async Task WriteToLogAsync(string text)
        {
            if (this.logging && this.logTextWriter != null) // If log file cannot be opened or path not exists this.LogTextWriter will be null
			{
                await this.logTextWriter.WriteAsync(text);
                await this.logTextWriter.FlushAsync();
            }
        }

        private async ValueTask DisposeProvider()
        {
            if (this.Provider != null)
                await this.Provider.DisposeAsync();
               
            this.provider = null;
        }

		#endregion |   Private Methods   |

		#region |   IProviderConnection Interface   |

		object IProviderConnection.Owner { get => this.owner; set => this.owner = value; }

        #endregion |   IProviderConnection Interface   |
    }

    #region |   Event Delegates   |

    public delegate ValueTask EndPointEventHandler(IPEndPoint endPoint);
    //public delegate ValueTask AsyncAction();

    #endregion |   Event Delegates   |

    #region |   Event Args & Helper Classes   |

    public struct TerminalDataResponse
    {
        public TerminalDataResponse(string command, string dataBody, string promptLine)
        {
            this.Command = command;
            this.DataBody = dataBody;
            this.PromptLine = promptLine;
        }

        public string Command { get; set; }
        public string DataBody { get; set; }
        public string PromptLine { get; set; }
    }

    //public class TerminalTextEventArgs : EventArgs
    //   {
    //	public TerminalTextEventArgs(string text)
    //       {
    //           this.Text = text;
    //       }

    //       public string Text { get; set; }
    //   }

    #endregion |   Event Args & Helper Classes   |

}
