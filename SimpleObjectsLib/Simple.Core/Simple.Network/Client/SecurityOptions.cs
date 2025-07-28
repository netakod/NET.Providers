using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public class SecurityOptions : SslClientAuthenticationOptions
    {
        public NetworkCredential Credential { get; set; }
    }
}