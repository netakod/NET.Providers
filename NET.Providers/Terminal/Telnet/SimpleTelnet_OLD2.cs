//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Net.Sockets;
//using System.Threading;
//using NET.Tools.Telnet;

//namespace NET.Tools.Terminal
//{
//	public class SimpleTelnet : IDisposable
//	{
//		TcpClient tcpSocket = null;

//		public SimpleTelnet(string remoteHost, int remotePort, int readingInterval = 100)
//		{
//			this.RemoteHost = remoteHost;
//			this.RemotePort = remotePort;
//			this.ReadInterval = readingInterval;

//			this.tcpSocket = new TcpClient(this.RemoteHost, this.RemotePort);
//		}

//		public event TextEventHandler TextReceived;

//		/// <summary>
//		/// SleepInterval in milliseconds.
//		/// </summary>
//		public int ReadInterval { get; private set; }
//		public string RemoteHost { get; private set; }
//		public int RemotePort { get; private set; }

//		public bool Connected
//		{
//			get { return this.tcpSocket != null && this.tcpSocket.Connected; }
//		}

//		public void Disconnect()
//		{
//			if (this.Connected)
//				this.tcpSocket.Close();

//			this.tcpSocket = null;
//		}

//		public void WriteLine(string command)
//		{
//			this.Write(command + "\n");
//		}

//		public void Write(string command)
//		{
//			if (!this.tcpSocket.Connected)
//				return;

//			byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));

//			this.tcpSocket.GetStream().Write(buffer, 0, buffer.Length);
//		}

//		public string WaitFor(params string[] waitForText)
//		{
//			return this.WaitFor(ignoreCase: true, waitForText);
//		}

//		public string WaitFor(bool ignoreCase, params string[] waitForText)
//		{
//			if (!tcpSocket.Connected)
//				return null;

//			StringBuilder stringBuilder = new StringBuilder();

//			while (true)
//			{
//				if (this.tcpSocket.Available > 0)
//				{
//					int input = this.tcpSocket.GetStream().ReadByte();
//					this.ProcessTelnetInput(input, ref stringBuilder);

//					string text = stringBuilder.ToString();

//					if (text.ContainsAny(waitForText, ignoreCase))
//						return text;
//				}

//				Thread.Sleep(this.ReadInterval);
//			}
//		}

//		public string Read()
//		{
//			if (!tcpSocket.Connected)
//				return null;

//			StringBuilder stringBuilder = new StringBuilder();

//			while (this.tcpSocket.Available > 0)
//			{
//				int input = this.tcpSocket.GetStream().ReadByte();

//				this.ProcessTelnetInput(input, ref stringBuilder);
//				Thread.Sleep(this.ReadInterval);
//			}

//			return stringBuilder.ToString();
//		}

//		public void Dispose() => this.Disconnect();

//		protected virtual void OnCommandReceived(TelnetCommand command) { }

//		protected virtual void OnDoOption(TelnetOption option)
//		{
//			this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.WILL);
//			else
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.WONT);

//			this.tcpSocket.GetStream().WriteByte((byte)option);
//		}

//		protected virtual void OnDontOption(TelnetOption option)
//		{
//			this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DO);
//			else
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DONT);

//			this.tcpSocket.GetStream().WriteByte((byte)option);
//		}

//		protected virtual void OnWillOption(TelnetOption option)
//		{
//			this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DO);
//			else
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DONT);

//			this.tcpSocket.GetStream().WriteByte((byte)option);
//		}

//		protected virtual void OnWontOption(TelnetOption option)
//		{
//			this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.IAC);

//			if (option == TelnetOption.SuppressGoAhead)
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DO);
//			else
//				this.tcpSocket.GetStream().WriteByte((byte)TelnetCommand.DONT);

//			this.tcpSocket.GetStream().WriteByte((byte)option);
//		}

//		private void ProcessTelnetInput(int input, ref StringBuilder stringBuilder)
//		{
//			int startIndex = stringBuilder.Length;

//			switch (input)
//			{
//				case -1: // End of stream
//					break;

//				case (int)TelnetCommand.IAC:

//					// interpret as command
//					int commandNumber = this.tcpSocket.GetStream().ReadByte();

//					if (commandNumber == -1) // End of stream
//						break;

//					TelnetCommand command = (TelnetCommand)commandNumber;

//					switch (command)
//					{
//						case TelnetCommand.IAC:

//							//literal IAC = 255 escaped, so append char 255 to string
//							stringBuilder.Append(commandNumber);
//							break;

//						case TelnetCommand.DO:

//							int optionNumber = this.tcpSocket.GetStream().ReadByte();

//							if (optionNumber == -1) // End of stream
//								break;

//							this.OnDoOption((TelnetOption)optionNumber);

//							break;

//						case TelnetCommand.DONT:

//							optionNumber = this.tcpSocket.GetStream().ReadByte();

//							if (optionNumber == -1) // End of stream
//								break;

//							this.OnDontOption((TelnetOption)optionNumber);

//							break;

//						case TelnetCommand.WILL:

//							optionNumber = this.tcpSocket.GetStream().ReadByte();

//							if (optionNumber == -1) // End of stream
//								break;

//							this.OnWillOption((TelnetOption)optionNumber);

//							break;

//						case TelnetCommand.WONT:

//							optionNumber = this.tcpSocket.GetStream().ReadByte();

//							if (optionNumber == -1) // End of stream
//								break;

//							this.OnWontOption((TelnetOption)optionNumber);

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

//							this.OnCommandReceived(command);
//							break;

//						default:
//							break;
//					}

//					break;

//				default:

//					stringBuilder.Append((char)input);
//					break;
//			}

//			this.TextReceived?.Invoke(stringBuilder.ToString(startIndex, stringBuilder.Length - startIndex));
//		}

//		private void WriteByte(byte data)
//		{
//			this.tcpSocket.GetStream().WriteByte(data);
//		}
//	}

//	//public delegate void TextEventHandler(string text);
//}