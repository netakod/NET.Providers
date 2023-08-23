using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Simple.Network;
using NET.Tools.Telnet;

namespace NET.Tools.Terminal
{
    public interface ITelnetClient
    {
        bool Connected { get; }

        event Telnet.TextEventHandlerAsync TextReceived;
        event CloseEventHandlerAsync Closed;

        //ValueTask ConnectAsync(string remoteHost, int remotePort = 23, CancellationToken cancellationToken = default);
        //ValueTask ConnectAsync(IPAddress remoteIpAddress, int remotePort = 23, CancellationToken cancellationToken = default);
        ValueTask ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default);

        ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
        //public ValueTask<string> SendLineAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor);
        //ValueTask<string> SendAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor);
        ValueTask SendAsync(string text, CancellationToken cancellationToken = default);

        ValueTask<string> WaitForAsync(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText);

        ValueTask CloseAsync(CloseReason closeReason);

        ValueTask DisposeAsync();
    }
}
