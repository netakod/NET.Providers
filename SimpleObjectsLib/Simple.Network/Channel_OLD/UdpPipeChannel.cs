using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public class UdpPipeChannel<TPackageInfo> : VirtualChannel<TPackageInfo>, IChannelWithSessionIdentifier
    {
        private Socket socket;
        private bool enableSendingPipe;

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint)
            : this(socket, pipelineFilter, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
        {
        }

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
            : base(pipelineFilter, options)
        {
            this.socket = socket;            
            this.enableSendingPipe = "true".Equals(options.Values?["enableSendingPipe"], StringComparison.OrdinalIgnoreCase);
            this.RemoteEndPoint = remoteEndPoint;
            this.SessionIdentifier = sessionIdentifier;
        }

        public string SessionIdentifier { get; }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            if (this.enableSendingPipe)
            {
                await base.SendAsync(buffer);

                return;
            }

            await this.SendOverIOAsync(new ReadOnlySequence<byte>(buffer), CancellationToken.None);
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            if (this.enableSendingPipe)
            {
                await base.SendAsync(packageEncoder, package);

                return;
            }

            try
            {
                await this.SendLock.WaitAsync();

                var writer = this.Out.Writer;

                this.WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync();
                await this.ProcessOutputRead(Out.Reader, null);
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write)
        {
            if (this.enableSendingPipe)
            {
                await base.SendAsync(write);

                return;
            }

            throw new NotSupportedException($"The method SendAsync(Action<PipeWriter> write) cannot be used when noSendingPipe is true.");
        }


        protected override void Close() => base.WriteEOFPackage();

        protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken) => throw new NotSupportedException();

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (this.enableSendingPipe || buffer.IsSingleSegment)
            {
                var total = 0;

                foreach (var piece in buffer)
                    total += await this.socket.SendToAsync(GetArrayByMemory<byte>(piece), SocketFlags.None, RemoteEndPoint);

                return total;
            }

            var pool = ArrayPool<byte>.Shared;
            var destBuffer = pool.Rent((int)buffer.Length);

            try
            {                
                this.MergeBuffer(ref buffer, destBuffer);
                
                return await this.socket.SendToAsync(new ArraySegment<byte>(destBuffer, 0, (int)buffer.Length), SocketFlags.None, RemoteEndPoint);       
            }
            finally
            {
                pool.Return(destBuffer);
            }            
        }

        protected override Task ProcessSends()
        {
            if (this.enableSendingPipe)
                return base.ProcessSends();

            return Task.CompletedTask;
        }


        private void MergeBuffer(ref ReadOnlySequence<byte> buffer, byte[] destBuffer)
        {
            Span<byte> destSpan = destBuffer;
            var total = 0;

            foreach (var piece in buffer)
            {
                piece.Span.CopyTo(destSpan);
                total += piece.Length;
                destSpan = destSpan.Slice(piece.Length);
            }
        }
    }
}