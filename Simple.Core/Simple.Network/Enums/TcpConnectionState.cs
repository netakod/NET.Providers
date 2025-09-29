using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Network
{
    public enum TcpConnectionState
    {
        Closed = 1,
        Listen = 2,
        SynSent = 3,
        SynReceived = 4,
        Established = 5,
        FinWait1 = 6,
        FinWait2 = 7,
        CloseWait = 8,
        LastAck = 9,
        Closing = 10,
        TimeWait = 11,
        DeleteTCB = 12
    }
}
