using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using Renci.SshNet;
using Renci.SshNet.Common;
using Simple;
using Simple.Network;
using NET.Tools.Telnet;

namespace NET.Tools.Terminal
{
    public class Ssh2ClientSshNetNew : TelnetClientBase, ISshClient, ITelnetClient, IDisposable
    {
        #region |   Private Members   |

        private ConnectionInfo connectionInfo = null;
        private SshClient sshClient = null;
        private ShellStream stream = null;
        private StreamWriter writer = null;
        //private string recievedDataInNoAcceptingDataPeriod = String.Empty;
		private byte[] receiveBuffer = null;
  //      private int receiveBufferSize = 1024;
        //private int readingInterval = 100;

		//private bool acceptData = true;
        private uint streamColumns = 255;
        private uint streamRows = 16384;
        private uint streamWidth = 255;
        private uint streamHeight = 255;
        private int stremBufferSize = 16384;

        //private readonly object lockObject = new object();

        #endregion |   Private Members   |

        #region |   Constructors and Initialization   |

        public Ssh2ClientSshNetNew() { }

        #endregion |   Constructors and Initialization   |

        #region |   Public Properties   |

        //public string RemoteHost { get; set; }
        //public int RemotePort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public override bool Connected => this.sshClient?.IsConnected ?? false;

        #endregion |   Public Properties   |

        #region |   Protected Method Overrides   |

        protected override async ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
            if (this.sshClient != null)
                await this.CloseAsync(CloseReason.LocalClosing);

            AuthenticationMethod method = new PasswordAuthenticationMethod(this.Username, this.Password);
            this.connectionInfo = new ConnectionInfo(remoteEndPoint.Address.ToString(), remoteEndPoint.Port, this.Username, method);
            this.sshClient = new SshClient(this.connectionInfo);

            Task result = Task.Run(() => this.sshClient.Connect());
            int index = Task.WaitAny(new[] { result }, cancellationToken);

            this.stream = sshClient.CreateShellStream("xterm", this.streamColumns, this.streamRows, this.streamWidth, this.streamHeight, this.stremBufferSize);

            if (cancellationToken != default && cancellationToken != CancellationToken.None && cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("Cancelation requested");

            this.writer = new StreamWriter(stream);
            this.writer.AutoFlush = true;

            if (cancellationToken != default && cancellationToken != CancellationToken.None && cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("Cancelation requested");
        }

        protected override void OnConnect()
        {
            base.OnConnect();

            this.receiveBuffer = new byte[this.ReceiveBufferSize];
            this.stream.BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null);
        }

        protected override async ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var data = buffer.ToArraySegment();
            
            await this.stream.WriteAsync(data.Array, data.Offset, data.Count);
        }

        protected override async ValueTask ClientCloseAsync(CloseReason closeReason)
		{
            if (this.sshClient != null && this.sshClient.IsConnected)
            {
                this.sshClient.Disconnect();
                this.stream.Close();
                //this.stream.DataReceived -= new EventHandler<ShellDataEventArgs>(stream_DataReceived);
                this.writer.Close();
            }

            if (this.stream != null)
            {
                await this.stream.DisposeAsync();
                this.stream = null;
            }

            if (this.writer != null)
            {
                await this.writer.DisposeAsync();
                this.writer = null;
            }

            if (this.sshClient != null)
            {
                this.sshClient.Dispose();
                this.sshClient = null;
            }
        }

		protected override ReadOnlyMemory<byte> PrepareTextForSending(string text)
		{
            return this.connectionInfo.Encoding.GetBytes(text);
		}

		#endregion |   Protected Method Overrides   |

		#region |   Private Method   |

		private void ReadCallback(IAsyncResult result)
        {
            try
            {
                int bytesRead = this.stream.EndRead(result); // Read data from the client sream.

                if (bytesRead > 0)
                {
                    string receivedText = this.connectionInfo.Encoding.GetString(this.receiveBuffer, 0, bytesRead);

                    this.OnTextReceived(receivedText);
                    this.stream.BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null); // Get the rest (next) data.
                }
                else
                {
                    // The connection is about to be closed.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception error when reading from stream: " + ex.ToString());
            }
        }

        #endregion |   Private Method   |
    }
}
