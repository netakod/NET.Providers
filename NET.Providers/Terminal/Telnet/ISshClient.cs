using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NET.Providers.Telnet;

namespace NET.Providers.Terminal
{
    public interface ISshClient : ITelnetClient
    {
        string Username { get; set; }
        string Password { get; set; }

    }
}
