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
	[ProviderType(ProviderGroup.GsmProvider)]
	public class GsmProvider : Provider, IProviderConnection, IDisposable
	{
		public override ValueTask CloseAsync() => new ValueTask();

		public override ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
		{
			return new ValueTask<TaskInfo<string>>(new TaskInfo<string>(resultValue: "GSM Provider connection test succeeded successufuly.", TaskResultInfo.Succeeded, message: String.Empty));
		}

		public override void SetLogging(string logFileName)
		{
		}

		public override async ValueTask FinishUpdateAsync()
		{
			await Task.Delay(0);
		}

		public override void Dispose()
		{
		}
	}
}
