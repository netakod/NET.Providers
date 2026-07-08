//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Net.Sockets;
//using System.Threading;
//using NET.Tools.Telnet;

//namespace NET.Tools.Terminal
//{
//	public class SimpleTelnet : ITelnetControl, IDisposable
//	{
//		TcpClient client = null;
//		private byte[] receiveBuffer = null;
//		private int position, length = 0;
//		StringBuilder stringBuilder = new StringBuilder(1024);

//		public SimpleTelnet()
//		{
//		}

//		//public event TextEventHandler TextReceived;
//		public event TextEventHandler TextReceived;
//		public event EventHandler Disconnected;

//		public bool AcceptData { get; set; }
		
//		///// <summary>
//		///// SleepInterval in milliseconds.
//		///// </summary>
//		//public int ReadInterval { get; private set; }

//		/// <summary>
//		/// Receive timeout in miliseconds
//		/// </summary>
//		public int ReceiveTimeout { get; set; }

//		/// <summary>
//		/// Send timeout in miliseconds
//		/// </summary>
//		public int SendTimeout { get; set; }
		
//		public string RemoteHost { get; set; }
//		public int RemotePort { get; set; }

//		public int ReceiveBufferSize { get; set; } = 1024;

//		public bool Echo { get; private set; }

//		/// <summary>
//		/// IAC - init sequence for telnet negotiation.
//		/// </summary>
//		public const byte IAC = 255;

//		/// <summary>
//		/// [IAC] DONT
//		/// </summary>
//		public const byte DONT = 254;

//		/// <summary>
//		/// [IAC] DO
//		/// </summary>
//		public const byte DO = 253;

//		/// <summary>
//		/// [IAC] WONT
//		/// </summary>
//		public const byte WONT = 252;

//		/// <summary>
//		/// [IAC] WILL
//		/// </summary>
//		public const byte WILL = 251;

//		/// <summary>
//		/// [IAC] Sub Begin
//		/// </summary>
//		public const byte SB = 250;

//		/// <summary>
//		/// [IAC] Go Ahead (GA)
//		/// </summary>
//		public const byte GoAhead = 249;

//		/// <summary>
//		/// [IAC] Erase Line (EL)
//		/// </summary>
//		/// <seealso cref="EraseCharacter"/>
//		public const byte EL = 248;

//		/// <summary>
//		/// [IAC] Erase Character (EC)
//		/// </summary>
//		/// <seealso cref="EraseLine"/>
//		public const byte EC = 247;

//		/// <summary>
//		/// [IAC] Are You There (AYT)
//		/// </summary>
//		public const byte AYT = 246;

//		/// <summary>
//		/// [IAC] Abort output (AO)
//		/// </summary>
//		public const byte AO = 245;

//		/// <summary>
//		/// [IAC] Interrupt Process (IP)
//		/// </summary>
//		public const byte IP = 244;

//		/// <summary>
//		/// [IAC] Break (BRK)
//		/// </summary>
//		public const byte BRK = 243;

//		/// <summary>
//		/// [IAC] Data Mark
//		/// </summary>
//		public const byte DM = 242;

//		/// <summary>
//		/// [IAC] No operation (NOP)
//		/// </summary>
//		public const byte NOP = 241;

//		/// <summary>
//		/// [IAC] Sub End
//		/// </summary>
//		public const byte SE = 240;

//		/// <summary>
//		/// [IAC] End Of Record
//		/// </summary>
//		public const byte EOR = 239;



//		public bool Connected
//		{
//			get { return this.client != null && this.client.Connected; }
//		}

//		public void Connect()
//		{
//			this.client = new TcpClient(this.RemoteHost, this.RemotePort);
//			this.client.ReceiveTimeout = this.ReceiveTimeout;
//			this.client.SendTimeout = this.SendTimeout;

//			this.receiveBuffer = new byte[this.ReceiveBufferSize];
//			this.client.GetStream().BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null);
//		}
//		public void Disconnect()
//		{
//			if (this.Connected)
//			{
//				this.client.Close();
//				this.Disconnected?.Invoke(this, EventArgs.Empty);
//			}

//			this.client = null;
//		}

//		public void WriteLine(string command)
//		{
//			this.Write(command + "\n");
//		}

//		public void Write(string command)
//		{
//			//if (!this.client.Connected)
//			//	return;

//			byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));

//			try
//			{
//				this.client.GetStream().Write(buffer, 0, buffer.Length);
//			}
//			catch
//			{
//				this.Disconnect();
//			}
//		}

//		public string WaitFor(params string[] waitForText)
//		{
//			return this.WaitFor(ignoreCase: true, waitForText);
//		}

//		public string WaitFor(bool ignoreCase, params string[] waitForText)
//		{
//			throw new NotImplementedException();
			
//			// TODO:
			
//			//if (!client.Connected)
//			//	return null;

//			//StringBuilder stringBuilder = new StringBuilder();

//			//while (true)
//			//{
//			//	if (this.client.Available > 0)
//			//	{
//			//		int input = this.client.GetStream().ReadByte();
//			//		this.ProcessTelnetInput(input, ref stringBuilder);

//			//		string text = stringBuilder.ToString();

//			//		if (text.ContainsAny(waitForText, ignoreCase))
//			//			return text;
//			//	}

//			//	//Thread.Sleep(this.ReadInterval);
//			//}
//		}

//		//public string Read()
//		//{
//		//	if (!client.Connected)
//		//		return null;

//		//	StringBuilder stringBuilder = new StringBuilder();

//		//	while (this.client.Available > 0)
//		//	{
//		//		int input = this.client.GetStream().ReadByte();

//		//		this.ProcessTelnetInput(input, ref stringBuilder);
//		//		//Thread.Sleep(this.ReadInterval);
//		//	}

//		//	return stringBuilder.ToString();
//		//}

//		public void Dispose() => this.Disconnect();

//		protected virtual void OnCommandReceived(TelnetCommand command, ref List<byte> response) 
//		{ 
//		}

//		protected virtual void OnDoOption(TelnetOption option, ref List<byte> response)
//		{
//			response.Add((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				response.Add((byte)TelnetCommand.WILL);
//			else
//				response.Add((byte)TelnetCommand.WONT);

//			response.Add((byte)option);
//		}

//		protected virtual void OnDontOption(TelnetOption option, ref List<byte> response)
//		{
//			response.Add((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				response.Add((byte)TelnetCommand.DO);
//			else
//				response.Add((byte)TelnetCommand.DONT);

//			response.Add((byte)option);
//		}

//		protected virtual void OnWillOption(TelnetOption option, ref List<byte> response)
//		{
//			response.Add((byte)TelnetCommand.IAC);

//			switch (option)
//			{
//				case TelnetOption.SuppressGoAhead: 
//				case TelnetOption.Authentication:  response.Add((byte)TelnetCommand.DO); break;
//				case TelnetOption.Echo:			   response.Add((byte)TelnetCommand.DO); this.Echo = true; break;
//				default:						   response.Add((byte)TelnetCommand.DONT); break;
//			}

//			response.Add((byte)option);
//		}

//		protected virtual void OnWontOption(TelnetOption option, ref List<byte> response)
//		{
//			response.Add((byte)TelnetCommand.IAC);

//			switch (option)
//			{
//				case TelnetOption.SuppressGoAhead:
//				case TelnetOption.Authentication: response.Add((byte)TelnetCommand.DO); break;
//				case TelnetOption.Echo:			  response.Add((byte)TelnetCommand.DO); this.Echo = false; break;
//				default:						  response.Add((byte)TelnetCommand.DONT); break;
//			}

//			response.Add((byte)option);
//		}

//		private void ProcessInputBuffer(int count)
//		{
//			List<byte> response = new List<byte>();
			
//			this.position = 0;
//			this.length = count;
//			this.stringBuilder.Clear();
			
//			while (this.position < this.length)
//			{
//				int input = this.GetNext();

//				switch (input)
//				{
//					case (int)TelnetCommand.IAC:

//					// interpret as command
//					int commandNumber = this.GetNext();

//					TelnetCommand command = (TelnetCommand)commandNumber;

//					switch (command)
//					{
//						case TelnetCommand.IAC: //literal IAC = 255 escaped, so append char 255 to string

//							this.stringBuilder.Append(commandNumber);

//							break;

//						case TelnetCommand.DO:

//							int optionNumber = this.GetNext();

//							this.OnDoOption((TelnetOption)optionNumber, ref response);

//							break;

//						case TelnetCommand.DONT:

//							optionNumber = this.GetNext();

//							this.OnDontOption((TelnetOption)optionNumber, ref response);

//							break;

//						case TelnetCommand.WILL:

//							optionNumber = this.GetNext();

//							this.OnWillOption((TelnetOption)optionNumber, ref response);

//							break;

//						case TelnetCommand.WONT:

//							optionNumber = this.GetNext();

//							this.OnWontOption((TelnetOption)optionNumber, ref response);

//							break;

//						case TelnetCommand.AbortOutput:
//						case TelnetCommand.AreYouThere:
//						case TelnetCommand.BREAK:
//						case TelnetCommand.DataMark:
//						case TelnetCommand.EraseCharacter:
//						case TelnetCommand.EraseLine:
//						case TelnetCommand.GoAhead:
//						case TelnetCommand.InterruptProcess:
//						case TelnetCommand.NoOperation:

//							this.OnCommandReceived(command, ref response);
//							break;

//						default:
//							break;
//					}

//					break;

//				default:

//					this.stringBuilder.Append((char)input);

//					break;
//				}
//			}

//			if (response.Count > 0)
//				this.Send(response.ToArray());

//			//return this.stringBuilder.ToString();
//		}

//		private byte GetNext()
//		{
//			while (true)
//			{
//				if (this.position < this.length)
//					return this.receiveBuffer[this.position++];

//				Thread.Sleep(150); // Wait for new data to arrive
//			}
//		}

//		private void Send(byte data) => this.client.GetStream().WriteByte(data);

//		private void Send(byte[] buffer) => this.client.GetStream().WriteAsync(buffer);

//		private void ReadCallback(IAsyncResult result)
//		{
//			try
//			{
//				NetworkStream stream = this.client.GetStream();

//				int bytesRead = stream.EndRead(result); // Read data from the client socket.

//				if (bytesRead > 0)
//				{
//					this.ProcessInputBuffer(bytesRead);

//					string receivedText = this.stringBuilder.ToString();

//					if (!String.IsNullOrEmpty(receivedText))
//						this.TextReceived(receivedText);

//					stream.BeginRead(this.receiveBuffer, 0, this.ReceiveBufferSize, new AsyncCallback(this.ReadCallback), state: null); // Get the rest of the data (next data).
//				}
//				else
//				{
//					// The connection is about to be closed.
//				}
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine("Exception error when reading from socket stream: " + ex.ToString());
//			}
//		}

//		void ITelnetControl.SendAsync(string command) => this.Write(command);
//	}
//}