using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Simple.Network
{
    public static class SocketExtensions
    {
        public static bool IsIgnorableSocketException(this SocketException sex)
        {
            switch (sex.SocketErrorCode)
            {
                case (SocketError.OperationAborted):
                case (SocketError.ConnectionReset):
                case (SocketError.TimedOut):
                case (SocketError.NetworkReset):
                    return true;
                
                default:
                    return false;
            }
        }
    }
}
