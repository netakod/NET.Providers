using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Threading
{
    public class ThreadAsyncWithDataExchange<TKey, TValue> : ThreadAsync<TKey>
    {
        object lockObject = new object();
        public Dictionary<object, TValue> returnValues = new Dictionary<object, TValue>();

        public new TValue WaitForAsync(TKey token) => this.WaitForAsync(token, Int32.MaxValue);

        /// <summary>
        /// Blocks the current thread until the current System.Threading.WaitHandle receives
        /// a signal, using a 32-bit signed integer to measure the time interval.
        /// </summary>
        /// <param name="token">The wait token.</param>
        /// <param name="timeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>The return value if the current instance receives a signal.</returns>
        /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">millisecondsTimeout is a negative number other than -1, which represents an infinite time-out.</exception>
        /// <exception cref="AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
        /// <exception cref="InvalidOperationException">The current instance is a transparent proxy for a System.Threading.WaitHandle in another application domain.</exception>
        /// <exception cref="TimeoutException">The exception that is thrown when the time allotted for a process or operation has expired.</exception>
        public new TValue WaitForAsync(TKey token, int timeout)
        {
            TValue value = default(TValue);
            
            base.WaitForAsync(token, timeout);

            lock (this.lockObject)
            {
                if (this.returnValues.ContainsKey(token))
                {
                    value = this.returnValues[token];
                    //this.returnValues.Remove(token);
                }
            }

            return value;
        }

        public bool Release(TKey token, TValue returnValue)
        {
            lock (this.lockObject)
            {
                if (this.returnValues.ContainsKey(token))
                {
                    this.returnValues[token] = returnValue;
                }
                else
                {
                    this.returnValues.Add(token, returnValue);
                }
            }

            bool isReleased = base.Release(token);

			if (!isReleased)
			{
				lock (this.lockObject)
				{
					this.returnValues.Remove(token);
				}
			}

			return isReleased;
        }

		public override void DisposeResetEvent(TKey token)
		{
			base.DisposeResetEvent(token);

            lock (this.lockObject)
			{
                this.returnValues.Remove(token);
            }
        }
    }
}
