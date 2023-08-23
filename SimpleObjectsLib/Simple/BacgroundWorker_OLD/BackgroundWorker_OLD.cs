////// General Information about an assembly is controlled through the following 
////// set of attributes. Change these attribute values to modify the information
////// associated with an assembly.
////[assembly: AssemblyTitle("System.ComponentModel.Custom.Generic")]
////[assembly: AssemblyDescription("Contains a generic BackgroundWorker class and supporting classes.")]
////[assembly: AssemblyConfiguration("")]
////[assembly: AssemblyCompany("DaveyM69")]
////[assembly: AssemblyProduct("System.ComponentModel.Custom.Generic")]
////[assembly: AssemblyCopyright("Copyright © See CPOL (2009)")]
////[assembly: AssemblyTrademark("http://www.codeproject.com/Members/DaveyM69")]
////[assembly: AssemblyCulture("")]

////// Setting ComVisible to false makes the types in this assembly not visible 
////// to COM components.  If you need to access a type in this assembly from 
////// COM, set the ComVisible attribute to true on that type.
////[assembly: ComVisible(false)]

////// The following GUID is for the ID of the typelib if this project is exposed to COM
////[assembly: Guid("ad626a4e-04d8-4c8a-89ab-9f196993a5f6")]

////// Version information for an assembly consists of the following four values:
//////
//////      Major Version
//////      Minor Version 
//////      Build Number
//////      Revision
//////
////// You can specify all the values or you can default the Build and Revision Numbers 
////// by using the '*' as shown below:
////// [assembly: AssemblyVersion("1.0.*")]
////[assembly: AssemblyVersion("1.0.0.0")]
////[assembly: AssemblyFileVersion("1.0.0.0")]

//using System.Threading;
//using System.Diagnostics.CodeAnalysis;



//namespace System.ComponentModel.Custom.Generic
//{
//    /// <summary>
//    /// Executes an operation on a separate thread.
//    /// </summary>
//    /// <typeparam name="TArgument">The type of argument passed to the worker.</typeparam>
//    /// <typeparam name="TProgress">The type of ProgressChangedEventArgs.UserState.</typeparam>
//    /// <typeparam name="TResult">The type of result retrieved from the worker.</typeparam>
//    public class BackgroundWorker<TArgument, TProgress, TResult> 
//    {
//        #region Constants

//        public const int MinProgress = 0;
//        public const int MaxProgress = 100;

//        #endregion

//        #region Events

//        /// <summary>
//        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerAsync() is called.
//        /// </summary>
//        public event EventHandler<DoWorkEventArgs<TArgument, TResult>>? DoWork;
        
//        /// <summary>
//        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.ReportProgress(System.Int32) is called.
//        /// </summary>
//        public event EventHandler<ProgressChangedEventArgs<TProgress>>? ProgressChanged;
        
//        /// <summary>
//        /// Occurs when the background operation has completed, has been canceled, or has raised an exception.
//        /// </summary>
//        public event EventHandler<RunWorkerCompletedEventArgs<TResult>>? RunWorkerCompleted;

//        #endregion

//        #region Fields

//        #if NETSTANDARD2_0_OR_GREATER
//        [AllowNull]
//        #endif
//        private AsyncOperation? asyncOperation = null;
        
//        private readonly BasicDelegate threadStart;
//        private readonly SendOrPostCallback operationCompleted;
//        private readonly SendOrPostCallback progressReporter;

//        #endregion

//        #region Constructor

//        /// <summary>
//        /// Initializes a new instance of the System.ComponentModel.Custom.Generic.BackgroundWorker class.
//        /// </summary>
//        public BackgroundWorker()
//        {
//            this.threadStart = new BasicDelegate(this.WorkerThreadStart);
//            this.operationCompleted = new SendOrPostCallback(this.AsyncOperationCompleted);
//            this.progressReporter = new SendOrPostCallback(this.ProgressReporter);
//            this.WorkerReportsProgress = true;
//            this.WorkerSupportsCancellation = true;
//        }

//        #endregion

//        #region Properties

//        /// <summary>
//        /// Gets a value indicating whether the application has requested cancellation of a background operation.
//        /// </summary>
//        public bool CancellationPending { get; private set; }
        
//        /// <summary>
//        /// Gets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// is running an asynchronous operation.
//        /// </summary>
//        public bool IsBusy { get; private set; }
        
//        /// <summary>
//        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// can report progress updates. The default is true.
//        /// </summary>
//        public bool WorkerReportsProgress { get; set; }
        
//        /// <summary>
//        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// supports asynchronous cancellation. The default is true.
//        /// </summary>
//        public bool WorkerSupportsCancellation { get; set; }

//        #endregion

//        #region Methods

//        private void AsyncOperationCompleted(object? state)
//        {
//            this.IsBusy = false;
//            this.CancellationPending = false;
            
//            if (state is RunWorkerCompletedEventArgs<TResult> e)
//                this.OnRunWorkerCompleted(e);
//        }

//        /// <summary>
//        /// Requests cancellation of a pending background operation.
//        /// </summary>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker supports cancellation;
//        /// otherwise, false.</returns>
//        public bool CancelAsync()
//        {
//            if (!this.WorkerSupportsCancellation)
//                return false;
            
//            this.CancellationPending = true;
            
//            return true;
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.DoWorkEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnDoWork(DoWorkEventArgs<TArgument, TResult> e)
//        {
//            EventHandler<DoWorkEventArgs<TArgument, TResult>>? eh = this.DoWork;
            
//            if (eh != null)
//                eh(this, e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.ProgressChangedEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnProgressChanged(ProgressChangedEventArgs<TProgress> e)
//        {
//            EventHandler<ProgressChangedEventArgs<TProgress>>? eh = this.ProgressChanged;
            
//            if (eh != null)
//                eh(this, e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerCompleted event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.RunWorkerCompletedEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs<TResult> e)
//        {
//            EventHandler<RunWorkerCompletedEventArgs<TResult>>? eh = this.RunWorkerCompleted;
            
//            if (eh != null)
//                eh(this, e);
//        }

//        private void ProgressReporter(object? state) 
//        {
//            if (state is ProgressChangedEventArgs<TProgress> e)
//                this.OnProgressChanged(e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
//        /// otherwise, false.</returns>
//        public bool ReportProgress(int percentProgress)
//        {
//            return ReportProgress(percentProgress, default(TProgress));
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="percentProgress">The percentage, from MinProgress to MaxProgress,
//        /// of the background operation that is complete.</param>
//        /// <param name="userState">An object to be passed to the
//        /// System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
//        /// otherwise, false.</returns>
//        public bool ReportProgress(int percentProgress, TProgress? userState)
//        {
//            if (!this.WorkerReportsProgress)
//                return false;
            
//			if (percentProgress < MinProgress)
//                percentProgress = MinProgress;
//            else if (percentProgress > MaxProgress)
//                percentProgress = MaxProgress;
            
//			ProgressChangedEventArgs<TProgress> args = new ProgressChangedEventArgs<TProgress>(percentProgress, userState);
            
//			if (this.asyncOperation != null)
//                this.asyncOperation.Post(progressReporter, args);
//            else
//                this.progressReporter(args);
            
//            return true;
//        }
        
//		/// <summary>
//        /// Starts execution of a background operation.
//        /// </summary>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
//        /// otherwise, false.</returns>
//        public bool RunWorkerAsync() => RunWorkerAsync(default(TArgument));
        
//		/// <summary>
//        /// Starts execution of a background operation.
//        /// </summary>
//        /// <param name="argument">A parameter for use by the background operation to be executed in the
//        /// System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event handler.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
//        /// otherwise, false.</returns>
//        public bool RunWorkerAsync(TArgument? argument)
//        {
//            if (this.IsBusy)
//                return false;
            
//            this.IsBusy = true;
//            this.CancellationPending = false;
//            this.asyncOperation = AsyncOperationManager.CreateOperation(argument);
//            this.threadStart.BeginInvoke(null, null);
            
//            return true;
//        }


//		private void WorkerThreadStart()
//        {
//            #if NETSTANDARD2_0_OR_GREATER
//            [AllowNull]
//            #endif
//            TResult? workerResult = default(TResult);

//            #if NETSTANDARD2_0_OR_GREATER
//            [AllowNull]
//            #endif
//            Exception? error = null;
            
//            bool cancelled = false;
            
//			try
//            {
//                TArgument? argument = default;

//                if (this.asyncOperation != null && this.asyncOperation.UserSuppliedState is TArgument userSuppliedState)
//                    argument = userSuppliedState;

//                DoWorkEventArgs<TArgument, TResult> doWorkArgs = new DoWorkEventArgs<TArgument, TResult>(argument);
                
//				this.OnDoWork(doWorkArgs);
                
//				if (doWorkArgs.Cancel)
//                    cancelled = true;
//                else
//                    workerResult = doWorkArgs.Result;
//            }
//            catch (Exception exception)
//            {
//                error = exception;
//            }

//            if (this.asyncOperation != null)
//            {
//                RunWorkerCompletedEventArgs<TResult> e = new RunWorkerCompletedEventArgs<TResult>(workerResult, error, cancelled);

//                this.asyncOperation.PostOperationCompleted(operationCompleted, e);
//            }
//        }

//#endregion
//    }

//    /// <summary>
//    /// Executes an operation on a separate thread.
//    /// </summary>
//    /// <typeparam name="T">The type of argument passed to in and out of the worker.</typeparam>
//    public class BackgroundWorker<T> where T : notnull
//    {
//#region Constants

//        public const int MinProgress = 0;
//        public const int MaxProgress = 100;

//        #endregion

//        #region Events

//        /// <summary>
//        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerAsync() is called.
//        /// </summary>
//        public event EventHandler<DoWorkEventArgs<T>>? DoWork;
//        /// <summary>
//        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.ReportProgress(System.Int32) is called.
//        /// </summary>

//        public event EventHandler<ProgressChangedEventArgs<T>>? ProgressChanged;

//        /// <summary>
//        /// Occurs when the background operation has completed, has been canceled, or has raised an exception.
//        /// </summary>
//        public event EventHandler<RunWorkerCompletedEventArgs<T>>? RunWorkerCompleted;

//        #endregion

//        #region Fields

//        #if NETSTANDARD2_0_OR_GREATER
//        [MaybeNull]
//        #endif
//        private AsyncOperation? asyncOperation = null;
        
//        private readonly BasicDelegate threadStart;
//        private readonly SendOrPostCallback operationCompleted;
//        private readonly SendOrPostCallback progressReporter;

//#endregion

//#region Constructor

//        /// <summary>
//        /// Initializes a new instance of the System.ComponentModel.Custom.Generic.BackgroundWorker class.
//        /// </summary>
//        public BackgroundWorker()
//        {
//            this.threadStart = new BasicDelegate(this.WorkerThreadStart);
//            this.operationCompleted = new SendOrPostCallback(this.AsyncOperationCompleted);
//            this.progressReporter = new SendOrPostCallback(this.ProgressReporter);
//            this.WorkerReportsProgress = true;
//            this.WorkerSupportsCancellation = true;
//        }

//#endregion

//#region Properties

//        /// <summary>
//        /// Gets a value indicating whether the application has requested cancellation of a background operation.
//        /// </summary>
//        public bool CancellationPending { get; private set; }

//        /// <summary>
//        /// Gets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// is running an asynchronous operation.
//        /// </summary>
//        public bool IsBusy { get; private set; }
        
//        /// <summary>
//        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// can report progress updates. The default is true.
//        /// </summary>
//        public bool WorkerReportsProgress { get; set; }
        
//        /// <summary>
//        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
//        /// supports asynchronous cancellation. The default is true.
//        /// </summary>
//        public bool WorkerSupportsCancellation { get; set; }

//#endregion

//#region Methods

//        private void AsyncOperationCompleted(object? state)
//        {
//            this.IsBusy = false;
//            this.CancellationPending = false;
            
//            if (state is RunWorkerCompletedEventArgs<T> e)
//                this.OnRunWorkerCompleted(e);
//        }

//        /// <summary>
//        /// Requests cancellation of a pending background operation.
//        /// </summary>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker supports cancellation;
//        /// otherwise, false.</returns>
//        public bool CancelAsync()
//        {
//            if (!this.WorkerSupportsCancellation)
//                return false;
            
//            this.CancellationPending = true;
            
//            return true;
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.DoWorkEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnDoWork(DoWorkEventArgs<T> e)
//        {
//            EventHandler<DoWorkEventArgs<T>>? eh = this.DoWork;
            
//            if (eh != null)
//                eh(this, e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.ProgressChangedEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnProgressChanged(ProgressChangedEventArgs<T> e)
//        {
//            EventHandler<ProgressChangedEventArgs<T>>? eh = this.ProgressChanged;
            
//            if (eh != null)
//                eh(this, e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerCompleted event.
//        /// </summary>
//        /// <param name="e">A System.ComponentModel.Custom.Generic.RunWorkerCompletedEventArgs
//        /// that contains the event data.</param>
//        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs<T> e)
//        {
//            EventHandler<RunWorkerCompletedEventArgs<T>>? eh = this.RunWorkerCompleted;
            
//            if (eh != null)
//                eh(this, e);
//        }
//        private void ProgressReporter(object? state)
//        {
//            if (state is (ProgressChangedEventArgs<T> e))
//                this.OnProgressChanged(e);
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
//        /// otherwise, false.</returns>
//        public bool ReportProgress(int percentProgress)
//        {
//            return this.ReportProgress(percentProgress, default(T));
//        }

//        /// <summary>
//        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
//        /// </summary>
//        /// <param name="percentProgress">The percentage, from MinProgress to MaxProgress,
//        /// of the background operation that is complete.</param>
//        /// <param name="userState">An object to be passed to the
//        /// System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
//        /// otherwise, false.</returns>
//#if NETSTANDARD2_0_OR_GREATER
//        [MaybeNull]
//#endif
//        public bool ReportProgress(int percentProgress, T? userState)
//        {
//            if (!this.WorkerReportsProgress)
//                return false;
            
//            if (percentProgress < MinProgress)
//                percentProgress = MinProgress;
//            else if (percentProgress > MaxProgress)
//                percentProgress = MaxProgress;
            
//            ProgressChangedEventArgs<T> args = new ProgressChangedEventArgs<T>(percentProgress, userState);
            
//            if (this.asyncOperation != null)
//                this.asyncOperation.Post(progressReporter, args);
//            else
//                this.progressReporter(args);
            
//            return true;
//        }

//        /// <summary>
//        /// Starts execution of a background operation.
//        /// </summary>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
//        /// otherwise, false.</returns>
//        public bool RunWorkerAsync()
//        {
//            return this.RunWorkerAsync(default(T));
//        }
//        /// <summary>
//        /// Starts execution of a background operation.
//        /// </summary>
//        /// <param name="argument">A parameter for use by the background operation to be executed in the
//        /// System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event handler.</param>
//        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
//        /// otherwise, false.</returns>
//        public bool RunWorkerAsync(T? argument)
//        {
//            if (this.IsBusy)
//                return false;
            
//            this.IsBusy = true;
//            this.CancellationPending = false;
//            this.asyncOperation = AsyncOperationManager.CreateOperation(argument);
//            this.threadStart.BeginInvoke(null, null);
            
//            return true;
//        }
//        private void WorkerThreadStart()
//        {
//            #if NETSTANDARD2_0_OR_GREATER
//            [AllowNull]
//            #endif
//            T? workerResult = default(T);

//            #if NETSTANDARD2_0_OR_GREATER
//            [AllowNull]
//            #endif
//            Exception? error = null;
            
//            bool cancelled = false;
            
//            try
//            {
//                T? argument = default;

//                if (this.asyncOperation != null && this.asyncOperation.UserSuppliedState is T userSuppliedState)
//                    argument = userSuppliedState;

//                DoWorkEventArgs<T> doWorkArgs = new DoWorkEventArgs<T>(argument);
                
//                this.OnDoWork(doWorkArgs);
                
//                if (doWorkArgs.Cancel)
//                    cancelled = true;
//                else
//                    workerResult = doWorkArgs.Result;
//            }
//            catch (Exception exception)
//            {
//                error = exception;
//            }

//            if (this.asyncOperation != null)
//            {
//                RunWorkerCompletedEventArgs<T> e = new RunWorkerCompletedEventArgs<T>(workerResult, error, cancelled);

//                this.asyncOperation?.PostOperationCompleted(operationCompleted, e);
//            }
//        }

//#endregion
//    }
//}