using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace Simple.Network
{
    public class SimpleClient<TPackage, TSendPackage> : SimpleClient<TPackage>, IEasyClient<TPackage, TSendPackage>
        where TPackage : class
    {
        private IPackageEncoder<TSendPackage> packageEncoder;

        protected SimpleClient(IPackageEncoder<TSendPackage> packageEncoder)
            : base()
        {
            this.packageEncoder = packageEncoder;
        }
        
        public SimpleClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ILogger logger = null)
            : this(pipelineFilter, packageEncoder, new ChannelOptions { Logger = logger })
        {
        }

        public SimpleClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            this.packageEncoder = packageEncoder;
        }

        public virtual async ValueTask SendAsync(TSendPackage package)
        {
            await this.SendAsync(this.packageEncoder, package);
        }

        public new IEasyClient<TPackage, TSendPackage> AsClient()
        {
            return this;
        }
    }

    public class SimpleClient<TReceivePackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        private IPipelineFilter<TReceivePackage> pipelineFilter;
        private IAsyncEnumerator<TReceivePackage> packageStream;

        protected IChannel<TReceivePackage> Channel { get; private set; }

        protected ILogger Logger { get; set; }

        protected ChannelOptions Options { get; private set; }


        public event PackageHandler<TReceivePackage> PackageHandler;

        public IPEndPoint LocalEndPoint { get; set; }

        public SecurityOptions Security { get; set; }

        protected SimpleClient()
        {
        }

        public SimpleClient(IPipelineFilter<TReceivePackage> pipelineFilter)
            : this(pipelineFilter, NullLogger.Instance)
        {
        }

        public SimpleClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger)
            : this(pipelineFilter, new ChannelOptions { Logger = logger })
        {
        }

        public SimpleClient(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions options)
        {
            if (pipelineFilter == null)
                throw new ArgumentNullException(nameof(pipelineFilter));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.pipelineFilter = pipelineFilter;
            this.Options = options;
            this.Logger = options.Logger;
        }

        public virtual IEasyClient<TReceivePackage> AsClient()
        {
            return this;
        }

        protected virtual IConnector GetConnector()
        {
            var security = Security;

            if (security != null)
                if (security.EnabledSslProtocols != SslProtocols.None)
                    return new SocketConnector(LocalEndPoint, new SslStreamConnector(security));

            return new SocketConnector(LocalEndPoint);
        }

        ValueTask<bool> IEasyClient<TReceivePackage>.ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return this.ConnectAsync(remoteEndPoint, cancellationToken);
        }

        protected virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            var connector = this.GetConnector();
            var state = await connector.ConnectAsync(remoteEndPoint, null, cancellationToken);

            if (state.Cancelled || cancellationToken.IsCancellationRequested)
            {
                this.OnError($"The connection to {remoteEndPoint} was cancelled.", state.Exception);
                
                return false;
            }                

            if (!state.Result)
            {
                this.OnError($"Failed to connect to {remoteEndPoint}", state.Exception);
                
                return false;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket is null.");

            var channelOptions = Options;
            
            this.SetupChannel(state.CreateChannel<TReceivePackage>(pipelineFilter, channelOptions));
            
            return true;
        }

        public void AsUdp(IPEndPoint remoteEndPoint, ArrayPool<byte> bufferPool = null, int bufferSize = 4096)
        { 
            var localEndPoint = this.LocalEndPoint;

            if (localEndPoint == null)
                localEndPoint = new IPEndPoint(remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            // bind the local endpoint
            socket.Bind(localEndPoint);

            var channel = new UdpPipeChannel<TReceivePackage>(socket, pipelineFilter, this.Options, remoteEndPoint);

            this.SetupChannel(channel);
            this.UdpReceive(socket, channel, bufferPool, bufferSize);
        }

        private async void UdpReceive(Socket socket, UdpPipeChannel<TReceivePackage> channel, ArrayPool<byte> bufferPool, int bufferSize)
        {
            if (bufferPool == null)
                bufferPool = ArrayPool<byte>.Shared;

            while (true)
            {
                var buffer = bufferPool.Rent(bufferSize);

                try
                {
                    var result = await socket.ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, channel.RemoteEndPoint)
                                             .ConfigureAwait(false);

                    await channel.WritePipeDataAsync((new ArraySegment<byte>(buffer, 0, result.ReceivedBytes)).AsMemory(), CancellationToken.None);
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    this.OnError($"Failed to receive UDP data.", e);
                }
                finally
                {
                    bufferPool.Return(buffer);
                }
            }
        }

        protected virtual void SetupChannel(IChannel<TReceivePackage> channel)
        {
            channel.Closed += OnChannelClosed;
            channel.Start();
            this.packageStream = channel.GetPackageStream();
            this.Channel = channel;
        }

        ValueTask<TReceivePackage> IEasyClient<TReceivePackage>.ReceiveAsync()
        {
            return ReceiveAsync();
        }

        /// <summary>
        /// Try to receive one package
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var p = await packageStream.ReceiveAsync();

            if (p != null)
                return p;

            this.OnClosed(Channel, EventArgs.Empty);
            
            return null;
        }

        void IEasyClient<TReceivePackage>.StartReceive()
        {
            this.StartReceive();
        }

        /// <summary>
        /// Start receive packages and handle the packages by event handler
        /// </summary>
        protected virtual void StartReceive()
        {
            this.StartReceiveAsync();
        }

        private async void StartReceiveAsync()
        {
            var enumerator = packageStream;

            while (await enumerator.MoveNextAsync())
                await this.OnPackageReceived(enumerator.Current);
        }

        protected virtual async ValueTask OnPackageReceived(TReceivePackage package)
        {
            var handler = PackageHandler;

            try
            {
                await handler.Invoke(this, package);
            }
            catch (Exception e)
            {
                this.OnError("Unhandled exception happened in PackageHandler.", e);
            }
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            this.Channel.Closed -= OnChannelClosed;
            this.OnClosed(this, e);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            var handler = this.Closed;

            if (handler != null)
                if (Interlocked.CompareExchange(ref this.Closed, null, handler) == handler)
                    handler.Invoke(sender, e);
        }

        protected virtual void OnError(string message, Exception exception)
        {
            this.Logger?.LogError(exception, message);
        }

        protected virtual void OnError(string message)
        {
            this.Logger?.LogError(message);
        }

        ValueTask IEasyClient<TReceivePackage>.SendAsync(ReadOnlyMemory<byte> data)
        {
            return this.SendAsync(data);
        }

        protected virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await this.Channel.SendAsync(data);
        }

        ValueTask IEasyClient<TReceivePackage>.SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            return this.SendAsync<TSendPackage>(packageEncoder, package);
        }

        protected virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            await this.Channel.SendAsync(packageEncoder, package);
        }

        public event EventHandler Closed;

        public virtual async ValueTask CloseAsync()
        {
            await this.Channel.CloseAsync(CloseReason.LocalClosing);
            this.OnClosed(this, EventArgs.Empty);
        }
    }
}
