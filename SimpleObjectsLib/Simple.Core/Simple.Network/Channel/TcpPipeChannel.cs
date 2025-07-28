using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System.Collections.Generic;

namespace Simple.Network
{
	public abstract class TcpPipeChannel : PipeChannel
    {
        private Socket socket;
        private List<ArraySegment<byte>> segmentsForSend;
        
        public override EndPoint RemoteEndPoint => this.socket?.RemoteEndPoint;
        public override EndPoint LocalEndPoint => this.socket?.LocalEndPoint;

		public override bool Connected => this.socket?.Connected ?? false;

		public virtual async ValueTask ConnectAsync(string remoteHost, int remotePort, CancellationToken cancellationToken = default)
        {
            await this.ConnectAsync(DnsHelper.ResolveIPAddressFromHostname(remoteHost), remotePort, cancellationToken);
        }

        public virtual async ValueTask ConnectAsync(IPAddress remoteIpAddress, int remotePort, CancellationToken cancellationToken = default)
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
                await this.CloseAsync(CloseReason.LocalClosing); 
            
            //if (this.socket != null) 
            //    this.socket.DisconnectAsync(new SocketAsyncEventArgs());

            this.socket = socket;
            this.SetSocketOptions();

            if (cancellationToken == CancellationToken.None)
            {
                var tokenSource = new CancellationTokenSource();

                tokenSource.CancelAfter(this.Options.ConnectTimeout);
                cancellationToken = tokenSource.Token;
            }

#if NETSTANDARD
            await this.socket.ConnectAsync(remoteEndPoint);
#else 
            await this.socket.ConnectAsync(remoteEndPoint, cancellationToken);
#endif
            if (this.Connected)
            {
                this.StartChannel();
                this.OnConnect();
            }
        }

        protected virtual void OnConnect() { }


        protected override void OnClosed()
        {
            this.socket = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await ReceiveAsync(this.socket, memory, SocketFlags.None, cancellationToken);
        }

        private async ValueTask<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return await this.socket.ReceiveAsync(memory.ToArraySegment(), socketFlags, cancellationToken)
                                    .ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (buffer.IsSingleSegment)
                return await this.socket.SendAsync(buffer.First.ToArraySegment(), SocketFlags.None, cancellationToken)
                                        .ConfigureAwait(false);
            
            if (this.segmentsForSend == null)
                this.segmentsForSend = new List<ArraySegment<byte>>();
            else
                this.segmentsForSend.Clear();

            var segments = this.segmentsForSend;

            foreach (var piece in buffer)
            {
                cancellationToken.ThrowIfCancellationRequested();
                segments.Add(piece.ToArraySegment());
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            return await this.socket.SendAsync(segments, SocketFlags.None)
                                    .ConfigureAwait(false);
        }

		protected override void Close()
        {
            var socket = this.socket;

            if (socket == null)
                return;

            if (Interlocked.CompareExchange(ref this.socket, null, socket) == socket)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    socket.Close();
                }
            }
        }

        protected override bool IsIgnorableException(Exception ex)
        {
            if (base.IsIgnorableException(ex))
                return true;

            if (ex is SocketException sex)
                if (sex.IsIgnorableSocketException())
                    return true;

            return false;
        }

        private void SetSocketOptions()
        {
            this.socket.NoDelay = base.Options.NoDelay;

            if (this.Options.ReceiveBufferSize > 0)
                socket.ReceiveBufferSize = this.Options.ReceiveBufferSize;

            if (this.Options.SendBufferSize > 0)
                socket.SendBufferSize = this.Options.SendBufferSize;

            if (this.Options.ReceiveTimeout > 0)
                socket.ReceiveTimeout = this.Options.ReceiveTimeout;

            if (this.Options.SendTimeout > 0)
                socket.SendTimeout = this.Options.SendTimeout;
        }
    }
}
