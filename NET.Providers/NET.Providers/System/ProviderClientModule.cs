using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;
using Simple.Network;

namespace NET.Tools.Providers
{
	public abstract class ClientProviderModule : TaskRequestActionBase
	{
		protected override TaskInfo GetExceptionRequestResult(Exception ex)
		{
			if (ex is ProviderInfoException)
				return new TaskInfo(TaskResultInfo.Succeeded, ex.GetFullErrorMessage());

			return base.GetExceptionRequestResult(ex);
		}

		protected override TaskInfo<TResult> GetExceptionRequestResult<TResult>(TResult result, Exception ex)
		{
			if (ex is ProviderInfoException)
				return new TaskInfo<TResult>(result, TaskResultInfo.Succeeded, ex.GetFullErrorMessage());

			return base.GetExceptionRequestResult<TResult>(result, ex);
		}
	}
}
