//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace NET.Tools.Terminal
//{
//	public class TelnetControlSimpleTelnet : ITelnetControl, IDisposable
//	{
//		private SimpleTelnet telnet = null;
//		private string receivedText = String.Empty;


//		public event TextEventHandler TextReceived;

//		/// <summary>
//		/// Timeout in milliseconds.
//		/// </summary>
//		public int Timeout { get; set; }

//		public string RemoteHost { get; set; }
//		public int RemotePort { get; set; }

//		public bool AcceptData { get; set; }

//		public bool Connected
//		{
//			get { return (this.telnet != null && this.telnet.Connected); }
//		}

//		public void Connect()
//		{
//			this.telnet = new SimpleTelnet(this.RemoteHost, this.RemotePort);
//			this.telnet.TextReceived += this.Telnet_TextReceived;
//		}

//		public void Disconnect()
//		{
//			this.telnet.Disconnect();
//		}

//		public void SendAsync(string command)
//		{
//			if (this.Connected)
//				this.telnet.Write(command);
//		}

//		public void Dispose()
//		{
//			if (this.Connected)
//				this.Disconnect();

//			this.telnet = null;
//		}

//		private void Telnet_TextReceived(string text)
//		{
//			if (this.AcceptData)
//			{
//				this.TextReceived?.Invoke(this.receivedText + text);
//				this.receivedText = String.Empty;
//			}
//			else
//			{
//				this.receivedText += text;
//			}
//		}
//	}
//}
