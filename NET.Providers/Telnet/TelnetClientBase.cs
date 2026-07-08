using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Buffers;
using Simple;
using Simple.Network;

namespace NET.Tools.Telnet
{
	/// <summary>
	/// The Telnet client abstract class.
	/// </summary>
	public abstract class TelnetClientBase
	{
		//private Socket socket = null;
		//private NetworkStream stream = null;
		//private TelnetProtocol protocolHandler = null;

		private string[] waitForList = null;
		private bool ignoreCase = false;
		private StringBuilder stringBuilder = new StringBuilder(512);

		// TODO: If we can improve ManualResetEvent with Monitor or somthing else, see: https://blog.teamleadnet.com/2012/02/why-autoresetevent-is-slow-and-how-to.html
		private ManualResetEvent waitForFoundResetEvent = new ManualResetEvent(false);

		#region |   Constructors and Initialization   |

		public TelnetClientBase()
		{
			//this.protocolHandler = new TelnetProtocol(telnetClient: this);
		}

		#endregion |   Constructors and Initialization   |

		public event TextEventHandlerAsync TextReceived;
		public event CloseEventHandlerAsync Closed;

		public bool Echo { get; private set; }

		public int ReceiveBufferSize { get; set; } = 1024;
		
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

		public CloseReason CloseReason { get; protected set; }

		public EndPoint RemoteEndPoint { get; private set; }

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
				await this.CloseAsync(CloseReason.LocalClosing);

			this.RemoteEndPoint = remoteEndPoint;

			await this.ClientConnectAsync(remoteEndPoint, cancellationToken);
			this.OnConnect();

			List<byte> command = new List<byte>();

			command.Add((byte)TelnetCommand.IAC);
			command.Add((this.Echo) ? (byte)TelnetCommand.DO : (byte)TelnetCommand.DONT);
			command.Add((byte)TelnetOption.Echo);

			await this.SendAsync(command.ToArray()); // .ConfigureAwait(false); //this.SendAsync(response.ToArray()).GetAwaiter().GetResult();
		}

		protected abstract ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken);

		protected virtual void OnConnect()
		{
			this.stringBuilder.Clear();
			this.waitForList = null;
			//this.protocolHandler.Reset();
			this.waitForFoundResetEvent.Close();
		}

		protected void OnDataReceived(ref SequenceReader<byte> reader)
		{
			string receivedText = this.ProcessReceivedData(ref reader);

			this.OnTextReceived(receivedText);

			//reader.Advance(buffer.LongLength); // We consume all given data
		}

		protected void OnTextReceived(string text)
		{
			if (text.Length > 0)
			{
				this.TextReceived?.Invoke(text).ConfigureAwait(false);

				//System.Diagnostics.Debug.WriteLine("TELNET RECEIVED: " + text);

				if (this.IsWaitForReceived())
				{
					this.waitForFoundResetEvent.Set(); // Unblock curent WaitFor
					this.waitForFoundResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another WaitFor calls
				}
			}
		}

		public async ValueTask SendLineAsync(string text, CancellationToken cancellationToken = default) //, params string[] waitFor)
		{
			await this.SendAsync(text + "\r\n", cancellationToken);
		}

		public async ValueTask SendAsync(string text, CancellationToken cancellationToken = default) //, params string[] waitFor)
		{
			var dataToSend = this.PrepareTextForSending(text);

			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();

				tokenSource.CancelAfter(this.SendTimeout);
				cancellationToken = tokenSource.Token;
			}

			await this.SendAsync(dataToSend, cancellationToken);

			//if (waitFor.Length > 0)
			//	return await this.WaitFor(ignoreCase, cancellationToken, waitFor);
			//else
			//	return default(string); // It was nothing to wait for, thus return result is null
		}
		
		public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			await this.ClientSendAsync(buffer, cancellationToken);
		}

		protected virtual ReadOnlyMemory<byte> PrepareTextForSending(string text)
		{
			return this.Encoding.GetBytes(text.Replace("\0xFF", "\0xFF\0xFF")); // Doubled IAC

			// TODO: Check if we need to parse ASCII 10 or 13 and need to add additional pairs "\r\n"
			// If the Terminal sends only \n or \r for linefeed+carriage and the server require \r\n for new line we need to insert complete \r\n
		}

		//protected virtual ReadOnlyMemory<byte> TransposeDataForSending(byte[] data)
		//{
			
		//	if (data.Contains((byte)0xFF)) // IAC must be doubled, if exists
		//	{
		//		byte[] newData = new byte[data.Length];
		//		int newCount = 0;

		//		for (int i = 0; i < data.Length; i++)
		//		{
		//			newData[newCount++] = data[i];

		//			if (data[i] == 0xFF)
		//				newData[newCount++] = 0xFF;
		//		}

		//		return new ReadOnlyMemory<byte>(newData, 0, newCount);
		//	}
		//	else
		//	{
		//		return new ReadOnlyMemory<byte>(data);
		//	}
		//}

		protected abstract ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

		// TODO: Check if this working with WaitOneAsync extension. There is some problems in PackageEngine with this async WaitOne !!!!!
		public async ValueTask<string> WaitForAsync(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
			//if (!this.Connected)
			//	throw new Exception("The client is not connected.");

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
			}                                   // TODO: Check if this working with WaitOneAsync extension. There is some problems in PackageEngine with this async WaitOne !!!!!
			else if (await this.waitForFoundResetEvent.WaitOneAsync(cancellationToken)) // WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, this.waitForFoundResetEvent });
			//else if (this.waitForFoundResetEvent.WaitOne(cancellationToken)) // WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, this.waitForFoundResetEvent });
			{
				receivedText = this.stringBuilder.ToString();
			}

			this.waitForList = null;
			this.stringBuilder.Clear();

			return receivedText;
		}

		public ValueTask CloseAsync(CloseReason closeReason)
		{
			this.CloseReason = closeReason;
			
			try
			{
				this.ClientCloseAsync(closeReason);
			}
			finally
			{
				this.waitForFoundResetEvent.Close();
				this.OnClosed();
			}

#if NETSTANDARD
			return new ValueTask();
#else
			return ValueTask.CompletedTask;
#endif
		}

		protected abstract ValueTask ClientCloseAsync(CloseReason closeReason);

		public void Dispose()
		{
			if (this.Connected)
				this.DisposeAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync()
		{
			if (this.Connected)
				await this.CloseAsync(CloseReason.LocalClosing);
		}

		protected virtual void OnClosed()
		{
			this.RemoteEndPoint = null;
			this.RaiseClosed();
		}

		protected void RaiseClosed()
		{
			var closed = this.Closed;

			if (closed == null)
				return;

			if (Interlocked.CompareExchange(ref this.Closed, null, closed) != closed)
				return;

			var closeReason = this.CloseReason; //.HasValue ? this.CloseReason.Value : Telnet.CloseReason.Unknown;

			closed.Invoke(closeReason);
		}

		protected bool IsWaitForReceived()
		{
			if (this.waitForList == null)
				return false;

			string receivedText = this.stringBuilder.ToString();

			return this.waitForList.Any((waitFor) => receivedText.Contains(waitFor, this.ignoreCase));
		}

		#region |   Telent Commands Handling   |

		protected virtual void OnCommandReceived(TelnetCommand command, ref List<byte> response)
		{
		}

		protected virtual void OnDoOption(TelnetOption option, ref List<byte> response)
		{
			response.Add((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				response.Add((byte)TelnetCommand.WILL);
			else
				response.Add((byte)TelnetCommand.WONT);

			response.Add((byte)option);
		}

		protected virtual void OnDontOption(TelnetOption option, ref List<byte> response)
		{
			response.Add((byte)TelnetCommand.IAC);

			if (option == TelnetOption.SuppressGoAhead)
				response.Add((byte)TelnetCommand.DO);
			else
				response.Add((byte)TelnetCommand.DONT);

			response.Add((byte)option);
		}

		protected virtual void OnWillOption(TelnetOption option, ref List<byte> response)
		{
			response.Add((byte)TelnetCommand.IAC);

			switch (option)
			{
				case TelnetOption.SuppressGoAhead:
				case TelnetOption.Authentication: response.Add((byte)TelnetCommand.DO);   break;
				case TelnetOption.Echo:			  response.Add((byte)TelnetCommand.DO);   this.Echo = true; break;
				default:						  response.Add((byte)TelnetCommand.DONT); break;
			}

			response.Add((byte)option);
		}

		protected virtual void OnWontOption(TelnetOption option, ref List<byte> response)
		{
			response.Add((byte)TelnetCommand.IAC);

			switch (option)
			{
				case TelnetOption.SuppressGoAhead:
				case TelnetOption.Authentication: response.Add((byte)TelnetCommand.DO);   break;
				case TelnetOption.Echo:			  response.Add((byte)TelnetCommand.DO);   this.Echo = false; break;
				default:						  response.Add((byte)TelnetCommand.DONT); break;
			}

			response.Add((byte)option);
		}

		protected string ProcessReceivedData(ref SequenceReader<byte> reader)
		{
			List<byte> response = new List<byte>();
			
			this.stringBuilder.Clear();

			while (!reader.End)
			{
				int input = reader.GetNext();

				switch (input)
				{
					case (int)TelnetCommand.IAC:

						// interpret as command
						int commandNumber = reader.GetNext();

						TelnetCommand command = (TelnetCommand)commandNumber;

						switch (command)
						{
							case TelnetCommand.IAC: //literal IAC = 255 escaped, so append char 255 to string

								this.stringBuilder.Append(commandNumber);

								break;

							case TelnetCommand.DO:

								int optionNumber = reader.GetNext();

								this.OnDoOption((TelnetOption)optionNumber, ref response);

								break;

							case TelnetCommand.DONT:

								optionNumber = reader.GetNext();

								this.OnDontOption((TelnetOption)optionNumber, ref response);

								break;

							case TelnetCommand.WILL:

								optionNumber = reader.GetNext();

								this.OnWillOption((TelnetOption)optionNumber, ref response);

								break;

							case TelnetCommand.WONT:

								optionNumber = reader.GetNext();

								this.OnWontOption((TelnetOption)optionNumber, ref response);

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

								this.OnCommandReceived(command, ref response);
								
								break;

							default:
								
								break;
						}

						break;

					default:

						this.stringBuilder.Append((char)input);

						break;
				}
			}

			if (response.Count > 0)
				this.SendAsync(response.ToArray()).ConfigureAwait(false); //this.SendAsync(response.ToArray()).GetAwaiter().GetResult();

			return this.stringBuilder.ToString();
		}

		#endregion |   Telent Commands Handling   |
	}

	public delegate ValueTask TextEventHandlerAsync(string text);
}
