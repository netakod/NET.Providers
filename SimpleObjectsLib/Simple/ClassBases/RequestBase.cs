using Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
	public abstract class RequestBase
	{
		public IRequestResult SendRequest(Action action)
		{
			return SendRequest<object>(() => 
				   { 
					   action(); 
					   
					   return null; 
				   });
		}

		public IRequestResult<TResult> SendRequest<TResult>(Func<TResult> func)
		{
			IRequestResult<TResult> result;

			try
			{
				TResult resultValue = func();
				result = new RequestResult<TResult>(resultValue, TaskResultInfo.Succeeded);
			}
			catch (TimeoutException ex)
			{
				result = new RequestResult<TResult>(default(TResult), TaskResultInfo.TimeOut, ex.GetFullErrorMessage());
			}
			catch (Exception ex)
			{
				return this.GetExceptionRequestResult<TResult>(ex);
			}

			return result;
		}

		protected virtual RequestResult<TResult> GetExceptionRequestResult<TResult>(Exception ex)
		{
			return new RequestResult<TResult>(default(TResult), TaskResultInfo.ExceptionIsCaught, ex.GetFullErrorMessage());
		}
	}
}