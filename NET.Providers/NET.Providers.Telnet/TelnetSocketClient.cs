using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using Simple.Network;

namespace NET.Tools.Telnet
{

	/// <summary>
	/// The telnet client based on socket
	/// The Receive/Send implementation is based on async TcpClient example: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
	/// </summary>
	public class TelnetSocketClient : TelnetClientBase
	{
		private byte[] receiveBuffer = null;
		private Socket socket = null;

		public EndPoint LocalEndPoint => this.socket?.LocalEndPoint;

		public override bool Connected => this.socket?.Connected ?? false;

		public async ValueTask ConnectAsync(Socket socket, IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
			this.socket = socket;

#if NET5_0_OR_GREATER
			await socket.ConnectAsync(remoteEndPoint, cancellationToken);
#else
                Task result = socket.ConnectAsync(remoteEndPoint);               
                int index = Task.WaitAny(new[] { result }, cancellationToken);
                var connected = socket.Connected;
                
				if (!connected)
                    socket.Close();

				await Task.Delay(1);
#endif
		}

		protected override async ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
		{
			await this.ConnectAsync(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), remoteEndPoint, cancellationToken);
		}

		protected override void OnConnect()
		{
			base.OnConnect();

			this.receiveBuffer = new byte[this.ReceiveBufferSize];
			this.socket.BeginReceive(this.receiveBuffer, 0, this.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.ReadCallback), state: null);
		}

		protected override async ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			await this.socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
		}

		protected override ValueTask ClientCloseAsync(CloseReason closeReason)
		{
			this.socket.Close();

#if NETSTANDARD
			return new ValueTask();
#else
			return ValueTask.CompletedTask;
#endif
		}

		private void ReadCallback(IAsyncResult result)
		{
			try
			{
				int bytesRead = this.socket.EndReceive(result); // Read data from the client socket.

				if (bytesRead > 0)
				{
					var receivedData = new ReadOnlySequence<byte>(this.receiveBuffer, 0, bytesRead);
					var receivedSequence = new SequenceReader<byte>(receivedData);

					this.OnDataReceived(ref receivedSequence);

					this.socket.BeginReceive(this.receiveBuffer, 0, this.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.ReadCallback), state: null); // Get the rest (next) data.
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
	}
}
