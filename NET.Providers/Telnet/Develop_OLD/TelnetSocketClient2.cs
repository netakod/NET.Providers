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
	public class TelnetSocketClient2 : TelnetClientBase, ITelnetClient
	{
		private Socket client = null;

		public override EndPoint RemoteEndPoint => this.client?.RemoteEndPoint;
		public EndPoint LocalEndPoint => this.client?.LocalEndPoint;

		public override bool Connected => this.client?.Connected ?? false;

		public async ValueTask ConnectAsync(Socket socket, IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
		{
			this.client = socket;

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

			//#if NETSTANDARD
			//			Task result = this.client.ConnectAsync(remoteEndPoint);
			//			int index = Task.WaitAny(new[] { result }, cancellationToken);
			//			var connected = this.client.Connected;

			//			if (!connected)
			//				this.client.Close();
			//#else
			//			await this.client.ConnectAsync(remoteEndPoint, cancellationToken);
			//#endif
		}

		protected override async ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken) 
		{
			await this.ConnectAsync(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), remoteEndPoint, cancellationToken);
		}

		protected override IAsyncResult ClientBeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return this.client.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
		}

		protected override int ClientEndReceive(IAsyncResult result) => this.client.EndReceive(result);

		protected override async ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			await this.client.SendAsync(buffer, SocketFlags.None, cancellationToken);
		}

		protected override ValueTask ClientCloseAsync(CloseReason? closeReason)
		{
			this.client.Close();

#if NETSTANDARD
			return new ValueTask();
#else
			return ValueTask.CompletedTask;
#endif
		}
	}
}
