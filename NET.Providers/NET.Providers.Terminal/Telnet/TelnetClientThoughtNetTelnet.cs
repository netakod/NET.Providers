using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Telnet;
using Thought.Net.Telnet;

namespace NET.Tools.Terminal
{
    public class TelnetClientThoughtNetTelnet : ITelnetClient, IDisposable //, ITelnetControl
    {
        #region |   Private Members   |

        private Thought.Net.Telnet.TelnetClient telnet = null;
        private Socket socket = null;
        private TelnetStream stream = null;
        private byte[] buffer = null;
        private string recievedData = String.Empty;
        private Thread readingThread = null;
        //private StreamReader reader = null;
        //private StreamWriter writer = null;
        private bool isConnected = false;
        private bool acceptData = true;
        private int defaultRecieveBufferSize = 4096;
        private int readingInterval = 30;
        //private AsyncCallback onReadCompletedAsyncCallback = null;
        //private IAsyncResult asyncResult = null;
        //private bool localEcho = true;
        //Dim backgroundThread As New Thread(AddressOf BackgroundOutputThread)
        //backgroundThread.Start()
        private readonly object lockObject = new object();

        #endregion |   Private Members   |

        #region |   Constructors and Initialization   |

        public TelnetClientThoughtNetTelnet()
        {
            //this.onReadCompletedAsyncCallback = new AsyncCallback(this.OnReadCompleted);
        }

        #endregion |   Constructors and Initialization   |

        #region |   Events   |

        public event TextEventHandlerAsync TextReceived;
        public event CloseEventHandlerAsync Closed;

        #endregion

        #region |   Public Properties   |

        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }

        /// <summary>
        /// Timeout in seconds.
        /// </summary>
        public int Timeout { get; set; }

        public bool Connected
        {
            get { return this.isConnected; }
        }

        public bool AcceptData
        {
            get { return this.acceptData; }
            set
            {
                lock (this.lockObject)
                {
                    this.acceptData = value;
                }
            }
        }

        #endregion |   Public Properties   |

        #region |   Private Properties   |

        #endregion |   Private Properties   |

        #region |   Public Methods   |

        public void Connect()
        {
            if (this.telnet != null)
            {
                this.Disconnect();
            }

            lock (this.lockObject)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Connect(this.RemoteHost, this.RemotePort);

                this.telnet = new Thought.Net.Telnet.TelnetClient(this.socket);
                this.stream = new TelnetStream(this.telnet);
                
                this.telnet.CommandReceived += new TelnetCommandEventHandler(telnet_CommandReceived);
                this.telnet.DoReceived += new TelnetOptionEventHandler(telnet_DoReceived);
                this.telnet.DontReceived += new TelnetOptionEventHandler(telnet_DontReceived);
                this.telnet.WillReceived += new TelnetOptionEventHandler(telnet_WillReceived);
                this.telnet.WontReceived += new TelnetOptionEventHandler(telnet_WontReceived);

                //this.telnet.SendDont(TelnetOption.Echo);

                
                this.recievedData = String.Empty;
                this.isConnected = true;
                
                this.buffer = new byte[this.defaultRecieveBufferSize];
                this.readingThread = new Thread(() => this.ReadStreamData());
                this.readingThread.Priority = ThreadPriority.Normal;
                this.readingThread.Start();
                //this.onReadCompletedAsyncCallback = new AsyncCallback(this.OnReadCompleted);
                //this.stream.BeginRead(this.buffer, 0, this.buffer.Length, this.onReadCompletedAsyncCallback, this.stream);

            }
        }

        public async ValueTask ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
            if (this.telnet != null)
                await this.CloseAsync(CloseReason.LocalClosing);

            this.RemoteHost = remoteEndPoint.Address.ToString();
            this.RemotePort = remoteEndPoint.Port;

			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await this.socket.ConnectAsync(this.RemoteHost, this.RemotePort);

            this.telnet = new Thought.Net.Telnet.TelnetClient(this.socket);
            this.stream = new TelnetStream(this.telnet);

            this.telnet.CommandReceived += new TelnetCommandEventHandler(telnet_CommandReceived);
            this.telnet.DoReceived += new TelnetOptionEventHandler(telnet_DoReceived);
            this.telnet.DontReceived += new TelnetOptionEventHandler(telnet_DontReceived);
            this.telnet.WillReceived += new TelnetOptionEventHandler(telnet_WillReceived);
            this.telnet.WontReceived += new TelnetOptionEventHandler(telnet_WontReceived);

            //this.telnet.SendDont(TelnetOption.Echo);


            this.recievedData = String.Empty;
            this.isConnected = true;

            this.buffer = new byte[this.defaultRecieveBufferSize];
            this.readingThread = new Thread(() => this.ReadStreamData());
            this.readingThread.Priority = ThreadPriority.Normal;
            this.readingThread.Start();
        }

        public async ValueTask CloseAsync(CloseReason closeReason)
		{
            if (this.telnet != null)
                this.telnet.SendDo(Thought.Net.Telnet.TelnetOption.Logout);

            if (this.socket != null)
            {
                this.socket.LingerState.Enabled = true;
                this.socket.LingerState.LingerTime = 0;
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Disconnect(reuseSocket: false);
                this.socket.Close();
            }

            if (this.telnet != null)
            {
                this.telnet.Close();
                this.telnet.DoReceived -= new TelnetOptionEventHandler(telnet_DoReceived);
                this.telnet.DontReceived -= new TelnetOptionEventHandler(telnet_DontReceived);
                this.telnet.WillReceived -= new TelnetOptionEventHandler(telnet_WillReceived);
                this.telnet.WontReceived -= new TelnetOptionEventHandler(telnet_WontReceived);
                this.telnet.CommandReceived -= new TelnetCommandEventHandler(telnet_CommandReceived);
            }

            //this.onReadCompletedAsyncCallback = null;

            this.isConnected = false;

            if (this.Closed != null)
                await this.Closed(closeReason);

            lock (this.lockObject)
            {

                if (this.readingThread != null)
                {
                    Thread.Sleep(this.readingInterval * 2);

                    //this.readingThread.Abort();
                }
            }

            if (this.stream != null)
            {
                this.stream.Close();
                await this.stream.DisposeAsync();
            }

            this.stream = null;
            this.socket = null;
            this.telnet = null;
            this.buffer = null;
            this.readingThread = null;

        }


        public void Disconnect()
        {
            //lock (this.lockObject)
            //{

                if (this.telnet != null)
                    this.telnet.SendDo(Thought.Net.Telnet.TelnetOption.Logout);

                //this.stream = null;
                //this.socket = null;
                //this.telnet = null;
                //this.buffer = null;

                //return;

                //if (this.stream != null)
                //{
      //              if (this.readingThread != null)
      //              {
      //                  this.readingThread.Abort();
						//Thread.Sleep(this.readingInterval);
						//this.readingThread = null;
      //              }
                    
                    //if (this.asyncResult != null && this.isReadBegin)
                    //{
                    //    //this.stream.EndWrite(this.asyncResult);
                    //    this.stream.EndRead(this.asyncResult);
                    //    this.isReadBegin = false;
                    //}
                    //try
                    //{
                    //    this.stream.EndWrite(this.asyncResult);
                    //}
                    //catch { }

                    //try
                    //{
                    //    this.stream.EndRead(this.asyncResult);
                    //}
                    //catch { }

                //    this.stream.Close();
                //}

                if (this.socket != null)
                {
                    //if (this.socket.Connected)
                    //{
                    //    LingerOption lingOpt = new LingerOption(false, 0);
                    //    this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingOpt);
                    //}
                    this.socket.LingerState.Enabled = true;
                    this.socket.LingerState.LingerTime = 0;
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Disconnect(reuseSocket: false);
                    this.socket.Close();
                }

                if (this.telnet != null)
                {   
                    this.telnet.Close();
                    this.telnet.DoReceived -= new TelnetOptionEventHandler(telnet_DoReceived);
                    this.telnet.DontReceived -= new TelnetOptionEventHandler(telnet_DontReceived);
                    this.telnet.WillReceived -= new TelnetOptionEventHandler(telnet_WillReceived);
                    this.telnet.WontReceived -= new TelnetOptionEventHandler(telnet_WontReceived);
                    this.telnet.CommandReceived -= new TelnetCommandEventHandler(telnet_CommandReceived);
                }

			//this.onReadCompletedAsyncCallback = null;

				this.isConnected = false;

				lock (this.lockObject)
				{

					if (this.readingThread != null)
					{
						Thread.Sleep(this.readingInterval * 2);
						
                        //this.readingThread.Abort();
					}
				}

				if (this.stream != null)
				{
					this.stream.Close();
					this.stream.Dispose();
				}

                this.stream = null;
                this.socket = null;
                this.telnet = null;
                this.buffer = null;
				this.readingThread = null;
			//}
        }

        public void SendAsync(string command)
        {
			if (this.Connected)
			{
				byte[] buffer = Encoding.ASCII.GetBytes(command);
				this.stream.Write(buffer, 0, buffer.Length);
			}
        }


        public async ValueTask SendAsync(string text, CancellationToken cancellationToken = default)
		{
            byte[] buffer = Encoding.ASCII.GetBytes(text);

            await this.SendAsync(buffer, cancellationToken);

        }


        public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
            await this.stream.WriteAsync(buffer.ToArraySegment().Array, 0, buffer.Length, cancellationToken);
        }

        public string Read()
		{
			if (!this.Connected)
				return null;

			string text = String.Empty;

			while (this.socket.Available > 0)
			{
				int bytesRead = this.stream.Read(this.buffer, 0, this.buffer.Length);

				if (bytesRead > 0)
					text += Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

				Thread.Sleep(this.readingInterval);
			}

			this.AppendReceivedText(text);

			return text;
		}

		public string WaitFor(params string[] waitForText)
		{
			return this.WaitFor(ignoreCase: true, waitForText: waitForText);
		}

		public string WaitFor(bool ignoreCase, params string[] waitForText)
		{
			if (!this.Connected)
				return null;

			string text = String.Empty;

			while (true)
			{
				int bytesRead = this.stream.Read(this.buffer, 0, this.buffer.Length);

				if (bytesRead > 0)
					text += Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

				if (text.ContainsAny(waitForText, ignoreCase))
					break;

				Thread.Sleep(this.readingInterval);
			}

			this.AppendReceivedText(text);

			return text;
		}

        public async ValueTask<string> WaitForAsync(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
            if (!this.Connected)
                return null;

            string text = String.Empty;

            while (true)
            {
                int bytesRead = await this.stream.ReadAsync(this.buffer, 0, this.buffer.Length, cancellationToken);

                if (bytesRead > 0)
                    text += Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

                if (text.ContainsAny(waitForText, ignoreCase))
                    break;

                Thread.Sleep(this.readingInterval);
            }

            this.AppendReceivedText(text);

            return text;
        }


        public void Dispose()
        {
            if (this.Connected)
                this.Disconnect();
		}

        public async ValueTask DisposeAsync()
		{
            if (this.Connected)
                await this.CloseAsync(CloseReason.LocalClosing);
		}

		#endregion |   Public Methods   |

		#region |   Protected Methods   |

		protected virtual string ProcessTerminalData(string receivedData)
		{
			//TODO: Parse received data and filter special command

			return receivedData;
		}

		#endregion |   Protected Methods   |

		#region |   Private Methods   |

		private void ReadStreamData()
        {
            string text = string.Empty;
            int bytesRead = 0;

            while (this.Connected)
            {
                bytesRead = 0;

				lock (this.lockObject)
				{
					if (this.stream != null)
					{
						try
						{
							bytesRead = this.stream.Read(this.buffer, 0, this.buffer.Length);
						}
						catch // Catch the exception when disconnecting by aborting reading thread (this.readingThread).
						{
						}

						if (bytesRead > 0)
						{
							text = Encoding.ASCII.GetString(this.buffer, 0, bytesRead);

							if (!text.IsNullOrEmpty())
							{
								if (this.AcceptData)
								{
									this.RaiseTextReceived(this.recievedData + text);
									this.recievedData = String.Empty;
								}
								else
								{
									this.recievedData += text;
								}
							}
						}
					}

					Thread.Sleep(this.readingInterval);
				}

                if (this.AcceptData && this.recievedData.Length > 0)
                {
                    this.RaiseTextReceived(this.recievedData);
                    this.recievedData = String.Empty;
                }
            }
        }

        
        //private void OnReadCompleted(IAsyncResult asyncResult)
        //{
        //    // Important:
        //    // EndRead() -is- supposed to block until the end of reading the buffer (that piece of the file),
        //    // but we should have a plenty good window for the originating thread to get in there
        //    // even though it's just the first read...but maybe doesn't get a chance?
        //    // Seems odd it would be that way every single time.

        //    int bytesRead = 0;
        //    string text = String.Empty;
        //    this.asyncResult = asyncResult;

        //    if (this.stream != null)
        //    {
        //        lock (this.lockObject)
        //        {
        //            bytesRead = this.stream.EndRead(asyncResult);
        //        }


        //        if (bytesRead != 0)
        //        {
        //            text = Encoding.ASCII.GetString(this.buffer, 0, bytesRead);
        //            // my this output could get large
        //            // Console.WriteLine(s);

        //            // This kind of calling will cause a stack overflow in the case of
        //            // large files with small buffers, but is not the real issue
        //            // about the seemingly non blocking read...or is it?
        //            //this.stream.BeginRead(buffer, 0, buffer.Length, this.onReadCompletedAsyncCallback, null);

        //        }  // end if-else



        //        lock (this.lockObject)
        //        {
        //            if (this.stream != null)
        //            {
        //                this.stream.BeginRead(this.buffer, 0, this.buffer.Length, this.onReadCompletedAsyncCallback, null);
        //                this.asyncResult = null;
        //            }
        //        }
        //    }
        //}

        private void stream_BytesRead(object sender, StreamActionEventArgs e)
        {
            if (!this.isConnected)
                return;

            this.RaiseTextReceived(Encoding.ASCII.GetString(e.Data));
        }
        
        private void telnet_CommandReceived(object sender, TelnetCommandEventArgs e)
        {
        }
        
        private void telnet_DoReceived(object sender, TelnetOptionEventArgs e)
        {
            if (!this.isConnected)
                return;

            //try
            //{
                this.telnet.SendWont(e.Option);
            //}
            //catch { }
        }
        
        private void telnet_DontReceived(object sender, TelnetOptionEventArgs e)
        {
        }
        
        private void telnet_WillReceived(object sender, TelnetOptionEventArgs e)
        {
            if (!this.isConnected)
                return;

            if (e.Option == Thought.Net.Telnet.TelnetOption.Echo)
            {
                // The server sent a WILL ECHO command.  This means the
                // server is willing to ECHO typed characters (instead of
                // the local client doing so).  This is a common option 
                // because the server can then fully control the output
                // of typed characters.
                //
                // This client agrees to let the server handle echoing.
                // Local echoing is turned off.
                
                e.Agreed = true;
                //this.localEcho = false;

            }
            else if (e.Option == Thought.Net.Telnet.TelnetOption.Encrypt)
            {
                e.Agreed = true;
                this.telnet.SendDont(Thought.Net.Telnet.TelnetOption.Encrypt);
            }
        }

        private void telnet_WontReceived(object sender, TelnetOptionEventArgs e)
        {
            if (!this.isConnected)
                return;

            if (e.Option == Thought.Net.Telnet.TelnetOption.Echo)
            {
                // The server sent a WONT ECHO command.  This means
                // it refuses to continue echoing typed characters.
                e.Agreed = true;
                this.telnet.SendDont(Thought.Net.Telnet.TelnetOption.Echo);
                //this.localEcho = true;
            }
        }


		private void AppendReceivedText(string text)
		{
			string processedData = this.ProcessTerminalData(text);

			this.recievedData += processedData;

			if (this.AcceptData)
			{
				this.RaiseTextReceived(this.recievedData);
				this.recievedData = String.Empty;
			}
		}





		//private void telnet_OnDo(object sender, TelnetDoEventArgs e)
		//{
		//    try
		//    {
		//        this.telnet.WontOption = e.OptionCode;
		//    }
		//    catch (IPWorksException)
		//    {
		//    }
		//}

		//private void telnet_OnWill(object sender, TelnetWillEventArgs e)
		//{
		//    try
		//    {
		//        if (e.OptionCode == 38)
		//        {
		//            this.telnet.DontOption = 38;
		//        }
		//    }
		//    catch (IPWorksException)
		//    {
		//    }
		//}


		#endregion |   Private Methods   |

		#region |   Private Raise Events Methods   |

		private void RaiseTextReceived(string text)
        {
			//System.Diagnostics.Debug.WriteLine("RECEIVED: " + text);

			this.TextReceived?.Invoke(text);
        }

        #endregion |   Private Raise Events Methods   |
    }
}
