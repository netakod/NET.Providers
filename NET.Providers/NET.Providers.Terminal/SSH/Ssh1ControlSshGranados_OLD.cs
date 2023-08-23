//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using Routrek.SSHC;
//using NET.Tools.Telnet;

//namespace NET.Tools.Terminal
//{
//	public class Ssh1ControlSshGranados : ISshControl, ITelnetControl, ISSHConnectionEventReceiver, ISSHChannelEventReceiver, IDisposable
//    {
//        #region |   Private Members   |

//		private SSHConnectionParameter connectionParameter = null;
//		private SSHConnection sshConnection = null;
//		private SSHChannel channel = null;
//		private string recievedDataInNoAcceptingDataPeriod = String.Empty;
//        private bool acceptData = true;

//        protected readonly object lockObject = new object();

//        #endregion |   Private Members   |

//        #region |   Constructors and Initialization   |

//		public Ssh1ControlSshGranados()
//        {
//  			/*
//			string cn = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
//			string t1 = Routrek.SSHC.Strings.GetString("NotSSHServer");
//			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja");
//			Routrek.SSHC.Strings.Reload();
//			string t2 = Routrek.SSHC.Strings.GetString("NotSSHServer");
//			*/

//#if false //RSA keygen
//			//RSA KEY GENERATION TEST
//			byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI");
//			RSAKeyPair kp = RSAKeyPair.GenerateNew(2048, new Random());
//			byte[] sig = kp.Sign(testdata);
//			kp.Verify(sig, testdata);

//			new SSH2UserAuthKey(kp).WritePublicPartInOpenSSHStyle(new FileStream("C:\\IOPort\\newrsakey", FileMode.Create));
//			//SSH2UserAuthKey newpk = SSH2PrivateKey.FromSECSHStyleFile("C:\\IOPort\\newrsakey", "nedved");
//#endif
			
//#if false //DSA keygen
//			//DSA KEY GENERATION TEST
//			byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI 0000");
//			DSAKeyPair kp = DSAKeyPair.GenerateNew(2048, new Random());
//			byte[] sig = kp.Sign(testdata);
//			kp.Verify(sig, testdata);
//			new SSH2UserAuthKey(kp).WritePublicPartInOpenSSHStyle(new FileStream("C:\\IOPort\\newdsakey", FileMode.Create));
//			//SSH2PrivateKey newpk = SSH2PrivateKey.FromSECSHStyleFile("C:\\IOPort\\newdsakey", "nedved");
//#endif
//	      }

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
//			get { return (this.sshConnection != null) ? !this.sshConnection.IsClosed : false; }
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
//			if (this.sshConnection != null)
//            {
//                this.Disconnect();
//            }

//            lock (this.lockObject)
//            {
//				this.connectionParameter = new SSHConnectionParameter();
//				this.connectionParameter.UserName = this.Username;
//				this.connectionParameter.Password = this.Password;
//				this.connectionParameter.AuthenticationType = AuthenticationType.Password;
//				this.connectionParameter.Protocol = SSHProtocol.SSH1;
//				// SSH1
//				this.connectionParameter.PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES, CipherAlgorithm.AES128 };
//				//SSH2
//				//this.connectionParameter.WindowSize = 0x1000;

//				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//				//socket.Blocking = false;
//				socket.Connect(new IPEndPoint(IPAddress.Parse(this.RemoteHost), this.RemotePort));
//				this.sshConnection = SSHConnection.Connect(this.connectionParameter, this, socket);

//#if false //Remote->Local
//				_conn.ListenForwardedPort("0.0.0.0", 29472);
//#elif false //Local->Remote
//				SSHChannel ch = _conn.ForwardPort(reader, "www.yahoo.co.jp", 80, "localhost", 0);
//				reader._pf = ch;
//				while(!reader._ready) System.Threading.Thread.Sleep(100);
//				reader._pf.Transmit(Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n"));
//#elif false //SSH over SSH
//				f.Password = "okajima";
//				SSHConnection con2 = _conn.OpenPortForwardedAnotherConnection(f, reader, "kuromatsu", 22);
//				reader._conn = con2;
//				SSHChannel ch = con2.OpenShell(reader);
//				reader._pf = ch;
//#else //normal shell
//#endif

//				this.channel = this.sshConnection.OpenShell(this);
//            }
//        }

//        public void Disconnect()
//        {
//            lock (this.lockObject)
//            {
//                if (this.sshConnection != null && !this.sshConnection.IsClosed)
//                {
//                    this.sshConnection.Disconnect("bye");
//                    this.channel.Close();
//                }

//                if (this.channel != null)
//                    this.channel = null;

//                if (this.sshConnection != null)
//                    this.sshConnection = null;
//            }
//        }

//        public void SendAsync(string command)
//        {
//			if (this.Connected)
//			{
//				byte[] buffer = Encoding.ASCII.GetBytes(command);
//				this.channel.Transmit(buffer, 0, buffer.Length);
//			}
//        }

//        public void Dispose()
//        {
//            if (this.Connected)
//                this.Disconnect();
//        }

//        #endregion |   Public Methods   |

//        #region |   Private Methods   |

//        #endregion |   Private Methods   |

//        #region |   Private Raise Events Methods   |

//        private void RaiseTextReceived(string text) => this.TextReceived?.Invoke(text);

//        #endregion |   Private Raise Events Methods   |

//		#region |   ISSHConnectionEventReceiver interface   |

//		void ISSHConnectionEventReceiver.OnDebugMessage(bool always_display, byte[] msg)
//		{
//		}

//		void ISSHConnectionEventReceiver.OnIgnoreMessage(byte[] msg)
//		{
//		}

//		void ISSHConnectionEventReceiver.OnUnknownMessage(byte type, byte[] data)
//		{
//		}

//		void ISSHConnectionEventReceiver.OnError(Exception error, string msg)
//		{
//		}

//		void ISSHConnectionEventReceiver.OnConnectionClosed()
//		{
//		}

//		void ISSHConnectionEventReceiver.OnAuthenticationPrompt(string[] prompts)
//		{
//			//keyboard-interactive only
//		}

//		PortForwardingCheckResult ISSHConnectionEventReceiver.CheckPortForwardingRequest(string remote_host, int remote_port, string originator_ip, int originator_port)
//		{
//			PortForwardingCheckResult portForwardingCheckResult = new PortForwardingCheckResult();
			
//			portForwardingCheckResult.allowed = true;
//			portForwardingCheckResult.channel = this;
			
//			return portForwardingCheckResult;
//		}

//		void ISSHConnectionEventReceiver.EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel)
//		{
//			this.channel = channel;
//		}

//		#endregion |   ISSHConnectionEventReceiver interface   |

//		#region |   ISSHChannelEventReceiver interface   |

//		void ISSHChannelEventReceiver.OnData(byte[] data, int offset, int length)
//		{
//			lock (this.lockObject)
//			{
//				if (this.acceptData)
//				{
//					this.RaiseTextReceived(Encoding.ASCII.GetString(data, offset, length));
//				}
//				else
//				{
//					this.recievedDataInNoAcceptingDataPeriod += Encoding.ASCII.GetString(data, offset, length);
//				}
//			}
//		}

//		void ISSHChannelEventReceiver.OnExtendedData(int type, byte[] data)
//		{
//		}

//		void ISSHChannelEventReceiver.OnChannelClosed()
//		{
//			this.Disconnect();
//		}

//		void ISSHChannelEventReceiver.OnChannelEOF()
//		{
//			this.channel.Close();
//		}

//		void ISSHChannelEventReceiver.OnChannelError(Exception error, string msg)
//		{
//		}

//		void ISSHChannelEventReceiver.OnChannelReady()
//		{
//		}

//		void ISSHChannelEventReceiver.OnMiscPacket(byte packet_type, byte[] data, int offset, int length)
//		{
//		}

//		#endregion |   ISSHChannelEventReceiver interface   |
//	}
//}
