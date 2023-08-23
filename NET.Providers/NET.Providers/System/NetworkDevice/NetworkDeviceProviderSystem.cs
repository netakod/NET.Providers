using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Providers
{
	public abstract class NetworkDeviceProviderSystem : NetworkDeviceProviderModule, INetworkDeviceProviderSystem
	{
		public abstract ValueTask<string> GetName();
		public abstract ValueTask SetName(string name);
		public abstract ValueTask SetCommunity(string community);
		public abstract ValueTask<IEnumerable<ApplyPasswordDestination>> SetPassword(string password);
		public abstract ValueTask<string> GetDescription();
		public abstract ValueTask<string> GetObjectID();
		public abstract ValueTask<string> GetLocation();
		public abstract ValueTask SetLocation(string location);
		public abstract ValueTask<string> GetContact();
		public abstract ValueTask SetContact(string contact);
		public abstract ValueTask<TimeSpan> GetUpTime();
	}
}
