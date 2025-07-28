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
    public abstract partial class PipeChannel : Channel
    {
        private Task readsTask;
        private Task sendsTask;
        private bool isDetaching = false;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);
        protected Pipe Out { get; private set; }
        protected Pipe In { get; private set; }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.SendLock.WaitAsync(cancellationToken);

                var writer = this.Out.Writer;

                this.WriteBuffer(writer, buffer);
                await writer.FlushAsync(cancellationToken);
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.SendLock.WaitAsync(cancellationToken);
                var writer = this.Out.Writer;
                write(writer);
                await writer.FlushAsync(cancellationToken);
            }
            finally
            {
                this.SendLock.Release();
            }
        }

        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            this.CloseReason = closeReason;
            this.cancellationTokenSource.Cancel();
            await this.HandleClosing();
        }

        protected override void StartChannel()
        {
            this.Options ??= ChannelOptions.Default;
            this.Out = this.Options.Out ?? new Pipe();
            this.In = this.Options.In ?? new Pipe();

            this.readsTask = this.ProcessReads();
            this.sendsTask = this.ProcessSends();
            this.WaitHandleClosing();
        }

        protected virtual async Task ProcessReads()
        {
            var pipe = this.In;

            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
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

        protected abstract void Close();

        protected virtual async Task FillPipeAsync(PipeWriter writer)
        {
            var options = this.Options;
            var cts = this.cancellationTokenSource;

            while (!cts.IsCancellationRequested)
            {
                try
                {                    
                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);
                    var bytesRead = await this.FillPipeWithDataAsync(memory, cts.Token);         

                    if (bytesRead == 0)
                    {
                        if (this.CloseReason == CloseReason.Unknown)
                            this.CloseReason = CloseReason.RemoteClosing;
                        
                        break;
                    }

                    this.LastActiveTime = DateTimeOffset.Now;
                    
                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    if (!this.IsIgnorableException(e))
                    {
                        this.OnError("Exception happened in ReceiveAsync", e);

                        if (this.CloseReason == CloseReason.Unknown)
                            this.CloseReason = cts.IsCancellationRequested ? CloseReason.LocalClosing
                                                                           : CloseReason.SocketError; 
                    }
                    else if (this.CloseReason == CloseReason.Unknown)
                    {
                        this.CloseReason = CloseReason.RemoteClosing;
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

        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);

        protected async ValueTask<bool> ProcessOutputRead(PipeReader reader, CancellationTokenSource cancellationTokenSource)
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
                    cancellationTokenSource?.Cancel(false);
                    
                    if (!this.IsIgnorableException(ex))
                        this.OnError("Exception happened in SendAsync", ex);
                    
                    return true;
                }
            }

            reader.AdvanceTo(end);
            
            return completed;
        }

        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        protected internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
                throw new InvalidOperationException("Buffer backed by array was expected");

            return result;
        }

        protected async Task ReadPipeAsync(PipeReader reader)
        {
            var cts = this.cancellationTokenSource;

            while (!cts.IsCancellationRequested)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cts.Token);
                }
                catch (Exception ex)
                {
                    if (!this.IsIgnorableException(ex))
                        this.OnError("Failed to read from the pipe", ex);
                    
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
                    this.CloseReason = CloseReason.ProtocolError; // close the connection if get a protocol error
                    this.TryClose();
                    
                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
            this.WriteEOF();
        }

        protected abstract void WriteEOF();

        protected abstract void OnDataReceive(ref SequenceReader<byte> sequenceReader);
    
        public override async ValueTask DetachAsync()
        {
            this.isDetaching = true;
            this.cancellationTokenSource.Cancel();
            await HandleClosing();
            this.isDetaching = false;
        }

        protected void OnError(string message, Exception ex = null)
        {
            if (ex != null)
                this.Options.Logger?.LogError(ex, message);
            else
                this.Options.Logger?.LogError(message);
        }

        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;
            var maxPackageLength = this.Options.MaxPackageLength;
            var sequenceReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                this.OnDataReceive(ref sequenceReader); // Consume the received data

                var bytesConsumed = sequenceReader.Consumed;

                bytesConsumedTotal += bytesConsumed;

                var len = bytesConsumed;

                // nothing has been consumed, need more data
                if (len == 0)
                    len = sequenceReader.Length;

                if (maxPackageLength > 0 && len > maxPackageLength)
                {
                    this.OnError($"Package cannot be larger than {maxPackageLength}.");
                    this.CloseReason = CloseReason.ProtocolError;
                    this.TryClose(); // close the the connection directly

                    return false;
                }

                consumed = buffer.GetPosition(bytesConsumedTotal);

                if (sequenceReader.End) // no more data
                {
                    examined = consumed = buffer.End;

                    return true;
                }

                if (bytesConsumed > 0)
                    sequenceReader = new SequenceReader<byte>(sequenceReader.Sequence.Slice(bytesConsumed));
            }
        }

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            this.CheckIsChannelOpen();
            writer.Write(buffer.Span);
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
                if (!this.isDetaching && this.Connected)
                    this.TryClose();
            }
        }

        private void TryClose()
		{
            try
            {
                this.Close();
            }
            catch (Exception exc)
            {
                if (!this.IsIgnorableException(exc))
                    this.OnError("Unhandled exception in the method PipeChannel.Close.", exc);
            }
            finally
			{
                this.OnClosed();
            }
        }

        private void CheckIsChannelOpen()
        {
            if (!this.Connected)
                throw new Exception("Channel is closed now, send is not allowed.");
        }

    }
}
