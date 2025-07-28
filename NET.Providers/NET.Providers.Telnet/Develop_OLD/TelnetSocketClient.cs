using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NET.Tools.Telnet_DevelopOld
{
	/// <summary>
	/// The Telnet client implementation based on async TcpClient example: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
	/// </summary>
	public class TelnetSocketClient : ITelnetClient
	{
		private Socket socket = null;
		//private NetworkStream stream = null;
		private byte[] receiveBuffer = null;
		private TelnetProtocol protocolHandler = null;

		private string[] waitForList = null;
		private bool ignoreCase = false;
		private StringBuilder stringBuilder = new StringBuilder(512);
		private ManualResetEvent waitForFoundResetEvent = new ManualResetEvent(false);

		public TelnetSocketClient()
		{
			this.protocolHandler = new TelnetProtocol(telnetClient: this);
		}

		public event TextEventHandler TextReceived;
		public event CloseEventHandler Closed;

		public int ReceiveBufferSize { get; set; } = 1024;
		
		/// <summary>
		/// Timeout in miliseconds.
		/// </summary>
		public int Timeout { get; set; } = 25000;

		public Encoding Encoding { get; set; } = new UTF8Encoding(false);

		public bool Connected => this.socket?.Connected ?? false;
		public CloseReason? CloseReason { get; private set; }

		public EndPoint RemoteEndPoint => this.socket?.RemoteEndPoint;
		public EndPoint LocalEndPoint => this.socket?.LocalEndPoint;


		public async ValueTask ConnectAsync(string remoteHost, int remotePort = 23, CancellationToken cancellationToken = default)
		{
			await this.ConnectAsync(DnsHelper.ResolveIPAddressFromHostname(remoteHost), remotePort, cancellationToken);
		}

		public async ValueTask ConnectAsync(IPAddress remoteIpAddress, int remotePort = 23, CancellationToken cancellationToken = default)
		{
			await this.ConnectAsync(new IPEndPoint(remoteIpAddress, remotePort), cancellationToken);
		}

		public async ValueTask ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
			await this.ConnectAsync(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), remoteEndPoint, cancellationToken);
		}

		public async ValueTask ConnectAsync(Socket socket, IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
			if (this.Connected)
				await this.CloseAsync(Telnet_DevelopOld.CloseReason.LocalClosing);

			this.socket = socket;
			//this.stream = new NetworkStream(this.socket);

			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();
				
				tokenSource.CancelAfter(this.Timeout);
				cancellationToken = tokenSource.Token;
			}

#if NETSTANDARD
			Task result = this.socket.ConnectAsync(remoteEndPoint);               
            int index = Task.WaitAny(new[] { result }, cancellationToken);
            var connected = this.socket.Connected;

			if (!connected)
				this.socket.Close();
#else
			await this.socket.ConnectAsync(remoteEndPoint, cancellationToken);
#endif
			this.OnConnect();
		}

		protected void OnConnect()
		{
			this.stringBuilder.Clear();
			this.protocolHandler.Reset();
			this.waitForFoundResetEvent.Close();
			this.receiveBuffer = new byte[this.ReceiveBufferSize];
			this.socket.BeginReceive(this.receiveBuffer, 0, this.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.ReadCallback), state: null);
		}

		public async ValueTask<string> SendLineAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			return await this.SendAsync(text + "\r\n", waitForIgnoreCase, cancellationToken, waitFor);
		}

		public async ValueTask<string> SendAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();

				tokenSource.CancelAfter(this.Timeout);
				cancellationToken = tokenSource.Token;
			}

			var data = this.Encoding.GetBytes(text);
			var dataToSend = this.protocolHandler.Transpose(data);

			await this.SendAsync(dataToSend, cancellationToken);

			if (waitFor.Length > 0)
				return await this.WaitFor(waitForIgnoreCase, cancellationToken, waitFor);
			else
				return null; // It was nothing to wait for, thus return result is null
		}
		
		public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			await this.socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
		}

		public async ValueTask<string> WaitFor(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
			if (!this.Connected)
				throw new Exception("The client is not connected.");

			if (waitForText.Length == 0) // No nothing to wait for
				return null;

			// WaitFor makes no sense to call from multiple threads simultaneously.
			// So the parameters here is passed as local class variables when processing in OnDataReceive method
			this.ignoreCase = ignoreCase;
			this.waitForList = waitForText;

			string receivedText = String.Empty;

			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();

				tokenSource.CancelAfter(this.Timeout);
				cancellationToken = tokenSource.Token;
			}

			if (this.IsWaitForReceived())
			{
				receivedText = this.stringBuilder.ToString();
			}
			else if (await this.waitForFoundResetEvent.WaitOneAsync(cancellationToken)) // WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, this.waitForFoundResetEvent });
			{
				receivedText = this.stringBuilder.ToString();
			}

			this.waitForList = null;
			this.stringBuilder.Clear();

			return receivedText;
		}

		public ValueTask CloseAsync(CloseReason? closeReason)
		{
			if (this.Connected)
			{
				this.socket.Close();
				this.OnClosed();
			}

#if NETSTANDARD
			return new ValueTask();
#else
			return ValueTask.CompletedTask;
#endif
		}

		protected virtual void OnClosed()
		{
			var closed = this.Closed;

			if (closed == null)
				return;

			if (Interlocked.CompareExchange(ref this.Closed, null, closed) != closed)
				return;

			var closeReason = this.CloseReason.HasValue ? this.CloseReason.Value : Telnet_DevelopOld.CloseReason.Unknown;

			closed.Invoke(closeReason);
		}

		//public async Task WriteAsync(byte[] bytes, CancellationToken cancellationToken = default)
		//{
		//	if (this.stream.CanWrite)
		//		await stream.WriteAsync(bytes, cancellationToken);
		//}

		private void ReadCallback(IAsyncResult result)
		{
			try
			{
				int bytesRead = this.socket.EndReceive(result); // Read data from the client socket.

				//this.Print("RECEIVED: ", this.receiveBuffer, bytesRead);


				if (bytesRead > 0)
				{
					this.protocolHandler.InputFeed(this.receiveBuffer, bytesRead);

					int bytesOfText = this.protocolHandler.Negotiate(this.receiveBuffer);

					if (bytesOfText > 0)
					{
						string receivedText = this.Encoding.GetString(this.receiveBuffer, 0, bytesOfText);

						this.TextReceived?.Invoke(receivedText).ConfigureAwait(false);

						if (this.IsWaitForReceived())
						{
							this.waitForFoundResetEvent.Set(); // Unblock curent WaitFor
							this.waitForFoundResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another WaitFor calls
						}
					}
					
					this.socket.BeginReceive(this.receiveBuffer, 0, this.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.ReadCallback), state: null); // Get the rest (next) of the data.
				}
				else
				{
					// The connection is about to be closed.
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception error when reading from socket stream: " + ex.ToString());
			}
		}

		private void Print(string prefix, byte[] data, int len)
		{
			string text = prefix;

			for (int i = 0; i < len; i++)
				text += " " + data[i].ToString();

			System.Diagnostics.Debug.WriteLine(text);
		}

		private bool IsWaitForReceived()
		{
			if (this.waitForList == null)
				return false;

			string receivedText = this.stringBuilder.ToString();

			return this.waitForList.Any((waitFor) => receivedText.Contains(waitFor, this.ignoreCase));
		}
	}
}
