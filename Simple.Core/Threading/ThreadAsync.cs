using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Threading
{
	public class ThreadAsync<TKey> : ResetEventHash<TKey>, IDisposable
    {
        ResetEventHash<TKey> resetEvents = new ResetEventHash<TKey>();

        public ThreadAsync() { }

        public Task WaitForAsync(TKey token) => this.WaitForAsync(token, Timeout.Infinite, CancellationToken.None);

		public Task WaitForAsync(TKey token, int timeoutMilliseconds)
		{
			ManualResetEvent resetEvent = this.resetEvents.GetResetEvent(token);

			Task task = resetEvent.AsTask(timeoutMilliseconds);

			if (!this.resetEvents.IsReleased(token))
				this.resetEvents.DisposeResetEvent(token);

			return task;
		}

		public Task WaitForAsync(TKey token, int timeoutMilliseconds, CancellationToken cancellationToken)
		{
            ManualResetEvent resetEvent = this.resetEvents.GetResetEvent(token);

            Task task = resetEvent.WaitOneAsync(timeoutMilliseconds, cancellationToken);

            if (!this.resetEvents.IsReleased(token))
                this.resetEvents.DisposeResetEvent(token);

            return task;
        }

        /// <summary>
        /// Join should be called after Prepare, so ManualResetEvent should be in hashtable. If Release is already called, join immediately.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="millisecondsTimeout"></param>
        public virtual Task Join(TKey token, int millisecondsTimeout)
        {
            if (this.ContainsToken(token))
                return this.WaitForAsync(token, millisecondsTimeout);

            return Task.CompletedTask;
        }
    }
}
