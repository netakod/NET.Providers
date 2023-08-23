using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Providers
{
	[NetworkDeviceModuleType(NetworkDeviceModule.System)]
	public interface INetworkDeviceProviderSystem : IDisposable
	{
		ValueTask<string> GetName();
		ValueTask SetName(string name);
		ValueTask SetCommunity(string community);
		ValueTask<IEnumerable<ApplyPasswordDestination>> SetPassword(string password);
		ValueTask<string> GetDescription();
		ValueTask<string> GetObjectID();
		ValueTask<string> GetLocation();
		ValueTask SetLocation(string location);
		ValueTask<string> GetContact();
		ValueTask SetContact(string contact);
		ValueTask<TimeSpan> GetUpTime();
	}
}
