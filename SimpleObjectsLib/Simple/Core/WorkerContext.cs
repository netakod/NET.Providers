using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Simple
{
	public class WorkerContext<TResult, TArgument> : WorkerContext<TResult> 
    {
        [MaybeNull]
        public new TArgument Argument 
        {
            get 
            { 
                if (base.Argument is TArgument arg)
                    return arg;

                return default;
            }
            
            set { base.Argument = value; }
        }
    }

    public class WorkerContext<TResult> : WorkerContext 
	{
        [MaybeNull]
        public new TResult Result
		{
			get 
            { 
                if (base.Result is TResult result)
                    return result;

                return default; // default!
            }
			
            set { base.Result = value; }
		}
	}

	public class WorkerContext
    {
        public object? Result { get; set; }
        public object? Argument { get; set; }
        public bool Canceled { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
        public BackgroundWorker? Worker { get; set; }
        public DoWorkEventArgs? DoWorkArgs { get; set; }

        public void SignalCancel()
        {
            this.Worker?.CancelAsync();
        }

        public void ReportProgress(object state)
        {
            this.ReportProgress(0, state);
        }

        public void ReportProgress(int prograssPercentage, object state)
        {
            if (this.Worker != null && this.Worker.WorkerReportsProgress)
                this.Worker.ReportProgress(prograssPercentage, state);
        }

        public bool ShouldCancel()
        {
            bool cancel = false;

            if (this.Worker != null && this.Worker.CancellationPending)
            {
                if (this.DoWorkArgs != null)
                    this.DoWorkArgs.Cancel = true;

                cancel = true;
            }

            return cancel;
        }
    }
}

