using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;


namespace Simple.Network
{
	public interface IProviderConnection
	{
		object Owner { get; set; }
		//IRequestResult Connect();
		ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext);
		//bool IsConnected { get; }
		//void Disconnect();
		void SetLogging(string logFileName);
		ValueTask FinishUpdateAsync();
	}
}