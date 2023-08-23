//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Timers;
//using System.IO;
//using Simple;
//using Simple.Threading;
//using NET.Tools.Providers;
//using System.Drawing;

//namespace NET.Tools.Terminal
//{
//    public class TerminalControl_OLD2 : IProviderControlConnection, IDisposable
//    {
//        #region |   Private Members   |

//        private const string strTerminalConnection = "Terminal connection";
//        private TerminalConnectionStringBuilder connectionStringBuilder = new TerminalConnectionStringBuilder();
//        private object providerControl;
//        private TelnetProviderType defaultTelnetProviderType =  TelnetProviderType.ThoughtNetTelnet;
//        private SshProviderType defaultSshProviderType = SshProviderType.IPWorks;
//        private bool autoLogin = true;
//        private List<string> usernamePromptList = null;
//        private List<string> passwordPromptList = null;
//        private List<string> enableSecretPromptList = null;
//        private List<string> nonPrivilegeModePromptList = null;
//        private List<string> privilegeModePromptList = null;
//        private List<string> morePromptList = null;
//        private string privilegeModePrompt = String.Empty;
//        private TerminalControlConnectionStatus terminalConnectionStateStatus = TerminalControlConnectionStatus.Disconnected;
//        private string receivingBuffer = String.Empty;
//        private string receivedData = String.Empty;
//        private string lastSendingText = String.Empty;
//        private Timer sendingIntervalTimer = new Timer();
//        private bool sendingIntervalTimeElapsed = true;
//        private ThreadSync threadSync = new ThreadSync();
//        private ThreadSyncToken threadSyncToken = new ThreadSyncToken(strTerminalConnection);
//        private ThreadSyncToken threadSendingIntervalToken = new ThreadSyncToken(strTerminalConnection);
//        private bool isInConfigMode = false;
//        private string connectionLog = String.Empty;
//        //private bool isAsteriskInsertedAsPassword = false;
//        //private bool isAsteriskInsertedAsEnableSecretPassword = true;
//        private string sendingCommand = String.Empty;
//        private string waitFor = String.Empty;
//        private bool logging = false;
//        private string logFileName = String.Empty;
//        private TextWriter logTextWriter = null;
//        private string connectionMessage = String.Empty;

//        #endregion |   Private Members   |

//        #region |   Public Members   |

//        public const TerminalProtocol DefaultTerminalProtocol = TerminalProtocol.Telnet;

//        #endregion |   Public Members   |
        
//        #region |   Constructors and Initialization   |

//        public TerminalControl_OLD2()
//        {
//            this.SetSendingTimerInterval();
//            this.sendingIntervalTimer.Elapsed += new ElapsedEventHandler(sendingIntervalTimer_Elapsed);
//        }

//        #endregion |   Constructors and Initialization   |

//        #region |   Events   |

//        public event ConnectedEventHandler Connected;
//        public event DisconnectedEventHandler Disconnected;
//        public event TerminalTextEventHandler TextReceived;
//        public event TerminalTextEventHandler TextSending;

//        #endregion

//        #region |   Public Properties   |

//        public object Owner { get; set; }

//        public bool AutoLogin
//        {
//            get { return this.autoLogin; }
//            set { this.autoLogin = value; }
//        }

//        public string ConnectionString
//        {
//            get { return this.connectionStringBuilder.BuildConnectionString(); }
//            set
//            {
//                this.connectionStringBuilder.SetConnectionString(value);
//                this.SetSettingsToProviderControl();
//            }
//        }

//        public bool UseTerminalConnection
//        {
//            get { return this.connectionStringBuilder.UseTerminalConnection; }
//            set { this.connectionStringBuilder.UseTerminalConnection = value; }
//        }


//        public TerminalProtocol TerminalProtocol
//        {
//            get { return this.connectionStringBuilder.TerminalProtocol; }
//            set { this.connectionStringBuilder.TerminalProtocol = value; }
//        }

//        public TelnetProviderType TelnetProviderType
//        {
//            get { return this.connectionStringBuilder.TelnetProviderType; }
//            set { this.connectionStringBuilder.TelnetProviderType = value; }
//        }

//        public SshProviderType SshProviderType
//        {
//            get { return this.connectionStringBuilder.SshProviderType; }
//            set { this.connectionStringBuilder.SshProviderType = value; }
//        }

//        public string RemoteHost
//        {
//            get { return this.connectionStringBuilder.RemoteHost; }
//            set { this.connectionStringBuilder.RemoteHost = value; }
//        }

//        public int RemotePort
//        {
//            get { return this.connectionStringBuilder.RemotePort; }
//            set { this.connectionStringBuilder.RemotePort = value; }
//        }

//        public string Username
//        {
//            get { return this.connectionStringBuilder.Username; }
//            set { this.connectionStringBuilder.Username = value; }
//        }

//        public string Password
//        {
//            get { return this.connectionStringBuilder.Password; }
//            set { this.connectionStringBuilder.Password = value; }
//        }

//        public string EnableSecret
//        {
//            get { return this.connectionStringBuilder.EnableSecret; }
//            set { this.connectionStringBuilder.EnableSecret = value; }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.connectionStringBuilder.Timeout; }
//            set { this.connectionStringBuilder.Timeout = value; }
//        }

//        /// <summary>
//        /// Interval between two sendings in miliseconds.
//        /// </summary>
//        public int SendingInterval
//        {
//            get { return this.connectionStringBuilder.SendingInterval; }
//            set
//            {
//                this.connectionStringBuilder.SendingInterval = value;
//                this.SetSendingTimerInterval();
//            }
//        }

//        public string PromptSeparator
//        {
//            get { return this.connectionStringBuilder.PromptSeparator; }
//            set
//            {
//                this.connectionStringBuilder.PromptSeparator = value;
//                this.usernamePromptList = null;
//                this.passwordPromptList = null;
//                this.enableSecretPromptList = null;
//                this.nonPrivilegeModePromptList = null;
//                this.privilegeModePromptList = null;
//                this.morePromptList = null;
//            }
//        }

//        public string UsernamePrompts
//        {
//            get { return this.connectionStringBuilder.UsernamePrompts; }
//            set
//            {
//                this.connectionStringBuilder.UsernamePrompts = value;
//                this.usernamePromptList = null;
//            }
//        }

//        public string PasswordPrompts
//        {
//            get { return this.connectionStringBuilder.PasswordPrompts; }
//            set
//            {
//                this.connectionStringBuilder.PasswordPrompts = value;
//                this.passwordPromptList = null;
//            }
//        }

//        public string EnableSecretPrompts
//        {
//            get { return this.connectionStringBuilder.EnableSecretPrompts; }
//            set
//            {
//                this.connectionStringBuilder.EnableSecretPrompts = value;
//                this.enableSecretPromptList = null;
//            }
//        }

//        public string NonPrivilegeModePrompts
//        {
//            get { return this.connectionStringBuilder.NonPrivilegeModePrompts; }
//            set
//            {
//                this.connectionStringBuilder.NonPrivilegeModePrompts = value;
//                this.nonPrivilegeModePromptList = null;
//            }
//        }

//        public string PrivilegeModeCommand
//        {
//            get { return this.connectionStringBuilder.PrivilegeModeCommand; }
//            set { this.connectionStringBuilder.PrivilegeModeCommand = value; }
//        }

//        public string PrivilegeModePrompts
//        {
//            get { return this.connectionStringBuilder.PrivilegeModePrompts; }
//            set
//            {
//                this.connectionStringBuilder.PrivilegeModePrompts = value;
//                this.privilegeModePromptList = null;
//            }
//        }

//        public string MorePrompts
//        {
//            get { return this.connectionStringBuilder.MorePrompts; }
//            set
//            {
//                this.connectionStringBuilder.MorePrompts = value;
//                this.morePromptList = null;
//            }
//        }

//        public bool IsConnected
//        {
//            get { return this.terminalConnectionStateStatus == TerminalControlConnectionStatus.Connected; }
//        }

//        public TerminalControlConnectionStatus ConnectionStatus
//        {
//            get { return this.terminalConnectionStateStatus; }
//        }

//        //public string ReceivingBuffer
//        //{
//        //    get { return this.receivingBuffer; }
//        //}

//        #endregion |   Public Properties   |

//        #region |   Protected Properties   |

//        protected ITerminalControl ProviderControl
//        {
//            get
//            {
//                if (this.providerControl == null)
//                {
//                    this.SetProviderControl();
//                }

//                return this.providerControl as ITerminalControl;
//            }
//        }

//        protected List<string> UsernamePromptList
//        {
//            get
//            {
//                if (this.usernamePromptList == null)
//                {
//                    this.usernamePromptList = this.ConvertStringToPromptList(this.UsernamePrompts);
//                }

//                return this.usernamePromptList;
//            }
//        }

//        protected List<string> PasswordPromptList
//        {
//            get
//            {
//                if (this.passwordPromptList == null)
//                {
//                    this.passwordPromptList = this.ConvertStringToPromptList(this.PasswordPrompts);
//                }

//                return this.passwordPromptList;
//            }
//        }

//        protected List<string> EnableSecretPromptList
//        {
//            get
//            {
//                if (this.enableSecretPromptList == null)
//                {
//                    this.enableSecretPromptList = this.ConvertStringToPromptList(this.EnableSecretPrompts);
//                }

//                return this.enableSecretPromptList;
//            }
//        }

//        protected List<string> NonPrivilegeModePromptList
//        {
//            get
//            {
//                if (this.nonPrivilegeModePromptList == null)
//                {
//                    this.nonPrivilegeModePromptList = this.ConvertStringToPromptList(this.NonPrivilegeModePrompts);
//                }

//                return this.nonPrivilegeModePromptList;
//            }
//        }

//        protected List<string> PrivilegeModePromptList
//        {
//            get
//            {
//                if (this.privilegeModePromptList == null)
//                {
//                    this.privilegeModePromptList = this.ConvertStringToPromptList(this.PrivilegeModePrompts);
//                }

//                return this.privilegeModePromptList;
//            }
//        }

//        protected List<string> MorePromptList
//        {
//            get
//            {
//                if (this.morePromptList == null)
//                {
//                    this.morePromptList = this.ConvertStringToPromptList(this.MorePrompts);
//                }

//                return this.morePromptList;
//            }
//        }


//        #endregion |   Protected Properties   |

//        #region |   Public Methods   |

//        public ProviderConnectionInfo Connect()
//        {
//            this.connectionMessage = String.Empty;

//            this.DisposeControl();

//            this.SetProviderControl();
//            this.SetSettingsToProviderControl();
//            this.SetLocalSettings();

//            this.terminalConnectionStateStatus = TerminalControlConnectionStatus.LoginInProgressWaitingForUsernamePrompt;

//            try
//            {
//                this.ProviderControl.Connect();
//                this.ProviderControl.AcceptData = true;
//            }
//            catch (Exception ex)
//            {
//                return new ProviderConnectionInfo(this.IsConnected, ex.Message);
//            }

//            try
//            {
//                this.threadSync.WaitFor(this.threadSyncToken, this.Timeout * 1000);
//            }
//            catch (Exception ex)
//            {
//                this.connectionMessage = TerminalHelper.RemoveEscapeSequences(this.connectionLog);
//                string message = this.connectionMessage.IsNullOrEmpty() ? ex.Message : this.connectionMessage;
//                throw new Exception(message);
//            }

//            if (this.IsConnected)
//            {
//                this.OnConnected();
//                this.RaiseConnected();
//            }

//            this.connectionMessage = TerminalHelper.RemoveEscapeSequences(this.connectionLog);
            
//            if (this.logging)
//                this.WriteToLog(this.connectionMessage);

//            return new ProviderConnectionInfo(this.IsConnected, this.connectionMessage);
//        }

//        public void Disconnect()
//        {
//            //if (this.IsConnected)
//            //{
//            //    this.ExitConfigMode();
//            //    this.SendAsync("logout");
//            //}


//            bool wasConnected = this.IsConnected;
//            this.ProviderControl.Disconnect();

//            this.terminalConnectionStateStatus = TerminalControlConnectionStatus.Disconnected;

//            if (this.logTextWriter != null)
//            {
//                if (wasConnected)
//                    this.WriteToLog("logout");
                
//                this.logTextWriter.Close();
//            }

//            this.logTextWriter = null;
//            this.receivingBuffer = String.Empty;
//            this.receivedData = String.Empty;
//            this.connectionMessage = String.Empty;
//            this.connectionLog = String.Empty;
//            this.OnDisconnected();
//            this.RaiseDisconnected();
//        }

//        public ProviderConnectionInfo TestConnection()
//        {
//            bool wasConnected = this.IsConnected;
//            int oldTimeout = this.Timeout;
//            this.Timeout = 5;
            
//            if (this.IsConnected)
//                this.Disconnect();

//            ProviderConnectionInfo result = this.Connect();

//            if (!wasConnected)
//                this.Disconnect();

//            this.Timeout = oldTimeout;
//            return result;
//        }

//        public string Send(string command)
//        {
//            return this.Send(command, true);
//        }

//        public string Send(string command, bool sendCrLf)
//        {
//            return this.Send(command, String.Empty);
//        }

//        public string Send(string command, string waitFor)
//        {
//            return this.Send(command, waitFor, true);
//        }

//        public string Send(string command, string waitFor, bool sendCrLf)
//        {
//            if (!this.IsConnected)
//                this.Connect();
            
//            string response = String.Empty;
//            string commandToSend = sendCrLf ? String.Format("{0}\r\n", command) : command;
//            this.sendingCommand = commandToSend;
//            this.waitFor = waitFor;
//            this.receivingBuffer = String.Empty;
//            this.receivedData = String.Empty;

//            this.SendAsync(commandToSend);
//            this.threadSync.WaitFor(this.threadSyncToken, this.Timeout * 1000);

//            response = this.receivedData;

//            this.sendingCommand = String.Empty;
//            this.waitFor = String.Empty;

//            return response;
//        }

//        public virtual void EnterConfigMode()
//        {
//            if (!this.IsConnected)
//                this.Connect();

//            if (!this.isInConfigMode)
//            {
//                string resonse = this.Send("configure terminal");
//                this.isInConfigMode = true;
//            }
//        }

//        public virtual void ExitConfigMode()
//        {
//            if (!this.IsConnected)
//                this.Connect();

//            if (this.isInConfigMode)
//            {
//                //this.Send("end");
//                string response = this.Send("exit");
//                this.isInConfigMode = false;
//            }
//        }

//        public void SetLogging(string logFileName)
//        {
//            this.logFileName = logFileName;
//            this.logging = false;

//            if (!logFileName.IsNullOrEmpty())
//            {
//                if (this.logTextWriter != null)
//                    this.logTextWriter.Close();
                
//                try
//                {
//                    this.logTextWriter = new StreamWriter(logFileName, append: false);
//                    this.logging = true;

//                    if (!this.connectionMessage.IsNullOrEmpty())
//                    {
//                        this.WriteToLog(this.connectionMessage);
//                    }
//                }
//                catch
//                {
//                }
//            }
//        }

//        public void Dispose()
//        {
//            this.DisposeControl();
//            this.sendingIntervalTimer.Elapsed -= new ElapsedEventHandler(sendingIntervalTimer_Elapsed);
//            this.sendingIntervalTimer.Dispose();
//            this.sendingIntervalTimer = null;

//            if (this.logTextWriter != null)
//            {
//                this.logTextWriter.Close();
//                this.logTextWriter.Dispose();
//            }

//            this.logTextWriter = null;
//        }

//        #endregion |   Public Methods   |

//        #region |   Protected Methods   |

//        protected List<string> ConvertStringToPromptList(string prompts)
//        {
//            List<string> result = new List<string>();
//            string[] resultArray = prompts.Split(new string[] { this.PromptSeparator }, StringSplitOptions.RemoveEmptyEntries);

//            foreach (string text in resultArray)
//            {
//                string element = text.Trim();

//                if (element.Length > 0)
//                {
//                    result.Add(text.Trim());
//                }
//            }

//            return result;
//        }

//        protected string ConvertPromptListToString(List<string> promptList)
//        {
//            string result = String.Empty;
//            string separator = String.Empty;
//            string emptySpace = String.Empty;

//            for (int i = 0; i < promptList.Count; i++)
//            {
//                if (i > 0)
//                {
//                    result += this.PromptSeparator + " ";
//                }

//                result += promptList[i];
//            }

//            return result;
//        }

//        protected void OnConnected()
//        {
//        }

//        protected void OnDisconnected()
//        {
//        }

//        protected void OnTextSending(string text)
//        {
//        }

//        protected void OnTextReceived(string text)
//        {
//        }

//        #endregion |   Protected Methods   |

//        #region |   Private Raise Events Methods   |

//        private void RaiseConnected()
//        {
//            if (this.Connected != null)
//            {
//                this.Connected(this, new EventArgs());
//            }
//        }

//        private void RaiseDisconnected()
//        {
//            if (this.Disconnected != null)
//            {
//                this.Disconnected(this, new EventArgs());
//            }
//        }

//        private void RaiseTextSending(string text)
//        {
//            if (this.TextSending != null)
//            {
//                this.TextSending(this, new TerminalTextEventArgs(text));
//            }
//        }

//        private void RaiseTextReceived(string text)
//        {
//            if (this.TextReceived != null)
//            {
//                this.TextReceived(this, new TerminalTextEventArgs(text));
//            }
//        }

//        #endregion |   Private Raise Events Methods   |

//        #region |   Private Methods   |

//        private void SendAsync(string command)
//        {
//            this.lastSendingText = command;
//            this.OnTextSending(command);
//            this.RaiseTextSending(command);

//            if (this.SendingInterval > 0)
//            {
//                // Wait for time to elapse between two sendings
//                if (!this.sendingIntervalTimeElapsed)
//                {
//                    this.threadSync.WaitFor(this.threadSendingIntervalToken);
//                }

//                // Fire sendingIntervalTimer to prevent sending another data without pausing between two sendings.
//                this.sendingIntervalTimeElapsed = false;
//                this.sendingIntervalTimer.Start();
//            }

//            if (this.IsConnected)
//                this.WriteToLog(command);
            
//            this.ProviderControl.Send(command);
//        }

//        private void SetProviderControl()
//        {
//            if (this.providerControl != null)
//            {
//                this.ProviderControl.TextReceived -= new TerminalTextEventHandler(ProviderControl_TextReceived);
//                (this.ProviderControl as IDisposable).Dispose();
//            }

//            Network.TerminalProtocol terminalProtocol = this.TerminalProtocol == Network.TerminalProtocol.Default ? DefaultTerminalProtocol : this.TerminalProtocol;

//            switch (terminalProtocol)
//            {
//                case Network.TerminalProtocol.Telnet:
                    
//                    Network.TelnetProviderType telnetProviderType = this.TelnetProviderType == Network.TelnetProviderType.Default ? defaultTelnetProviderType : this.TelnetProviderType;

//                    switch (telnetProviderType)
//                    {
//                        case Network.TelnetProviderType.ThoughtNetTelnet:
//                            this.providerControl = new TelnetControlThoughtNetTelnet();
//                            break;

//                        //case Network.TelnetProviderType.IPWorks:
//                        //    this.providerControl = new TelnetControlIPWorks();
//                        //    break;

//                        default:
//                            throw new ArgumentException("For the TelnetProviderType " + this.TelnetProviderType.ToString() + " provider class is not specified.");
//                    }

//                    break;

//                case Network.TerminalProtocol.SSH1:
//                    Network.SshProviderType sshProviderType = this.SshProviderType == Network.SshProviderType.Default ? defaultSshProviderType : this.SshProviderType;

//                    //switch (sshProviderType)
//                    //{
//                    //    //case Network.SshProviderType.IPWorks:
//                    //    //    this.providerControl = new SshControlIPWorks();
//                    //    //    break;

//                    //    default:
//                    //        throw new ArgumentException("For the SshProviderType " + this.SshProviderType.ToString() + " provider class is not specified.");
//                    //}

//                    break;

//                case Network.TerminalProtocol.SSH2:
//                    //Network.SshProviderType sshProviderType = this.SshProviderType == Network.SshProviderType.Default ? defaultSshProviderType : this.SshProviderType;

//                    //switch (sshProviderType)
//                    //{
//                    //    //case Network.SshProviderType.IPWorks:
//                    //    //    this.providerControl = new SshControlIPWorks();
//                    //    //    break;

//                    //    default:
//                    //        throw new ArgumentException("For the SshProviderType " + this.SshProviderType.ToString() + " provider class is not specified.");
//                    //}

//                    break;

//                default:
//                    throw new ArgumentException("For the TerminalProtocol " + this.TerminalProtocol.ToString() + " provider class is not specified.");
//            }

//            if (this.ProviderControl != null)
//            {
//                this.ProviderControl.TextReceived += new TerminalTextEventHandler(ProviderControl_TextReceived);
//            }
//        }

//        private bool isMoreDetected = false;

//        private void ProviderControl_TextReceived(object sender, TerminalTextEventArgs e)
//        {
//            const string strPressAnyKey = "Press any key";
//            this.ProviderControl.AcceptData = false;
//            this.receivingBuffer += e.Text;
//            this.receivingBuffer = TerminalHelper.RemoveEscapeSequences(this.receivingBuffer);
//            TerminalControlConnectionStatus oldConnectionStatus = this.ConnectionStatus;

//            if (!this.isMoreDetected && !this.lastSendingText.IsNullOrEmpty() && this.receivingBuffer.StartsWith(this.lastSendingText))
//            {
//                this.receivedData = this.receivingBuffer.Substring(this.lastSendingText.Length, this.receivingBuffer.Length - this.lastSendingText.Length);
//                this.receivingBuffer = String.Empty;
//            }
//            else
//            {
//                this.receivedData += e.Text;

//                //if (!this.isMoreDetected && !this.lastSendingText.IsNullOrEmpty() && this.receivingBuffer.StartsWith(this.lastSendingText))
//                //{
//                //    this.receivedData = this.receivingBuffer.Substring(this.lastSendingText.Length, this.receivingBuffer.Length - this.lastSendingText.Length);
//                //    this.receivingBuffer = String.Empty;
//                //}
//            }

//            this.isMoreDetected = false;
//            string recivedDataWithoutEscapeSequences = TerminalHelper.RemoveEscapeSequences(this.receivedData);
            
//            if (this.AutoLogin && this.ConnectionStatus != TerminalControlConnectionStatus.Connected && this.ConnectionStatus != TerminalControlConnectionStatus.Disconnected)
//            {
//                if (this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForUsernamePrompt)
//                {
//                    if (e.Text.Contains(strPressAnyKey, ignoreCase: true))
//                    {
//                        this.connectionLog += this.receivedData;
//                        string textToSend = "\r\n";
//                        this.connectionLog += textToSend;
//                        this.SendAsync(textToSend);
//                    }
//                    else if (recivedDataWithoutEscapeSequences.Contains(this.UsernamePromptList, ignoreCase: true))
//                    {
//                        this.connectionLog += this.receivedData;
//                        char[] chars = e.Text.ToCharArray();
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.LoginInProgressWaitingForPasswordPrompt);
//                        String textToSend = String.Format("{0}\r\n", this.Username);
//                        this.connectionLog += textToSend;
//                        this.SendAsync(textToSend);
//                    }
//                    else if (recivedDataWithoutEscapeSequences.Contains(this.PasswordPromptList, ignoreCase: true))
//                    {
//                        string tempRecievedData = this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.LoginInProgressWaitingForPasswordPrompt);
//                        this.receivedData = tempRecievedData;
//                    }
//                }

//                if (this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForPasswordPrompt)
//                {
//                    if (recivedDataWithoutEscapeSequences.Contains(this.PasswordPromptList, ignoreCase: true))
//                    {
//                        this.connectionLog += this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.LoginInProgressWaitingForEnableSecretPrompt);
//                        this.connectionLog += "**********";
//                        this.SendAsync(String.Format("{0}\r\n", this.Password));
////                        this.isAsteriskInsertedAsPassword = false;
//                    }
//                }
//                else if (this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForEnableSecretPrompt)
//                {
//                    if (recivedDataWithoutEscapeSequences.Contains(this.NonPrivilegeModePromptList, ignoreCase: true)) // EndsWithAny(this.NonPrivilegeModePromptList, true, false))
//                    {
//                        this.connectionLog += this.receivedData;

//                        if (recivedDataWithoutEscapeSequences.EndsWithAny(this.NonPrivilegeModePromptList, trim: true, ignoreCase: true))
//                        {
//                            this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.LoginInProgressWaitingForEnableSecretPasswordPrompt);
//                            String textToSend = String.Format("{0}\r\n", this.PrivilegeModeCommand);
//                            this.connectionLog += textToSend;
//                            this.SendAsync(textToSend);
//                        }
//                        else
//                        {
//                            String textToSend = "\r\n";
//                            this.connectionLog += textToSend;
//                            this.SendAsync(textToSend);
//                        }
//                    }
//                    else if (recivedDataWithoutEscapeSequences.Contains(this.PrivilegeModePromptList, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//                    {
//                        this.connectionLog += this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.Connected);
//                    }

//                    //if (!isAsteriskInsertedAsPassword)
//                    //{
//                    //    this.connectionLog += String.Format("{0}\r\n", "********");
//                    //    isAsteriskInsertedAsPassword = true;
//                    //}
//                }
//                else if (this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressWaitingForEnableSecretPasswordPrompt)
//                {
//                    if (recivedDataWithoutEscapeSequences.Contains(this.EnableSecretPromptList, true))
//                    {
//                        this.connectionLog += this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.LoginInProgressAfterEnableSecretIsSent);
//                        String textToSend = String.Format("{0}\r\n", this.EnableSecret);
//                        this.connectionLog += "**********";
//                        this.SendAsync(String.Format("{0}\r\n", this.EnableSecret));
//                        //this.isAsteriskInsertedAsEnableSecretPassword = false;
//                    }
//                    else if (recivedDataWithoutEscapeSequences.Contains(this.PrivilegeModePromptList, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//                    {
//                        this.connectionLog += this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.Connected);
//                    }
//                }
//                else if (this.ConnectionStatus == TerminalControlConnectionStatus.LoginInProgressAfterEnableSecretIsSent)
//                {
//                    //if (!isAsteriskInsertedAsEnableSecretPassword)
//                    //{
//                    //    this.connectionLog += String.Format("{0}\r\n", "********");
//                    //    isAsteriskInsertedAsEnableSecretPassword = true;
//                    //}

//                    if (recivedDataWithoutEscapeSequences.Contains(this.PrivilegeModePromptList, ignoreCase: true)) //(this.DoesTextEndsWithAnyPrivilegedModePrompt(this.receivedData))
//                    {
//                        this.connectionLog += this.receivedData;
//                        this.ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus.Connected);
//                    }
//                }
//            }

//            this.OnTextReceived(e.Text);
//            this.RaiseTextReceived(e.Text);

//            if (this.ConnectionStatus == TerminalControlConnectionStatus.Connected)
//            {
//                if (recivedDataWithoutEscapeSequences.TrimStart().StartsWith(this.sendingCommand))
//                    recivedDataWithoutEscapeSequences = recivedDataWithoutEscapeSequences.TrimStart().Substring(this.sendingCommand.Length);

//                if (this.DoesTextEndsWithAnyMorePrompt(recivedDataWithoutEscapeSequences, true))
//                {
//                    this.ProviderControl.AcceptData = true;
//                    this.isMoreDetected = true;
//                    this.SendAsync(" "); // Send space to continue after -- More --
//                }
//                else if (recivedDataWithoutEscapeSequences.Contains(this.PrivilegeModePromptList, ignoreCase: true) || (!this.waitFor.IsNullOrEmpty() && recivedDataWithoutEscapeSequences.Contains(this.waitFor)))
//                {
//                    //this.receivedData = TerminalSequences.RemoveSequences(this.receivedData);
                    
//                    // prevent writing to log for first pass
//                    if (oldConnectionStatus == TerminalControlConnectionStatus.Connected)
//                        this.WriteToLog(recivedDataWithoutEscapeSequences);
                    
//                    this.receivedData = this.FilterTerminalData(recivedDataWithoutEscapeSequences);
//                    //if (this.receivedData.StartsWith("\r\n\r\n\r\n"))
//                    //    this.receivedData = this.receivedData.Substring(4, this.receivedData.Length - 4);

//                    this.threadSync.Release(this.threadSyncToken); // previlege mode prompt is recieved -> release Send thread.
//                }
//            }
                
//            this.ProviderControl.AcceptData = true;
//        }

//        private void ChangeTerminalConnectionStateStatus(TerminalControlConnectionStatus terminalConnectionStateStatus)
//        {
//            bool isConnectedRightNow = this.terminalConnectionStateStatus != TerminalControlConnectionStatus.Connected && terminalConnectionStateStatus == TerminalControlConnectionStatus.Connected;
//            this.terminalConnectionStateStatus = terminalConnectionStateStatus;
//            this.receivingBuffer = String.Empty;
//            this.receivedData = String.Empty;
//            this.ProviderControl.AcceptData = true;

//            if (isConnectedRightNow)
//            {
//                this.threadSync.Release(this.threadSyncToken);
//            }
//        }

//        private bool DoesTextEndsWithAnyPrivilegedModePrompt(string text)
//        {
//            string val = text.TrimEnd();

//            if (val.Length == 0)
//                return false;

//            foreach (string privilegeModePrompt in this.PrivilegeModePromptList)
//            {
//                if (text.TrimEnd().EndsWith(privilegeModePrompt))
//                {
//                    this.privilegeModePrompt = privilegeModePrompt;
//                    return true;
//                }
//            }

//            return false;
//        }

//        private bool DoesTextEndsWithAnyMorePrompt(string text, bool ignoreCase)
//        {
//            bool result = false;
            
//            if (text.Length > 0)
//            {
//                string[] lines = text.Split(new string[] { "\r\n", "\b" }, StringSplitOptions.RemoveEmptyEntries);

//                if (lines.Count() > 0)
//                {
//                    string lastLine = lines[lines.Length - 1].Trim();
//                    result = lastLine.Contains(this.MorePromptList, ignoreCase);
//                }
//            }

//            return result;
//        }

//        private string FilterTerminalData(string terminalData)
//        {
//            StringBuilder result = new StringBuilder();
//            bool isPreviusLineMorePrompt = false;
//            string[] lines;

//            //// TODO: Remove EOT, use TerminalSequence operations instead.
//            //int eotIndex = terminalData.IndexOf((char)27);               // THR TODO: Check EOT vs prompt ending chars.

//            //if (eotIndex >= 0)
//            //{
//            //terminalData = TerminalHelper.RemoveSequences(terminalData);
//            //}

//            lines = terminalData.Split(new string[] { "\r\n", "\n\r", "\b" }, StringSplitOptions.RemoveEmptyEntries);

//            for (int i = 0; i < lines.Length; i++)
//            {
//                bool appendLine = true;
//                string line = lines[i];

//                //if (line.IndexOf((char)27) > -1)
//                //{
//                    //line = TerminalSequences.RemoveSequences(line);
//                //}

//                if (line.Contains(this.MorePromptList, ignoreCase: true))
//                {
//                    appendLine = false;
//                    isPreviusLineMorePrompt = true;
//                }

//                // If previus line was more prompt and this line is empty - no appending
//                if (appendLine)
//                {
//                    if (isPreviusLineMorePrompt)
//                    {
//                        if (line.Trim().Length == 0)
//                        {
//                            appendLine = false;
//                        }

//                        isPreviusLineMorePrompt = false;
//                    }
//                }

//                // If line is last and contain privilege mode prompt - no appending
//                if (appendLine && i == lines.Length - 1 && line.Contains(this.PrivilegeModePromptList, ignoreCase: true))
//                {
//                    appendLine = false;
//                }

//                if (appendLine)
//                {
//                    result.AppendLine(line);
//                }
//            }

//            return result.ToString();
//        }

//        private void SetSettingsToProviderControl()
//        {
//            this.ProviderControl.RemoteHost = this.connectionStringBuilder.RemoteHost;
//            this.ProviderControl.RemotePort = this.connectionStringBuilder.RemotePort;
//            this.ProviderControl.Timeout = this.connectionStringBuilder.Timeout;
//        }

//        private void SetLocalSettings()
//        {
//            this.SetSendingTimerInterval();
//        }

//        private void SetSendingTimerInterval()
//        {
//            this.sendingIntervalTimer.Interval = this.SendingInterval > 0 ? this.SendingInterval : 1;
//        }

//        private void sendingIntervalTimer_Elapsed(object sender, ElapsedEventArgs e)
//        {
//            this.sendingIntervalTimeElapsed = true;

//            if (this.threadSync.ContainsToken(this.threadSendingIntervalToken))
//            {
//                this.threadSync.Release(this.threadSendingIntervalToken);
//            }
//        }

//        private void WriteToLog(string text)
//        {
//            if (this.logging)
//            {
//                this.logTextWriter.Write(text);
//                this.logTextWriter.Flush();
//            }
//        }

//        private void DisposeControl()
//        {
//            if (this.providerControl != null)
//            {
//                (this.providerControl as IDisposable).Dispose();
//                this.providerControl = null;
//            }
//        }

//        #endregion |   Private Methods   |
//    }

//    #region |   Enums   |

//    public enum TerminalProtocol
//    {
//        Default = -1,
//        Telnet = 0,
//        SSH1 = 1,
//        SSH2
//    }

//    public enum WebProtocol
//    {
//        Default = -1,
//        HTTP = 0,
//        // HTTPS = 1
//    }

//    public enum TelnetProviderType
//    {
//        Default = -1,
//        ThoughtNetTelnet = 0,
//        IPWorks = 1
//    }

//    public enum SshProviderType
//    {
//        Default = -1,
//        IPWorks = 0
//    }

//    public enum TerminalControlConnectionStatus
//    {
//        Connected,
//        Disconnected,
//        LoginInProgressWaitingForUsernamePrompt,
//        LoginInProgressWaitingForPasswordPrompt,
//        LoginInProgressWaitingForEnableSecretPrompt,
//        LoginInProgressWaitingForEnableSecretPasswordPrompt,
//        LoginInProgressAfterEnableSecretIsSent
//    }

//    #endregion |   Enums   |

//    #region |   Delegates   |

//    public delegate void TerminalTextEventHandler(object sender, TerminalTextEventArgs e);

//    #endregion |   Delegates   |

//    #region |   Event Args   |

//    public class TerminalTextEventArgs : EventArgs
//    {
//        private string text = String.Empty;

//        public TerminalTextEventArgs(string text)
//        {
//            this.text = text;
//        }

//        public string Text
//        {
//            get { return this.text; }
//            set { this.text = value; }
//        }
//    }

//    #endregion |   Event Args   |

//    #region |   Interfaces   |

//    public interface ITerminalControl
//    {
//        string RemoteHost { get; set; }
//        int RemotePort { get; set; }
//        int Timeout { get; set; }
//        bool AcceptData { get; set; }

//        void Connect();
//        void Disconnect();
//        bool IsConnected { get; }

//        void Send(string command);
//        event TerminalTextEventHandler TextReceived;
//    }

//    #endregion |   Interfaces   |
//}
