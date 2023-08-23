//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple;
//using NET.Tools.Providers;

//namespace Simple.Services
//{
//    public class ProviderManager : IProviderManager
//    {
//        private static ProviderManager instance = null;
//        //private object lockObject = new object();
//        private static object lockObjectInstance = new object();
//		private object lockObject = new object();

//        private int nextProviderToken = 1;
//        private Dictionary<int, object> providerObjectsByProviderToken = new Dictionary<int, object>();

//        public static ProviderManager Instance
//        {
//            get { return GetInstance<ProviderManager>(); }
//        }

//        public event ProviderRequstResultEventHandler RequestRecieved;

//        public ProviderActivationInfo ActivateProvider(int providerModelType, IDictionary<string, object> providerPropertyData)
//        {
//            ProviderActivationInfo result = null;
//			object providerObject = null;

//			//try
//			//{
//                Type providerType = ProviderDiscoveryManager.Instance.GetProviderType(providerModelType);

//                if (providerType != null)
//                {
//                    providerObject = Activator.CreateInstance(providerType);

//                    if (!(providerObject is IProvider))
//                    {
//                        result = new ProviderActivationInfo(-1, false, ProviderActivationStatus.ProviderDoesNotHaveRequiredInterface, "Provider type " + providerType + " does not have implemented IProvider interface");
//                    }
//                    else if (!(providerObject is IProviderConnection))
//                    {
//                        result = new ProviderActivationInfo(-1, false, ProviderActivationStatus.ProviderDoesNotHaveRequiredInterface, "Provider type " + providerType + " does not have implemented IDeviceConnection interface");
//                    }
//                    else
//                    {
//                        IProvider provider = providerObject as IProvider;

//                        // Sets provider properties including connection string, operating systems, etc.
//                        foreach (var providerPropertyDataItem in providerPropertyData)
//                            provider.SetPropertyValue(providerPropertyDataItem.Key, providerPropertyDataItem.Value);

//                        // Set Owner and Activate device connection
//                        IProviderConnection deviceConnection = providerObject as IProviderConnection;
//                        deviceConnection.Owner = this;
//                        //DeviceConnectionInfo providerConnectionInfo = providerConnection.Connect();

//                        //if (!providerConnectionInfo.Success)
//                        //{
//                        //    result = new ProviderActivationInfo(-1, false, ProviderActivationStatus.ConnectionError, providerConnectionInfo.Message);
//                        //}
//                        //else
//                        //{

//						int providerToken;

//						lock (this.lockObject)
//						{
//							providerToken = this.nextProviderToken++;

//							while (this.providerObjectsByProviderToken.ContainsKey(providerToken))
//							{
//								providerToken = this.nextProviderToken++;
//							}

//							this.providerObjectsByProviderToken.Add(providerToken, provider);
//						}

//                        result = new ProviderActivationInfo(providerToken, true, ProviderActivationStatus.ProviderSuccessfullyActivated, String.Empty);
//                        //}
//                    }
//                }
//			//}
//			//catch (Exception ex)
//			//{
//			//	result = new ProviderActivationInfo(-1, false, ProviderActivationStatus.ExceptionIsCaught, ex.Message);
//			//}

//            return result;
//        }

//        public ProviderActivationInfo DeactivateProvider(int providerToken)
//        {
//            ProviderActivationInfo result = null;

//			object providerObject = null;
//			bool tokenExists = false;

//			lock (this.lockObject)
//			{
//				if (this.providerObjectsByProviderToken.ContainsKey(providerToken))
//				{
//					tokenExists = true;
//					providerObject = this.providerObjectsByProviderToken[providerToken];
//					this.providerObjectsByProviderToken.Remove(providerToken);
//				}
//			}

//			try
//            {
//				if (tokenExists)
//				{
//					(providerObject as IProviderConnection).Disconnect();

//                    if (providerObject is IDisposable)
//                    {
//                        (providerObject as IDisposable).Dispose();
//                    }

//                    providerObject = null;

//                    result = new ProviderActivationInfo(providerToken, true, ProviderActivationStatus.ProviderSeccessfullyDectivated, String.Empty);
//				}
//                else
//                {
//                    result = new ProviderActivationInfo(providerToken, false, ProviderActivationStatus.UnknownProviderToken, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//                result = new ProviderActivationInfo(providerToken, false, ProviderActivationStatus.ExceptionIsCaught, ex.Message);
//            }

//            return result;
//        }

//        public ProviderRequestResult SendRequest(int providerToken, string moduleName, string methodName, object[] arguments)
//        {
//            return this.SendRequest<object>(providerToken, moduleName, methodName, arguments, SendingMethod.Synchronous);
//        }


//        public ProviderRequestResult SendRequest(int providerToken, string moduleName, string methodName, object[] arguments, SendingMethod sendingMethod)
//        {
//            return this.SendRequest<object>(providerToken, moduleName, methodName, arguments, sendingMethod);
//        }

//        public ProviderRequestResult<TResult> SendRequest<TResult>(int providerToken, string moduleName, string methodName, object[] arguments)
//        {
//            return this.SendRequest<TResult>(providerToken, moduleName, methodName, arguments, SendingMethod.Synchronous);
//        }
        
//        // TODO: implement sync - async method with events
//        public ProviderRequestResult<TResult> SendRequest<TResult>(int providerToken, string moduleName, string methodName, object[] arguments, SendingMethod sendingMethod)
//        {
//            ProviderRequestResult<TResult> result = null;
//            object providerObject = null;

//            try
//            {
//                if (this.providerObjectsByProviderToken.TryGetValue(providerToken, out providerObject))
//                {
//                    IProvider provider = providerObject as IProvider;

//                    result = provider.InvokeMethod<TResult>(moduleName, methodName, arguments);
//                    result.ProviderToken = providerToken;
//                }
//                else
//                {
//                    result = new ProviderRequestResult<TResult>(providerToken, false, null, ProviderRequestActionResultInfo.UnknownProviderToken, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//                bool success = false;
//                string message = (ex.InnerException ?? ex).Message;


//                if (ex.InnerException != null && ex.InnerException is ProviderInfoException)
//                    success = true;

//                result = new ProviderRequestResult<TResult>(providerToken, success, null, ProviderRequestActionResultInfo.ExceptionIsCaught, message);
//            }

//            this.OnRequestRecieved(result);
//            this.RaiseRequestRecieved(result);
            
//            return result;
//        }

//        public ProviderRequestResult ConnectDeviceConnection(int providerToken)
//        {
//            ProviderRequestResult result = null;
//            object providerObject = null;
//			IProviderConnection deviceConnection = null;

//            try
//            {
//                if (this.providerObjectsByProviderToken.TryGetValue(providerToken, out providerObject))
//                {
//                    deviceConnection = providerObject as IProviderConnection;
//					IRequestResult deviceConnectionInfo = deviceConnection.Connect();
//                    result = new ProviderRequestResult(providerToken, deviceConnectionInfo.Succeed, null, ProviderRequestActionResultInfo.RequestSucceeded, deviceConnectionInfo.Message);
//                }
//                else
//                {
//                    result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.UnknownProviderToken, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//				if (deviceConnection != null)
//					deviceConnection.Disconnect();
				
//				//string message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
//                string message = (ex.InnerException ?? ex).Message;
//                result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.ExceptionIsCaught, message);
//            }

//            this.OnRequestRecieved(result);
//            this.RaiseRequestRecieved(result);

//            return result;
//        }

//        public ProviderRequestResult TestConnection(int providerToken, WorkerContext workerContext)
//        {
//            ProviderRequestResult result = null;
//            object providerObject = null;
//			IProviderConnection deviceConnection = null;

//            try
//            {
//                if (this.providerObjectsByProviderToken.TryGetValue(providerToken, out providerObject))
//                {
//                    deviceConnection = providerObject as IProviderConnection;
//					IRequestResult deviceConnectionInfo = deviceConnection.TestConnection(workerContext);
//                    //DeviceConnectionInfo deviceConnectionInfo = workerContext.Result as DeviceConnectionInfo;

//                    result = new ProviderRequestResult(providerToken, deviceConnectionInfo.Succeed, deviceConnectionInfo, ProviderRequestActionResultInfo.RequestSucceeded, deviceConnectionInfo.Message);
//                }
//                else
//                {
//                    result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.UnknownProviderToken, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//				if (deviceConnection != null)
//					deviceConnection.Disconnect();
				
//				//string message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
//                string message = (ex.InnerException ?? ex).Message;
//                result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.ExceptionIsCaught, message);
//            }

//            this.OnRequestRecieved(result);
//            this.RaiseRequestRecieved(result);

//            return result;
//        }

//        public ProviderRequestResult SetLogging(int providerToken, string logFileName)
//        {
//            ProviderRequestResult result = null;
//            object providerObject = null;
//			IProviderConnection deviceConnection = null;

//            try
//            {
//                if (this.providerObjectsByProviderToken.TryGetValue(providerToken, out providerObject))
//                {
//                    deviceConnection = providerObject as IProviderConnection;
//                    deviceConnection.SetLogging(logFileName);
//                    result = new ProviderRequestResult(providerToken, true, null, ProviderRequestActionResultInfo.RequestSucceeded, String.Empty);
//                }
//                else
//                {
//                    result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.UnknownProviderToken, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//				if (deviceConnection != null)
//					deviceConnection.Disconnect();

//				//string message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
//                string message = (ex.InnerException ?? ex).Message;
//                result = new ProviderRequestResult(providerToken, false, null, ProviderRequestActionResultInfo.ExceptionIsCaught, message);
//            }

//            this.OnRequestRecieved(result);
//            this.RaiseRequestRecieved(result);

//            return result;
//        }

//		public void Shutdown()
//        {
//            lock (lockObjectInstance)
//            {
//                while (this.providerObjectsByProviderToken.Count > 0)
//                {
//                    this.DeactivateProvider(this.providerObjectsByProviderToken.Keys.ElementAt(0));
//                }
//            }
//        }

//        //public ProviderRequstResult<T> SendRequest<T>(int providerToken, string moduleName, string methodName, object[] arguments)
//        //{
//        //    ProviderRequstResult providerRequstResult = this.SendRequest(providerToken, moduleName, methodName, arguments);
//        //    ProviderRequstResult<T> result = new ProviderRequstResult<T>(providerRequstResult);

//        //    return result;
//        //}

//        //public ProviderRequstResult<T> SendRequest<T>(int providerToken, string moduleName, string methodName, object[] arguments, SendingMethod sendingMethod)
//        //{
//        //    ProviderRequstResult providerRequstResult = this.SendRequest(providerToken, moduleName, methodName, arguments, sendingMethod);
//        //    ProviderRequstResult<T> result = new ProviderRequstResult<T>(providerRequstResult);

//        //    return result;
//        //}

//        protected virtual void OnRequestRecieved(ProviderRequestResult providerRequstResult)
//        {
//        }

//        protected static T GetInstance<T>() where T : ProviderManager, new()
//        {
//            lock (lockObjectInstance)
//            {
//                if (instance == null)
//                {
//                    instance = new T();
//                }
//            }

//            return instance as T;
//        }

//        private void RaiseRequestRecieved(ProviderRequestResult providerRequstResult)
//        {
//            if (this.RequestRecieved != null)
//            {
//                this.RequestRecieved(this, new ProviderRequstResultArgs(providerRequstResult));
//            }
//        }
//    }
//}