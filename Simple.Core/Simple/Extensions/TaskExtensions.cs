using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class TaskExtensions
	{
		public static void DoNotAwait(this Task task) { }

		public static void DoNotAwait<T>(this Task<T> task) { }

		public static void DoNotAwait(this ValueTask task) { }

		public static void DoNotAwait<T>(this ValueTask<T> task) { }

		public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			if (task == await Task.WhenAny(task, Task.Delay(timeout)))
				return await task;

			throw new System.TimeoutException();
		}

		/// <summary>
		/// If you prefer to not throw an exception (as I do) it's even simpler, just return the default value:
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="task"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static Task<TResult> WithTimeoutDefault<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			var timeoutTask = Task.Delay(timeout).ContinueWith(_ => default(TResult), TaskContinuationOptions.ExecuteSynchronously);
			
			return Task.WhenAny(task, timeoutTask).Unwrap();
		}
	}
}
