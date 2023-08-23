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
	/// The Telnet client abstract class.
	/// The Receive/Send implementation is based on async TcpClient example: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
	/// </summary>
	public abstract class TelnetClientBase : ITelnetClient
	{
		//private Socket socket = null;
		//private NetworkStream stream = null;
		private byte[] receiveBuffer = null;
		private TelnetProtocol protocolHandler = null;

		private string[] waitForList = null;
		private bool ignoreCase = false;
		private StringBuilder stringBuilder = new StringBuilder(512);
		private ManualResetEvent waitForFoundResetEvent = new ManualResetEvent(false);

		public TelnetClientBase()
		{
			this.protocolHandler = new TelnetProtocol(telnetClient: this);
		}

		public event TextEventHandler TextReceived;
		public event CloseEventHandler Closed;

		public int ReceiveBuferSize { get; set; } = 1024;
		
		/// <summary>
		/// Receive timeout in miliseconds.
		/// </summary>
		public int ReceiveTimeout { get; set; } = 13000;

		/// <summary>
		/// Send timeout in milliseconds
		/// </summary>
		public int SendTimeout { get; set; } = 13000;

		public Encoding Encoding { get; set; } = new UTF8Encoding(false);

		public abstract bool Connected { get; }

		public CloseReason? CloseReason { get; private set; }

		public abstract EndPoint RemoteEndPoint { get; }

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
			if (this.Connected)
				await this.CloseAsync(Telnet_DevelopOld.CloseReason.LocalClosing);

			await this.ClientConnectAsync(remoteEndPoint, cancellationToken);
			this.OnConnect();
		}

		protected abstract ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken);

		protected void OnConnect()
		{
			this.stringBuilder.Clear();
			this.protocolHandler.Reset();
			this.waitForFoundResetEvent.Close();
			this.receiveBuffer = new byte[this.ReceiveBuferSize];
			this.ClientBeginReceive(this.receiveBuffer, 0, this.ReceiveBuferSize, new AsyncCallback(this.ReadCallback), state: null);
		}

//#nullable enable
		protected abstract IAsyncResult ClientBeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state);

		public async ValueTask<string> SendLineAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			return await this.SendAsync(text + "\r\n", waitForIgnoreCase, cancellationToken, waitFor);
		}

		public async ValueTask<string> SendAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();

				tokenSource.CancelAfter(this.SendTimeout);
				cancellationToken = tokenSource.Token;
			}

			await this.SendAsync(this.Encoding.GetBytes(text), cancellationToken);

			if (waitFor.Length > 0)
				return await this.WaitFor(waitForIgnoreCase, cancellationToken, waitFor);
			else
				return null; // It was nothing to wait for, thus return result is null
		}
		
		public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var dataToSend = this.protocolHandler.Transpose(buffer.ToArray());

			await this.ClientSendAsync(dataToSend, cancellationToken);
		}

		protected abstract ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

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

				tokenSource.CancelAfter(this.ReceiveTimeout);
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
				this.ClientCloseAsync(closeReason);
				this.OnClosed();
			}

#if NETSTANDARD
			return new ValueTask();
#else
			return ValueTask.CompletedTask;
#endif
		}

		protected abstract ValueTask ClientCloseAsync(CloseReason? closeReason);

		protected void OnClosed()
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
				int bytesRead = this.ClientEndReceive(result); // Read data from the client socket.

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

					this.ClientBeginReceive(this.receiveBuffer, 0, this.ReceiveBuferSize, new AsyncCallback(this.ReadCallback), state: null); // Get the rest (next) data.
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

		protected abstract int ClientEndReceive(IAsyncResult result);

		private bool IsWaitForReceived()
		{
			if (this.waitForList == null)
				return false;

			string receivedText = this.stringBuilder.ToString();

			return this.waitForList.Any((waitFor) => receivedText.Contains(waitFor, this.ignoreCase));
		}
	}

	public delegate ValueTask TextEventHandler(string text);
}
