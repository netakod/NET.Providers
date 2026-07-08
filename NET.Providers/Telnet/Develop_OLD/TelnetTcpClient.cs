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
	public class TelnetTcpClient : ITelnetClient, IDisposable
	{
		private TcpClient client = null;
		private TelnetProtocol protocolHandler = null;
		private byte[] receiveBuffer = null;

		private string[] waitForList = null;
		private bool ignoreCase = false;
		private StringBuilder stringBuilder = new StringBuilder(512);
		private ManualResetEvent waitForFoundResetEvent = new ManualResetEvent(false);

		public TelnetTcpClient()
		{
			this.protocolHandler = new TelnetProtocol(telnetClient: this);
		}

		public int ReceiveBufferSize { get; set; } = 1024;

		/// <summary>
		/// Timeout in miliseconds.
		/// </summary>
		public int ReceiveTimeout { get; set; } = 13000;
		public int SendTimeout { get; set; } = 13000;

		public bool Connected => this.client?.Connected ?? false;

		public CloseReason? CloseReason { get; private set; }
		public EndPoint RemoteEndPoint { get; private set; }
		//public EndPoint LocalEndPoint => this.socket?.LocalEndPoint;

		public Encoding Encoding { get; set; } = new UTF8Encoding(false);
		
		public event TextEventHandler TextReceived;
		public event CloseEventHandler Closed;

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

			//this.RemoteHost = remoteEndPoint.Address;
			//this.RemotePort = remoteEndPoint.Port;
			this.RemoteEndPoint = remoteEndPoint;

			this.client = new TcpClient();
			this.client.ReceiveBufferSize = this.ReceiveBufferSize;
			this.client.ReceiveTimeout = this.ReceiveTimeout;
			this.client.SendTimeout = this.SendTimeout;

#if NETSTANDARD
			Task result = this.client.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);               
            int index = Task.WaitAny(new[] { result }, cancellationToken);
            var connected = this.client.Connected;

			if (!connected)
				this.client.Close();
#else
			await this.client.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port, cancellationToken);
#endif
			this.OnConnect();
		}

		protected void OnConnect()
		{
			this.stringBuilder.Clear();
			this.protocolHandler.Reset();
			//this.waitForFoundResetEvent.Close();
			this.receiveBuffer = new byte[this.ReceiveBufferSize];
			this.client.GetStream().BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null);
		}

		private void ReadCallback(IAsyncResult result)
		{
			try
			{
				NetworkStream stream = this.client.GetStream();

				int bytesRead = stream.EndRead(result); // Read data from the client socket.

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

					stream.BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null); // Get the rest of the data (next data).
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

		private bool IsWaitForReceived()
		{
			if (this.waitForList == null)
				return false;

			string receivedText = this.stringBuilder.ToString();

			return this.waitForList.Any((waitFor) => receivedText.Contains(waitFor, this.ignoreCase));
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

				tokenSource.CancelAfter(this.ReceiveTimeout);
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

			await this.client.GetStream().WriteAsync(buffer, cancellationToken);
		}

		/// <summary>
		/// Reads from client stram roe recieved data and firing event TextReceived each time when the data is received.
		/// </summary>
		/// <param name="cancellationToken"> The cancelation token source</param>
		/// <returns>ValueTask result</returns>
		public async ValueTask ReadAsync(CancellationToken cancellationToken)
		{
			NetworkStream stream = this.client.GetStream();

			while (cancellationToken.IsCancellationRequested is false)
			{
				if (!stream.CanRead || !stream.DataAvailable)
					break;
					
				var buffer = new byte[this.ReceiveBufferSize];
				//Memory<byte> buffer = new byte[this.ReceiveBufferSize];

				var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
				// Array.Resize(ref data, await stream.ReadAsync(data, cancellationToken));
				//bytes = bytes.Concat(data);

				if (bytesRead > 0)
				{
					this.protocolHandler.InputFeed(buffer, bytesRead);

					int bytesOfText = this.protocolHandler.Negotiate(buffer);

					if (bytesOfText > 0)
					{
						string receivedText = this.Encoding.GetString(buffer, 0, bytesOfText);

						this.TextReceived?.Invoke(receivedText).ConfigureAwait(false);
					}
				}
				else
				{
					break; // The connection is about to be closed.
				}
			}
		}

		public async ValueTask<string> WaitFor(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
			NetworkStream stream = this.client.GetStream();
			StringBuilder stringBuilder = new StringBuilder();

			if (stream.CanRead)
			{
				//IEnumerable<byte> bytes = Array.Empty<byte>();
				string text = String.Empty;

				if (cancellationToken == default || cancellationToken == CancellationToken.None)
				{
					var tokenSource = new CancellationTokenSource();
					
					tokenSource.CancelAfter(this.ReceiveTimeout);
					cancellationToken = tokenSource.Token;
				}

				try
				{
					while (cancellationToken.IsCancellationRequested is false)
					{
						var buffer = new byte[this.ReceiveBufferSize];
						//Memory<byte> buffer = new byte[this.ReceiveBufferSize];
						var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

						// Array.Resize(ref data, await stream.ReadAsync(data, cancellationToken));
						//bytes = bytes.Concat(data);

						if (bytesRead > 0)
						{
							this.protocolHandler.InputFeed(buffer, bytesRead);

							int bytesOfText = this.protocolHandler.Negotiate(buffer);

							if (bytesOfText > 0)
							{
								string receivedText = this.Encoding.GetString(buffer, 0, bytesOfText);

								this.stringBuilder.Append(receivedText);
								this.TextReceived?.Invoke(receivedText).ConfigureAwait(false);
							}
						}
						else
						{
							// The connection is about to be closed.
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error when reading from TcpClient: " + ex.ToString());
				}
			}

			return stringBuilder.ToString();
		}

		public ValueTask CloseAsync(CloseReason? closeReason)
		{
			if (this.Connected)
			{
				this.client.Close();
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

		public void Dispose()
		{
			this.client.Dispose();
			this.client = null;
			this.stringBuilder = null;
			this.protocolHandler = null;
		}

		private void ProcessTelnetInput(int input, ref StringBuilder stringBuilder)
		{
			int startIndex = stringBuilder.Length;

			switch (input)
			{
				case -1: // End of stream
					break;

				case (int)TelnetCommand.IAC:

					// interpret as command
					int commandNumber = this.client.GetStream().ReadByte();

					if (commandNumber == -1) // End of stream
						break;

					TelnetCommand command = (TelnetCommand)commandNumber;

					switch (command)
					{
						case TelnetCommand.IAC:

							//literal IAC = 255 escaped, so append char 255 to string
							stringBuilder.Append(commandNumber);
							break;

						case TelnetCommand.DO:

							int optionNumber = this.client.GetStream().ReadByte();

							if (optionNumber == -1) // End of stream
								break;

							this.OnDoOption((TelnetOption)optionNumber);

							break;

						case TelnetCommand.DONT:

							optionNumber = this.client.GetStream().ReadByte();

							if (optionNumber == -1) // End of stream
								break;

							this.OnDontOption((TelnetOption)optionNumber);

							break;

						case TelnetCommand.WILL:

							optionNumber = this.client.GetStream().ReadByte();

							if (optionNumber == -1) // End of stream
								break;

							this.OnWillOption((TelnetOption)optionNumber);

							break;

						case TelnetCommand.WONT:

							optionNumber = this.client.GetStream().ReadByte();

							if (optionNumber == -1) // End of stream
								break;

							this.OnWontOption((TelnetOption)optionNumber);

							break;

						case TelnetCommand.AbortOutput:
						case TelnetCommand.AreYouThere:
						case TelnetCommand.BREAK:
						case TelnetCommand.DataMark:
						case TelnetCommand.EraseCharacter:
						case TelnetCommand.EraseLine:
						case TelnetCommand.GoAhead:
						case TelnetCommand.InterruptProcess:
						case TelnetCommand.NoOperation:

							this.OnCommandReceived(command);
							
							break;

						default:
							
							break;
					}

					break;

				default:

					stringBuilder.Append((byte)input); //this.Encoding.GetString(new byte[] { (byte)input }));
					
					break;
			}

			_ = (this.TextReceived?.Invoke(stringBuilder.ToString(startIndex, stringBuilder.Length - startIndex)));
		}

		protected virtual void OnCommandReceived(TelnetCommand command) { }

		protected virtual void OnDoOption(TelnetOption option)
		{
			NetworkStream networkStream = this.client.GetStream();

			networkStream.WriteByte((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				networkStream.WriteByte((byte)TelnetCommand.WILL);
			else
				networkStream.WriteByte((byte)TelnetCommand.WONT);

			networkStream.WriteByte((byte)option);
		}

		protected virtual void OnDontOption(TelnetOption option)
		{
			NetworkStream networkStream = this.client.GetStream();

			networkStream.WriteByte((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				networkStream.WriteByte((byte)TelnetCommand.DO);
			else
				networkStream.WriteByte((byte)TelnetCommand.DONT);

			networkStream.WriteByte((byte)option);
		}

		protected virtual void OnWillOption(TelnetOption option)
		{
			NetworkStream networkStream = this.client.GetStream();

			networkStream.WriteByte((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				networkStream.WriteByte((byte)TelnetCommand.DO);
			else
				networkStream.WriteByte((byte)TelnetCommand.DONT);

			networkStream.WriteByte((byte)option);
		}

		protected virtual void OnWontOption(TelnetOption option)
		{
			NetworkStream networkStream = this.client.GetStream();
			
			networkStream.WriteByte((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				networkStream.WriteByte((byte)TelnetCommand.DO);
			else
				networkStream.WriteByte((byte)TelnetCommand.DONT);

			networkStream.WriteByte((byte)option);
		}

		private void WriteByte(byte data) => this.client.GetStream().WriteByte(data);
	}
}
