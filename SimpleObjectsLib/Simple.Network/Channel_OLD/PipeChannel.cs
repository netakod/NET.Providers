using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Simple.Network
{
    public abstract class PipeChannel<TPackageInfo> : Channel<TPackageInfo>, IChannel<TPackageInfo>, IChannel, IPipeChannel
    {
        private IPipelineFilter<TPackageInfo> pipelineFilter;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private IObjectPipe<TPackageInfo> packagePipe;
        private Task readsTask;
        private Task sendsTask;
        private bool isDetaching = false;

        protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);
        protected Pipe In { get; }
        protected Pipe Out { get; }
        protected ILogger Logger { get; }
        protected ChannelOptions Options { get; }

        protected abstract void Close();
        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);
        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);

        public override void Start()
        {
            this.readsTask = this.ProcessReads();
            this.sendsTask = this.ProcessSends();
            
            this.WaitHandleClosing();
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        {
            if (this.readsTask == null || this.sendsTask == null)
                throw new Exception("The channel has not been started yet.");

            while (true)
            {
                var package = await this.packagePipe.ReadAsync().ConfigureAwait(false);

                if (package == null)
                {
                    await this.HandleClosing();
                    
                    yield break;
                }

                yield return package;
            }
        }

        public override async ValueTask DetachAsync()
        {
            this.isDetaching = true;
            this.cancellationTokenSource.Cancel();
            await this.HandleClosing();
            this.isDetaching = false;
        }

        protected void OnError(string message, Exception ex = null)
        {
            if (ex != null)
                this.Logger?.LogError(ex, message);
            else
                this.Logger?.LogError(message);
        }

        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            this.cancellationTokenSource.Cancel();
            await HandleClosing();
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            try
            {
                await this.SendLock.WaitAsync();

                var writer = this.Out.Writer;

                this.WriteBuffer(writer, buffer);
                await writer.FlushAsync();
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            try
            {
                await this.SendLock.WaitAsync();

                var writer = this.Out.Writer;

                this.WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync();
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write)
        {
            try
            {
                await this.SendLock.WaitAsync();

                var writer = this.Out.Writer;

                write(writer);
                await writer.FlushAsync();
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
        {
            this.pipelineFilter = pipelineFilter;

            if (!options.ReadAsDemand)
                this.packagePipe = new DefaultObjectPipe<TPackageInfo>();
            else
                this.packagePipe = new DefaultObjectPipeWithSupplyControl<TPackageInfo>();

            this.Options = options;
            this.Logger = options.Logger;
            this.In = options.In ?? new Pipe();
            this.Out = options.Out ?? new Pipe();
        }

        protected virtual async Task FillPipeAsync(PipeWriter writer)
        {
            var options = this.Options;
            var cts = this.cancellationTokenSource;
            var supplyController = this.packagePipe as ISupplyController;

            if (supplyController != null)
            {
                cts.Token.Register(() =>
                {
                    supplyController.SupplyEnd();
                });
            }

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (supplyController != null)
                    {
                        await supplyController.SupplyRequired();

                        if (cts.IsCancellationRequested)
                            break;
                    }

                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);
                    var bytesRead = await this.FillPipeWithDataAsync(memory, cts.Token);

                    if (bytesRead == 0)
                    {
                        if (!this.CloseReason.HasValue)
                            this.CloseReason = Network.CloseReason.RemoteClosing;

                        break;
                    }

                    this.LastActiveTime = DateTimeOffset.Now;

                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    if (!this.IsIgnorableException(ex))
                    {
                        this.OnError("Exception happened in ReceiveAsync", ex);

                        if (!CloseReason.HasValue)
                        {
                            this.CloseReason = cts.IsCancellationRequested ? Network.CloseReason.LocalClosing : 
                                                                             Network.CloseReason.SocketError;
                        }
                    }
                    else if (!this.CloseReason.HasValue)
                    {
                        this.CloseReason = Network.CloseReason.RemoteClosing;
                    }

                    break;
                }

                // Make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                    break;
            }

            // Signal to the reader that we're done writing
            await writer.CompleteAsync().ConfigureAwait(false);
            // And don't allow writing data to outgoing pipeline
            await this.Out.Writer.CompleteAsync().ConfigureAwait(false);
        }

        protected virtual bool IsIgnorableException(Exception ex)
        {
            if (ex is ObjectDisposedException || ex is NullReferenceException || ex is OperationCanceledException)
                return true;

            if (ex.InnerException != null)
                return this.IsIgnorableException(ex.InnerException);

            return false;
        }

        protected virtual async Task ProcessReads()
        {
            var pipe = this.In;

            Task writing = this.FillPipeAsync(pipe.Writer);
            Task reading = this.ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
        }

        protected async ValueTask<bool> ProcessOutputRead(PipeReader reader, CancellationTokenSource cts)
        {
            var result = await reader.ReadAsync(CancellationToken.None);
            var completed = result.IsCompleted;
            var buffer = result.Buffer;
            var end = buffer.End;

            if (!buffer.IsEmpty)
            {
                try
                {
                    await this.SendOverIOAsync(buffer, CancellationToken.None);
                    this.LastActiveTime = DateTimeOffset.Now;
                }
                catch (Exception ex)
                {
                    cts?.Cancel(false);

                    if (!this.IsIgnorableException(ex))
                        this.OnError("Exception happened in SendAsync", ex);

                    return true;
                }
            }

            reader.AdvanceTo(end);
            
            return completed;
        }

        protected virtual async Task ProcessSends()
        {
            var output = this.Out.Reader;
            var cts = this.cancellationTokenSource;

            while (true)
            {
                var completed = await this.ProcessOutputRead(output, cts);

                if (completed)
                    break;
            }

            output.Complete();
        }




        protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            this.CheckChannelOpen();
            packageEncoder.Encode(writer, package);
        }

        protected internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
                throw new InvalidOperationException("Buffer backed by array was expected");

            return result;
        }

        protected async Task ReadPipeAsync(PipeReader reader)
        {
            var cts = cancellationTokenSource;

            while (!cts.IsCancellationRequested)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cts.Token);
                }
                catch (Exception ex)
                {
                    if (!IsIgnorableException(ex))
                        OnError("Failed to read from the pipe", ex);

                    break;
                }

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

                try
                {
                    if (buffer.Length > 0)
                    {
                        if (!this.ReaderBuffer(ref buffer, out consumed, out examined))
                        {
                            completed = true;
                            
                            break;
                        }
                    }

                    if (completed)
                        break;
                }
                catch (Exception ex)
                {
                    this.OnError("Protocol error", ex);
                    this.Close(); // close the connection if get a protocol error

                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
            this.WriteEOFPackage();
        }

        protected void WriteEOFPackage() => this.packagePipe.Write(default);

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            this.CheckChannelOpen();
            writer.Write(buffer.Span);
        }

        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;
            var maxPackageLength = this.Options.MaxPackageLength;
            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                var currentPipelineFilter = this.pipelineFilter;
                var filterSwitched = false;
                var packageInfo = currentPipelineFilter.Filter(ref seqReader);
                var nextFilter = currentPipelineFilter.NextFilter;

                if (nextFilter != null)
                {
                    nextFilter.Context = currentPipelineFilter.Context; // pass through the context
                    this.pipelineFilter = nextFilter;
                    filterSwitched = true;
                }

                var bytesConsumed = seqReader.Consumed;
                
                bytesConsumedTotal += bytesConsumed;

                var len = bytesConsumed;

                if (len == 0) // nothing has been consumed, need more data
                    len = seqReader.Length;

                if (maxPackageLength > 0 && len > maxPackageLength)
                {
                    this.OnError($"Package cannot be larger than {maxPackageLength}.");
                    this.Close(); // close the the connection directly
                    
                    return false;
                }

                if (packageInfo == null)
                {
                    if (!filterSwitched) // the current pipeline filter needs more data to process
                    {
                        consumed = buffer.GetPosition(bytesConsumedTotal); // set consumed position and then continue to receive...
                        
                        return true;
                    }

                    currentPipelineFilter.Reset(); // we should reset the previous pipeline filter after switch
                }
                else
                {
                    currentPipelineFilter.Reset(); // reset the pipeline filter after we parse one full package
                    this.packagePipe.Write(packageInfo);
                }

                if (seqReader.End) // no more data
                {
                    examined = consumed = buffer.End;
                    
                    return true;
                }

                if (bytesConsumed > 0)
                    seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
            }
        }

        private void CheckChannelOpen()
        {
            if (this.IsClosed)
                throw new Exception("Channel is closed now, send is not allowed.");
        }

        private async void WaitHandleClosing() => await HandleClosing();

        private async ValueTask HandleClosing()
        {
            try
            {
                await Task.WhenAll(this.readsTask, this.sendsTask);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                this.OnError("Unhandled exception in the method PipeChannel.Run.", ex);
            }
            finally
            {
                if (!this.isDetaching && !this.IsClosed)
                {
                    try
                    {
                        this.Close();
                        this.OnClosed();
                    }
                    catch (Exception ex)
                    {
                        if (!this.IsIgnorableException(ex))
                            this.OnError("Unhandled exception in the method PipeChannel.Close.", ex);
                    }
                }
            }
        }

        Pipe IPipeChannel.Out => this.Out;
        Pipe IPipeChannel.In => this.In;
        IPipelineFilter IPipeChannel.PipelineFilter => this.pipelineFilter;
    }
}
