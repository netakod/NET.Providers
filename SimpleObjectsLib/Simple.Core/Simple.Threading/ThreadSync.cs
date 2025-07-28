using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.ComponentModel;

namespace Simple.Threading
{
    public class ThreadSync<TKey> : ResetEventHash<TKey>, IDisposable
    {
        private bool exceptionOnTimeout = true;

        public ThreadSync() { }

        public bool ExceptionOnTimeout { get => this.exceptionOnTimeout; set => this.exceptionOnTimeout = value; }

        public virtual void WaitFor(TKey token) => this.WaitFor(token, Int32.MaxValue);

		/// <summary>
		/// Blocks the current thread until the current System.Threading.WaitHandle receives a signal, using a 32-bit signed integer to specify the time interval in milliseconds.
		/// </summary>
		/// <param name="token">The token to waite to be released.</param>
		/// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
		public virtual void WaitFor(TKey token, int millisecondsTimeout)
        {
            ManualResetEvent resetEvent = this.GetResetEvent(token);
            
            resetEvent.WaitOne(millisecondsTimeout);

            if (!this.IsReleased(token))
            {
                this.DisposeResetEvent(token);

                if (this.exceptionOnTimeout)
                    throw new TimeoutException(String.Format("{0} timeout.", token));
            }
        }

        public static T Invoke<T>(Func<CancelEventArgs, T> function, TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException("timeout");

            CancelEventArgs args = new CancelEventArgs(false);
            IAsyncResult functionResult = function.BeginInvoke(args, null, null);
            WaitHandle waitHandle = functionResult.AsyncWaitHandle;

            if (!waitHandle.WaitOne(timeout))
            {
                args.Cancel = true; // flag to worker that it should cancel!
                /* •————————————————————————————————————————————————————————————————————————•
                   | IMPORTANT: Always call EndInvoke to complete your asynchronous call.   |
                   | http://msdn.microsoft.com/en-us/library/2e08f6yc(VS.80).aspx           |
                   | (even though we arn't interested in the result)                        |
                   •————————————————————————————————————————————————————————————————————————• */
                ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, (state, timedOut) => function.EndInvoke(functionResult), null, -1, true);
                
                throw new TimeoutException("Timeout");
            }
            else
            {
                return function.EndInvoke(functionResult);
            }
        }

        public static T Invoke<T>(Func<T> function, TimeSpan timeout) => Invoke(args => function(), timeout); // ignore CancelEventArgs

        public static void Invoke(Action<CancelEventArgs> action, TimeSpan timeout)
        {
            Invoke<int>(args =>
                { // pass a function that returns 0 & ignore result
                    action(args);

                    return 0;
                },
                    timeout);
        }

        public static void TryInvoke(Action action, TimeSpan timeout) => Invoke(args => action(), timeout); // ignore CancelEventArgs

        private static void Invoke(Action action, TimeSpan timeout)
        {
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            AsyncCallback callback = ar => waitHandle.Set();
            IAsyncResult asyncResult = action.BeginInvoke(callback, null);

            if (waitHandle.WaitOne(timeout))
            {
                action.EndInvoke(asyncResult);
            }
            else
            {
                throw new TimeoutException("Timeout.");
            }
        }
    }

    //public class ThreadSyncToken
    //{
    //    private string name;

    //    public ThreadSyncToken(string name) => this.name = name;

    //    public override string ToString() => this.name;
    //}
}
