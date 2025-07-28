using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using System.Collections.Generic;

namespace Simple.Network
{
    public class TcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
    {
        private Socket socket;
        private List<ArraySegment<byte>> segmentsForSend;
        
        public TcpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            this.socket = socket;
            this.RemoteEndPoint = socket.RemoteEndPoint;
            this.LocalEndPoint = socket.LocalEndPoint;
        }

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
            return await socket.ReceiveAsync(GetArrayByMemory((ReadOnlyMemory<byte>)memory), socketFlags, cancellationToken)
                               .ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (buffer.IsSingleSegment)
            {
                return await this.socket.SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None, cancellationToken)
                                        .ConfigureAwait(false);
            }
            
            if (this.segmentsForSend == null)
            {
                this.segmentsForSend = new List<ArraySegment<byte>>();
            }
            else
            {
                this.segmentsForSend.Clear();
            }

            var segments = this.segmentsForSend;

            foreach (var piece in buffer)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.segmentsForSend.Add(GetArrayByMemory(piece));
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            return await this.socket.SendAsync(this.segmentsForSend, SocketFlags.None)
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
    }
}
