using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple
{
    public static class WaitHandleExtensions
    {
        public static Task AsTask(this WaitHandle handle) => AsTask(handle, Timeout.InfiniteTimeSpan);

        public static Task AsTask(this WaitHandle handle, int timeoutMilliseconds) => AsTask(handle, TimeSpan.FromMilliseconds(timeoutMilliseconds));

        public static Task AsTask(this WaitHandle handle, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<object>();
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<object>)state;
                
                if (timedOut)
                    localTcs.TrySetCanceled();
                else
                    localTcs.TrySetResult(null);
            }, 
            tcs, timeout, executeOnlyOnce: true);
            
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
            
            return tcs.Task;
        }

//        public static Task WaitOneAsync(this WaitHandle waitHandle, CancellationToken cancellationToken, int timeoutMilliseconds = Timeout.Infinite)
//        {
//            if (waitHandle == null)
//                throw new ArgumentNullException(nameof(waitHandle));

//            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
//            CancellationTokenRegistration ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
//            TimeSpan timeout = timeoutMilliseconds > Timeout.Infinite ? TimeSpan.FromMilliseconds(timeoutMilliseconds) : Timeout.InfiniteTimeSpan;

//            RegisteredWaitHandle registration = ThreadPool.RegisterWaitForSingleObject(waitHandle,
//                (_, timedOut) =>
//                {
//                    if (timedOut)
//                        tcs.TrySetCanceled();
//                    else
//                        tcs.TrySetResult(true);
//                },
//                null, timeout, executeOnlyOnce: true);

//            Task<bool> task = tcs.Task;

//#if NETSTANDARD
//            _ = task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
//#else
//            _ = task.ContinueWith(_ => { registration.Unregister(null); return ctr.Unregister(); }, CancellationToken.None); // ctr.Unregister() not supported in netstandard2.1
//#endif

//            return task;
//        }

        //public static Task WaitOneAsync(this WaitHandle waitHandle, int timeoutMilliseconds = Timeout.Infinite)
        //{
        //    if (waitHandle == null)
        //        throw new ArgumentNullException("waitHandle");

        //    var tcs = new TaskCompletionSource<bool>();
        //    var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, callBack: delegate { tcs.TrySetResult(true); }, state: null, timeoutMilliseconds, executeOnlyOnce: true);
        //    var task = tcs.Task;
            
        //    task.ContinueWith((antecedent) => rwh.Unregister(null));
            
        //    return task;
        //}

        //public static bool WaitOne(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        //{
        //    int n = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout);

        //    switch (n)
        //    {
        //        case WaitHandle.WaitTimeout:

        //            return false;

        //        case 0:

        //            return true;

        //        default:

        //            cancellationToken.ThrowIfCancellationRequested();

        //            return false; // never reached
        //    }
        //}

        //public static bool WaitOne(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        //{
        //    return handle.WaitOne((int)timeout.TotalMilliseconds, cancellationToken);
        //}

        //public static bool WaitOne(this WaitHandle handle, CancellationToken cancellationToken)
        //{
        //    return handle.WaitOne(Timeout.Infinite, cancellationToken);
        //}


        public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);

            try
            {
                var tcs = new TaskCompletionSource<bool>();

                registeredHandle = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs, millisecondsTimeout, true);
                tokenRegistration = cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs);

                return await tcs.Task;
            }
            finally
            {
                if (registeredHandle != null)
                    registeredHandle.Unregister(null);

                tokenRegistration.Dispose();
            }
        }

        public static async Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        public static async Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
        {
            return await handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
        }
    }
}
