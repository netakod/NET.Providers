//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Threading;
//using Renci.SshNet;
//using Renci.SshNet.Common;
//using NET.Tools.Telnet;

//namespace NET.Tools.Terminal
//{
//    public class Ssh2ControlSshNet_OLD : ISshControl, ITelnetControl, IDisposable
//    {
//        #region |   Private Members   |

//        private ConnectionInfo connectionInfo = null;
//        private SshClient sshClient = null;
//        private ShellStream stream = null;
//        private StreamWriter writer = null;
//        private string recievedDataInNoAcceptingDataPeriod = String.Empty;
//		private byte[] buffer = null;
//		private int readingInterval = 100;

//		private bool acceptData = true;
//        private uint streamColumns = 255;
//        private uint streamRows = 16384;
//        private uint streamWidth = 255;
//        private uint streamHeight = 255;
//        private int stremBufferSize = 16384;

//        private readonly object lockObject = new object();

//        #endregion |   Private Members   |

//        #region |   Constructors and Initialization   |

//        public Ssh2ControlSshNet_OLD()
//        {
//        }

//        #endregion |   Constructors and Initialization   |

//        #region |   Events   |

//        public event TextEventHandler TextReceived;

//        #endregion

//        #region |   Public Properties   |

//        public string RemoteHost { get; set; }
//        public int RemotePort { get; set; }
//        public string Username { get; set; }
//        public string Password { get; set; }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout { get; set; }

//        public bool Connected
//        {
//            get { return (this.sshClient != null) ? this.sshClient.IsConnected : false; }
//        }

//        public bool AcceptData
//        {
//            get { return this.acceptData; }
//            set
//            {
//                if (this.acceptData == value)
//                    return;
                
//                lock (this.lockObject)
//                {
//                    this.acceptData = value;
//                }

//                if (this.acceptData && this.recievedDataInNoAcceptingDataPeriod.Length > 0)
//                {
//                    this.RaiseTextReceived(this.recievedDataInNoAcceptingDataPeriod);
//                    this.recievedDataInNoAcceptingDataPeriod = string.Empty;
//                }
//            }
//        }

//        #endregion |   Public Properties   |

//        #region |   Public Methods   |

//        public void Connect()
//        {
//            if (this.sshClient != null)
//                this.Disconnect();

//            lock (this.lockObject)
//            {
//                AuthenticationMethod method = new PasswordAuthenticationMethod(this.Username, this.Password);
//                this.connectionInfo = new ConnectionInfo(this.RemoteHost, this.RemotePort, this.Username, method);
//                this.sshClient = new SshClient(this.connectionInfo);

//                this.sshClient.Connect();

//                this.stream = sshClient.CreateShellStream("xterm", this.streamColumns, this.streamRows, this.streamWidth, this.streamHeight, this.stremBufferSize);
//                this.stream.DataReceived += new EventHandler<ShellDataEventArgs>(stream_DataReceived);

//                this.writer = new StreamWriter(stream);
//                this.writer.AutoFlush = true;
//            }
//        }

//        public void Disconnect()
//        {
//            lock (this.lockObject)
//            {
//                if (this.sshClient != null && this.sshClient.IsConnected)
//                {
//                    this.sshClient.Disconnect();
//                    this.stream.Close();
//                    this.stream.DataReceived -= new EventHandler<ShellDataEventArgs>(stream_DataReceived);
//                    this.writer.Close();
//                }

//                if (this.stream != null)
//                {
//                    this.stream.Dispose();
//                    this.stream = null;
//                }

//                if (this.writer != null)
//                {
//                    this.writer.Dispose();
//                    this.writer = null;
//                }

//                if (this.sshClient != null)
//                {
//                    this.sshClient.Dispose();
//                    this.sshClient = null;
//                }
//            }
//        }

//		public string Read()
//		{
//			if (!this.Connected)
//				return null;

//			string receivedText = String.Empty;

//			while (this.stream.DataAvailable)
//			{
//				int bytesRead = this.stream.Read(this.buffer, 0, this.buffer.Length);

//				if (bytesRead > 0)
//					receivedText += this.connectionInfo.Encoding.GetString(this.buffer, 0, bytesRead);
//				//Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

//				Thread.Sleep(this.readingInterval);
//			}

//			this.AppendReceivedText(receivedText);

//			return receivedText;
//		}

//		public string WaitFor(params string[] waitForText)
//		{
//			return this.WaitFor(ignoreCase: true, waitForText: waitForText);
//		}

//		public string WaitFor(bool ignoreCase, params string[] waitForText)
//		{
//			if (!this.Connected)
//				return null;

//			string text = String.Empty;

//			while (true)
//			{
//				int bytesRead = this.stream.Read(this.buffer, 0, this.buffer.Length);

//				if (bytesRead > 0)
//					text += this.connectionInfo.Encoding.GetString(this.buffer, 0, bytesRead);
//				//Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

//				if (text.ContainsAny(waitForText, ignoreCase))
//					break;

//				Thread.Sleep(this.readingInterval);
//			}

//			this.AppendReceivedText(text);

//			return text;
//		}

//		public void SendAsync(string command)
//        {
//			if (this.Connected)
//			{
//				//byte[] buffer = this.connectionInfo.Encoding.GetBytes(command);
//				//this.stream.Write(buffer, 0, buffer.Length);
//				this.stream.Write(command);
//			}
//        }

//        public void Dispose()
//        {
//            if (this.Connected)
//                this.Disconnect();
//        }

//		#endregion |   Public Methods   |

//		#region |   Private Methods   |

//		private void AppendReceivedText(string text)
//		{
//			this.recievedDataInNoAcceptingDataPeriod += text;

//			if (this.AcceptData)
//			{
//				this.RaiseTextReceived(this.recievedDataInNoAcceptingDataPeriod);
//				this.recievedDataInNoAcceptingDataPeriod = String.Empty;
//			}
//		}

//		private void stream_DataReceived(object sender, ShellDataEventArgs e)
//        {
//			//lock (this.lockObject)
//			//{

//			string recievedText = connectionInfo.Encoding.GetString(e.Data);

//				if (this.acceptData)
//				{
//					this.RaiseTextReceived(connectionInfo.Encoding.GetString(e.Data));
//				}
//				else
//				{
//					this.recievedDataInNoAcceptingDataPeriod += connectionInfo.Encoding.GetString(e.Data);
//				}
//			//}
//        }

//        #endregion |   Private Methods   |

//        #region |   Private Raise Events Methods   |

//        private void RaiseTextReceived(string text) => this.TextReceived?.Invoke(text);

//        #endregion |   Private Raise Events Methods   |
//    }
//}
