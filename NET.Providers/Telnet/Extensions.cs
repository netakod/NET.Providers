using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using System.Runtime.InteropServices;

namespace NET.Tools.Telnet
{
    public static class Extensions
    {
        //public static bool Contains(this string source, string value, StringComparison comparisonType) => source?.IndexOf(value, comparisonType) >= 0;

        //public static bool Contains(this string source, string value, bool ignoreCase)
        //{
        //    if (ignoreCase)
        //        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
        //    else
        //        return source.Contains(value);
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


        //public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        //{
        //    RegisteredWaitHandle registeredHandle = null;
        //    CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);

        //    try
        //    {
        //        var tcs = new TaskCompletionSource<bool>();

        //        registeredHandle = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs, millisecondsTimeout, true);
        //        tokenRegistration = cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs);

        //        return await tcs.Task;
        //    }
        //    finally
        //    {
        //        if (registeredHandle != null)
        //            registeredHandle.Unregister(null);

        //        tokenRegistration.Dispose();
        //    }
        //}

        //public static async Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        //{
        //    return await handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
        //}

        //public static async Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
        //{
        //    return await handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
        //}

        //public static ArraySegment<T> ToArraySegment<T>(this Memory<T> memory) => ((ReadOnlyMemory<T>)memory).ToArraySegment();

        //public static ArraySegment<T> ToArraySegment<T>(this ReadOnlyMemory<T> memory)
        //{
        //    if (!MemoryMarshal.TryGetArray(memory, out var result))
        //        throw new InvalidOperationException("Buffer backed by array was expected");

        //    return result;
        //}

        public static byte GetNext(this ref SequenceReader<byte> reader)
        {
            reader.TryRead(out byte result);

            return result;
        }

        public static bool IsIgnorableSocketException(this SocketException sex)
        {
            switch (sex.SocketErrorCode)
            {
                case (SocketError.OperationAborted):
                case (SocketError.ConnectionReset):
                case (SocketError.TimedOut):
                case (SocketError.NetworkReset):

                    return true;

                default:

                    return false;
            }
        }
    }
}
