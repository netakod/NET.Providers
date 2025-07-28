using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Simple
{
	public static class BackgroundWorkerExtensions
	{
        public static Task<object> RunWorkerTaskAsync(this BackgroundWorker backgroundWorker)
        {
            var tcs = new TaskCompletionSource<object>();

            RunWorkerCompletedEventHandler handler = (sender, args) =>
            {
                if (args.Cancelled)
                    tcs.TrySetCanceled();
                else if (args.Error != null)
                    tcs.TrySetException(args.Error);
                else if (args.Result != null)
                    tcs.TrySetResult(args.Result);
                //else
                //    tcs.TrySetResult(default);
            };

            backgroundWorker.RunWorkerCompleted += handler;
            
            try
            {
                backgroundWorker.RunWorkerAsync();
            }
            catch
            {
                backgroundWorker.RunWorkerCompleted -= handler;
                
                throw;
            }

            return tcs.Task;
        }
    }
}
