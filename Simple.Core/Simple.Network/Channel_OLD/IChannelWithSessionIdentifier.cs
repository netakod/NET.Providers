using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public interface IChannelWithSessionIdentifier
    {
        string SessionIdentifier { get; }
    }
}
