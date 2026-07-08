using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Simple.Network
{
    public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
    {
        private Stream stream;

        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : this(stream, remoteEndPoint, null, pipelineFilter, options)
        {
        }

        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            this.stream = stream;
            this.RemoteEndPoint = remoteEndPoint;
            this.LocalEndPoint = localEndPoint;
        }

        protected override void Close() => this.stream.Close();

        protected override void OnClosed()
        {
            this.stream = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await this.stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var total = 0;

            foreach (var data in buffer)
            {
                await this.stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                total += data.Length;
            }

            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            
            return total;
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