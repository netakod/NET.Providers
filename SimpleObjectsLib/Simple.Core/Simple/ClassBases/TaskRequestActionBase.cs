using Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public abstract class TaskRequestActionBase
	{
		public TaskInfo SendRequest(Action action)
		{
			TaskInfo result;

			try
			{
				action();
				result = TaskInfo.CompletedSuccessful;
			}
			catch (TimeoutException ex)
			{
				result = new TaskInfo(TaskResultInfo.TimeOut, ex.GetFullErrorMessage());
			}
			catch (Exception ex)
			{
				result = this.GetExceptionRequestResult(ex);
			}

			return result;
		}

		public async ValueTask<TaskInfo> SendRequestAsync(AsyncAction action)
		{
			TaskInfo result;

			try
			{
				await action();
				result = TaskInfo.CompletedSuccessful;
			}
			catch (TimeoutException ex)
			{
				result = new TaskInfo(TaskResultInfo.TimeOut, ex.GetFullErrorMessage());
			}
			catch (Exception ex)
			{
				result = this.GetExceptionRequestResult(ex);
			}

			return result;
		}

		protected virtual TaskInfo GetExceptionRequestResult(Exception ex) => new TaskInfo(TaskResultInfo.ExceptionIsCaught, ex.GetFullErrorMessage());

		public TaskInfo<TResult> SendRequest<TResult>(Func<TResult> func)
		{
			TaskInfo<TResult> result;
			TResult resultValue = default(TResult);

			try
			{
				resultValue = func();
				result = new TaskInfo<TResult>(resultValue, TaskResultInfo.Succeeded);
			}
			catch (TimeoutException ex)
			{
				result = new TaskInfo<TResult>(resultValue, TaskResultInfo.TimeOut, ex.GetFullErrorMessage());
			}
			catch (Exception ex)
			{
				return this.GetExceptionRequestResult<TResult>(resultValue, ex);
			}

			return result;
		}

		public async ValueTask<TaskInfo<TResult>> SendRequestAsync<TResult>(AsyncFunc<TResult> func)
		{
			TaskInfo<TResult> result;
			TResult resultValue = default(TResult);

			try
			{
				resultValue = await func();
				result = new TaskInfo<TResult>(resultValue, TaskResultInfo.Succeeded);
			}
			catch (TimeoutException ex)
			{
				result = new TaskInfo<TResult>(resultValue, TaskResultInfo.TimeOut, ex.GetFullErrorMessage());
			}
			catch (Exception ex)
			{
				return this.GetExceptionRequestResult<TResult>(resultValue, ex);
			}

			return result;
		}


		protected virtual TaskInfo<TResult> GetExceptionRequestResult<TResult>(TResult result, Exception ex)
		{
			return new TaskInfo<TResult>(result, TaskResultInfo.ExceptionIsCaught, ex.GetFullErrorMessage());
		}
	}
}