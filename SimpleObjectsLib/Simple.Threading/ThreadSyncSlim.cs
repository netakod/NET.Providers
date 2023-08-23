using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.ComponentModel;

namespace Simple.Threading
{
    public class ThreadSyncSlim : IDisposable
    {
        private bool exceptionOnTimeout = true;
        Hashtable resetEvents = new Hashtable();

        public ThreadSyncSlim()
        {
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

        public static T Invoke<T>(Func<T> function, TimeSpan timeout)
        {
            return Invoke(args => function(), timeout); // ignore CancelEventArgs
        }

        public static void Invoke(Action<CancelEventArgs> action, TimeSpan timeout)
        {
            Invoke<int>(args =>
            { // pass a function that returns 0 & ignore result
                action(args);
                return 0;
            }, timeout);
        }

        public static void TryInvoke(Action action, TimeSpan timeout)
        {
            Invoke(args => action(), timeout); // ignore CancelEventArgs
        }

        static void Invoke(Action action, TimeSpan timeout)
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

        public bool ExceptionOnTimeout
        {
            get { return this.exceptionOnTimeout; }
            set { this.exceptionOnTimeout = value; }
        }

        public virtual void WaitFor(object token)
        {
            this.WaitFor(token, Int32.MaxValue);
        }

		/// <summary>
		/// Blocks the current thread until the current System.Threading.WaitHandle receives a signal, using a 32-bit signed integer to specify the time interval in milliseconds.
		/// </summary>
		/// <param name="token">The token to waite to be released.</param>
		/// <param name="timeout">The timeout in milliseconds.</param>
		public virtual void WaitFor(object token, int timeout)
        {
            ManualResetEventSlim resetEvent = this.GetResetEvent(token);
            
            resetEvent.Wait(timeout);

            if (!this.IsReleased(token))
            {
                this.DisposeResetEvent(token);

                if (this.exceptionOnTimeout)
                    throw new TimeoutException(String.Format("{0} timeout.", token));
            }
        }

        public virtual void Prepare(object token)
        {
            // GetResetEvent will only create ManualResetEventSlim without calling Wait on it.
            ManualResetEventSlim resetEvent = this.GetResetEvent(token);
        }

        public virtual void Join(object token, int timeout)
        {
            // Join should be called after Prepare, so ManualResetEventSlim should be in hashtable.
            // If Release is already called, join immediately.
            if (this.resetEvents.ContainsKey(token))
            {
                this.WaitFor(token, timeout);
            }
        }

        public virtual bool Release(object token)
        {
            bool released = false;

            if (this.ResetEvents.ContainsKey(token))
            {
                ManualResetEventSlim resetEvent = this.GetResetEvent(token);
                
                this.ResetEvents.Remove(token);
                resetEvent.Set();
                resetEvent.Reset(); // .Close();

                released = true;
            }

            return released;
        }

        public bool ContainsToken(object token)
        {
            return this.ResetEvents.ContainsKey(token);
        }

        public void Dispose()
        {
            string[] tokens = new string[this.ResetEvents.Keys.Count];
            this.ResetEvents.Keys.CopyTo(tokens, 0);

            foreach (string token in tokens)
            {
                this.DisposeResetEvent(token);
            }
        }

        protected Hashtable ResetEvents
        {
            get { return Hashtable.Synchronized(this.resetEvents); }
        }

        private ManualResetEventSlim GetResetEvent(object token)
        {
            ManualResetEventSlim resetEvent;

            if (this.ResetEvents.ContainsKey(token))
            {
                resetEvent = (ManualResetEventSlim)this.ResetEvents[token];
            }
            else
            {
                resetEvent = new ManualResetEventSlim(false);
                this.ResetEvents.Add(token, resetEvent);
            }

            return resetEvent;
        }

        private bool IsReleased(object token)
        {
            return !this.ResetEvents.ContainsKey(token);
        }

        private void DisposeResetEvent(object token)
        {
            if (this.ResetEvents.ContainsKey(token))
            {
                ManualResetEventSlim resetEvent = (ManualResetEventSlim)this.ResetEvents[token];
                this.ResetEvents.Remove(token);

                resetEvent.Reset();
            }
        }
    }

  //  public class ThreadSyncToken
  //  {
  //      private string name;

		//public ThreadSyncToken(string name) => this.name = name;

		//public override string ToString() => this.name;
  //  }
}
