using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Providers.Snmp;
using NET.Providers.Terminal;
using NET.Providers.Web;

namespace NET.Providers
{
	public class GsmClientProvider : ClientProvider, IProviderConnection, IDisposable
	{
		public GsmClientProvider(GsmProvider provider)
		{
			this.Provider = provider;
		}

		private GsmProvider Provider { get; set; }

		public override async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
		{
			return await this.Provider.TestConnectionAsync(workerContext);
		}

		public override async ValueTask CloseAsync() => await this.Provider.CloseAsync();

		public override async ValueTask FinishUpdateAsync() => await this.Provider.FinishUpdateAsync();

		public override void SetLogging(string logFileName) => this.Provider.SetLogging(logFileName);

		public override void Dispose() =>  this.Provider.Dispose();
	}
}
