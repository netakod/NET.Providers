using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Telnet
{
	public class TelnetPipeClient : TelnetClientBase
	{
		public TelnetPipeClient() : this(ChannelOptions.Default) { }

		public TelnetPipeClient(ChannelOptions options) => this.Channel = this.CreateChannel(options);

		protected TelnetTcpPipeChannel Channel { get; private set; }

		public override bool Connected => this.Channel?.Connected ?? false;
		
		protected virtual TelnetTcpPipeChannel CreateChannel(ChannelOptions options) => new TelnetTcpPipeChannel(this.OnDataReceived, this.OnChannelClosed) { Options = options };

		protected override async ValueTask ClientConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
		{
			await this.Channel.ConnectAsync(remoteEndPoint, cancellationToken);
		}

		protected override async ValueTask ClientCloseAsync(CloseReason closeReason)
		{
			await this.Channel.CloseAsync(closeReason);
		}

		protected override async ValueTask ClientSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			await this.Channel.SendAsync(buffer, cancellationToken);
		}

		protected void OnChannelClosed(CloseReason closeReason)
		{
			if (closeReason != CloseReason.Unknown)
				this.CloseReason = closeReason;
			
			this.RaiseClosed();
		}
	}
}
