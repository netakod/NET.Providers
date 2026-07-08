using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Network
{
    public delegate ValueTask PackageHandler<TReceivePackage>(SimpleClient<TReceivePackage> sender, TReceivePackage package)
        where TReceivePackage : class;
}