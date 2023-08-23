using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class Provider : IProviderConnection, IDisposable
	{
		//public Provider()
		//{
		//}

		//public Provider(ProviderManager manager, ProviderType providerType, int deviceType)
		//{
		//	this.Manager = manager;
		//	this.ProviderType = providerType;
		//	this.DeviceType = deviceType;
		//}

		//public Provider(ProviderManager manager, ProviderGroup providerType, int deviceType)
		//{
		//	this.Manager = manager;
		//	this.ProviderType = providerType;
		//	this.DeviceType = deviceType;
		//}

		//public ProviderManager Manager { get; protected internal set; }
		public ProviderGroup ProviderModelType { get; protected internal set; }
		public int DeviceManagementType { get; protected internal set; }

		public object Owner { get; set; }
		//public abstract IRequestResult Connect();
		public abstract ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext);
		//public abstract bool IsConnected { get; }
		public abstract ValueTask CloseAsync();
		public abstract void SetLogging(string logFileName);
		public abstract ValueTask FinishUpdateAsync();

		public abstract void Dispose();
	}
}
