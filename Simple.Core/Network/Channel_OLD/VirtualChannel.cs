using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public abstract class VirtualChannel<TPackageInfo> : PipeChannel<TPackageInfo>, IVirtualChannel
    {
        public VirtualChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
        }

        protected override Task FillPipeAsync(PipeWriter writer) => Task.CompletedTask;

        public async ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await this.In.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}