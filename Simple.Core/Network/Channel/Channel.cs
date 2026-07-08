using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public abstract class Channel
    {
        public ChannelOptions Options { get; set; }

        protected abstract void StartChannel();
        
        //public abstract IAsyncEnumerable<TPackageInfo> RunAsync();

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

        //public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        
        public abstract ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken);

        public abstract bool Connected { get; }

        public abstract EndPoint RemoteEndPoint { get; }

        public abstract EndPoint LocalEndPoint { get; }

        public CloseReason CloseReason { get; protected set; }

        public DateTimeOffset LastActiveTime { get; protected set; } = DateTimeOffset.Now;

        protected virtual void OnClosed()
        {
            var closed = this.Closed;

            if (closed == null)
                return;

            if (Interlocked.CompareExchange(ref this.Closed, null, closed) != closed)
                return;

            var closeReason = this.CloseReason; //.HasValue ? this.CloseReason.Value : Telnet.CloseReason.Unknown;

            closed.Invoke(closeReason);
        }

        public event CloseEventHandlerAsync Closed;

        public abstract ValueTask CloseAsync(CloseReason closeReason);

        public abstract ValueTask DetachAsync();
    }

    public delegate ValueTask CloseEventHandlerAsync(CloseReason closeReason);
}
