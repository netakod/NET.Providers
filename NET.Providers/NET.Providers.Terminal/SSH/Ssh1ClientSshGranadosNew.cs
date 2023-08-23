using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Routrek.SSHC;
using System.Threading.Tasks;
using System.Threading;
using Simple;
using Simple.Network;
using NET.Tools.Telnet;

namespace NET.Tools.Terminal
{
	public class Ssh1ClientSshGranadosNew : TelnetClientBase, ISshClient, ITelnetClient, ISSHConnectionEventReceiver, ISSHChannelEventReceiver, IDisposable
    {
        #region |   Private Members   |

		private SSHConnectionParameter connectionParameter = null;
		private SSHConnection sshConnection = null;
		private SSHChannel channel = null;

		#endregion |   Private Members   |

		#region |   Constructors and Initialization   |

		public Ssh1ClientSshGranadosNew()
        {
  			/*
			string cn = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
			string t1 = Routrek.SSHC.Strings.GetString("NotSSHServer");
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja");
			Routrek.SSHC.Strings.Reload();
			string t2 = Routrek.SSHC.Strings.GetString("NotSSHServer");
			*/

#if false //RSA keygen
			//RSA KEY GENERATION TEST
			byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI");
			RSAKeyPair kp = RSAKeyPair.GenerateNew(2048, new Random());
			byte[] sig = kp.Sign(testdata);
			kp.Verify(sig, testdata);

			new SSH2UserAuthKey(kp).WritePublicPartInOpenSSHStyle(new FileStream("C:\\IOPort\\newrsakey", FileMode.Create));
			//SSH2UserAuthKey newpk = SSH2PrivateKey.FromSECSHStyleFile("C:\\IOPort\\newrsakey", "nedved");
#endif
			
#if false //DSA keygen
			//DSA KEY GENERATION TEST
			byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI 0000");
			DSAKeyPair kp = DSAKeyPair.GenerateNew(2048, new Random());
			byte[] sig = kp.Sign(testdata);
			kp.Verify(sig, testdata);
			new SSH2UserAuthKey(kp).WritePublicPartInOpenSSHStyle(new FileStream("C:\\IOPort\\newdsakey", FileMode.Create));
			//SSH2PrivateKey newpk = SSH2PrivateKey.FromSECSHStyleFile("C:\\IOPort\\newdsakey", "nedved");
#endif
	      }

        #endregion |   Constructors and Initialization   |

        #region |   Events   |

        #endregion

        #region |   Public Properties   |

        //public string RemoteHost { get; set; }
        //public int RemotePort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

		public override bool Connected => !this.sshConnection?.IsClosed ?? false;

		public SSHProtocol Protocol { get; set; } = SSHProtocol.SSH1;

		#endregion |   Public Properties   |

		#region |   Protected Method Overrides   |

		protected override async ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
		{
			this.connectionParameter = new SSHConnectionParameter();
			this.connectionParameter.UserName = this.Username;
			this.connectionParameter.Password = this.Password;
			this.connectionParameter.AuthenticationType = AuthenticationType.Password;
			this.connectionParameter.Protocol = this.Protocol;
			
			if (this.Protocol == SSHProtocol.SSH1)
				this.connectionParameter.PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES, CipherAlgorithm.AES128 };
			else
				this.connectionParameter.WindowSize = 0x1000; // SSH2

			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//socket.Blocking = false;
			await socket.ConnectAsync(remoteEndPoint);
			this.sshConnection = SSHConnection.Connect(this.connectionParameter, this, socket);

#if false //Remote->Local
				_conn.ListenForwardedPort("0.0.0.0", 29472);
#elif false //Local->Remote
				SSHChannel ch = _conn.ForwardPort(reader, "www.yahoo.co.jp", 80, "localhost", 0);
				reader._pf = ch;
				while(!reader._ready) System.Threading.Thread.Sleep(100);
				reader._pf.Transmit(Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n"));
#elif false //SSH over SSH
				f.Password = "okajima";
				SSHConnection con2 = _conn.OpenPortForwardedAnotherConnection(f, reader, "kuromatsu", 22);
				reader._conn = con2;
				SSHChannel ch = con2.OpenShell(reader);
				reader._pf = ch;
#else //normal shell
#endif

			this.channel = this.sshConnection.OpenShell(this);
		}

		protected override async ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var data = buffer.ToArraySegment();

			await this.channel.TransmitAsync(data.Array, data.Offset, data.Count);
		}

		protected override async ValueTask ClientCloseAsync(CloseReason closeReason)
		{
			if (this.sshConnection != null && !this.sshConnection.IsClosed)
			{
				this.sshConnection.Disconnect("bye");
				await this.channel.CloseAsync();
			}

			if (this.channel != null)
				this.channel = null;

			if (this.sshConnection != null)
				this.sshConnection = null;
		}

		protected override ReadOnlyMemory<byte> PrepareTextForSending(string text)
		{
			return this.Encoding.GetBytes(text);
		}

		#endregion |   Protected Method Overrides   |

		#region |   ISSHConnectionEventReceiver interface   |

		void ISSHConnectionEventReceiver.OnDebugMessage(bool always_display, byte[] msg)
		{
		}

		void ISSHConnectionEventReceiver.OnIgnoreMessage(byte[] msg)
		{
		}

		void ISSHConnectionEventReceiver.OnUnknownMessage(byte type, byte[] data)
		{
		}

		void ISSHConnectionEventReceiver.OnError(Exception error, string msg)
		{
		}

		void ISSHConnectionEventReceiver.OnConnectionClosed()
		{
		}

		void ISSHConnectionEventReceiver.OnAuthenticationPrompt(string[] prompts)
		{
			//keyboard-interactive only
		}

		PortForwardingCheckResult ISSHConnectionEventReceiver.CheckPortForwardingRequest(string remote_host, int remote_port, string originator_ip, int originator_port)
		{
			PortForwardingCheckResult portForwardingCheckResult = new PortForwardingCheckResult();
			
			portForwardingCheckResult.allowed = true;
			portForwardingCheckResult.channel = this;
			
			return portForwardingCheckResult;
		}

		void ISSHConnectionEventReceiver.EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel)
		{
			this.channel = channel;
		}

		#endregion |   ISSHConnectionEventReceiver interface   |

		#region |   ISSHChannelEventReceiver interface   |

		void ISSHChannelEventReceiver.OnData(byte[] data, int offset, int length)
		{
			string receivedText = this.Encoding.GetString(data, offset, length);

			this.OnTextReceived(receivedText);
		}

		void ISSHChannelEventReceiver.OnExtendedData(int type, byte[] data)
		{
		}

		void ISSHChannelEventReceiver.OnChannelClosed()
		{
			this.CloseAsync(CloseReason.Unknown);
		}

		void ISSHChannelEventReceiver.OnChannelEOF()
		{
			this.channel.Close();
		}

		void ISSHChannelEventReceiver.OnChannelError(Exception error, string msg)
		{
		}

		void ISSHChannelEventReceiver.OnChannelReady()
		{
		}

		void ISSHChannelEventReceiver.OnMiscPacket(byte packet_type, byte[] data, int offset, int length)
		{
		}

		#endregion |   ISSHChannelEventReceiver interface   |
	}
}
