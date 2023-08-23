﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net;
//using System.IO;
//using System.Threading;
//using Simple.Threading;
//using System.Collections;
//using Simple.Network;

//namespace NET.Tools.Web
//{
//	public class WebControl : IProviderConnection, IDisposable
//    {
//		private WebProtocol webProtocol = WebProtocol.HTTP;
//		private string remoteHost = String.Empty;
//		private int remotePort = 80;
//		private string username = String.Empty;
//		private string password = String.Empty;
//		private int timeout = 10; // in seconds
//		private int sendingInterval = 1; // in milliseconds
//		private bool useProxy = false;
//		private string proxy = String.Empty;
//		private int proxyPort = 8080;
//		private string proxyUsername = String.Empty;
//		private string proxyPassword = String.Empty;
//		private bool isPostAuthorisationAttempted = false;
//		private bool isConnectionInitiated = false;

//		protected WebRequestMethod ConnectionMethod = WebRequestMethod.GET;
//		protected string connectAction = "login.htm";
//        protected string usernamePostDataPrefix = "username=";
//        protected string passwordPostDataPrefix = "&password=";
//		protected string userAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

//		protected string logOffAction = "config/authentication_page.htm?logOff";
//        protected string logOffRefererAction = "home.htm";
//		protected string getContentType = "text/html";
//		protected string postContentType = "application/x-www-form-urlencoded";
//		protected string defaultRefererAction = "index.html";

//        //protected bool isConnectionInProgress = false;
//        //protected bool isDisconnectionInProgress = false;

//		private string resolvedRemoteHost = String.Empty;
//		private Cookie cookie = null;

//		public static  WebControl Default = new WebControl();
//		//private WebConnectionStringBuilder connectionStringBuilder = new WebConnectionStringBuilder();

//		public WebControl()
//		{
//		}

//		public object Owner { get; set; }
//        //public bool IsConnected { get; protected set; }

//   //     public string ConnectionString
//   //     {
//   //         get { return this.connectionStringBuilder.BuildConnectionString(); }
//   //         set
//			//{
//			//	this.connectionStringBuilder.SetConnectionString(value);
//			//	this.SetResolvedRemoteHost();
//			//}
//   //     }

//        //public bool UseWebConnection
//        //{
//        //    get { return this.connectionStringBuilder.UseWebConnection; }
//        //    set { this.connectionStringBuilder.UseWebConnection = value; }
//        //}

//        public WebProtocol WebProtocol
//        {
//            get { return this.webProtocol; }
//            set { this.webProtocol = value; }
//        }

//        public string RemoteHost
//        {
//            get { return this.remoteHost; }
//            set
//			{
//				this.remoteHost = value;
//				this.SetResolvedRemoteHost();
//			}
//		}

//        public int RemotePort
//        {
//            get { return this.remotePort; }
//            set { this.remotePort = value; }
//        }

//        public string Username
//        {
//            get { return this.username; }
//            set { this.username = value; }
//        }

//        public string Password
//        {
//            get { return this.password; }
//            set { this.password = value; }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.timeout; }
//            set { this.timeout = value; }
//        }

//        /// <summary>
//        /// Sending interval in miliseconds.
//        /// </summary>
//        public int SendingInterval
//        {
//            get { return this.sendingInterval; }
//            set { this.sendingInterval = value; }
//        }

//		public bool UseProxy
//		{
//			get { return this.useProxy; }
//			set { this.useProxy = value; }
//		}

//		public string Proxy
//		{
//			get { return this.proxy; }
//			set { this.proxy = value; }
//		}

//		public int ProxyPort
//		{
//			get { return this.proxyPort; }
//			set { this.proxyPort = value; }
//		}

//		public string ProxyUsername
//		{
//			get { return this.proxyUsername; }
//			set { this.proxyUsername = value; }
//		}

//		public string ProxyPassword
//		{
//			get { return this.proxyPassword; }
//			set { this.proxyPassword = value; }
//		}

//		protected IRequestResult Connect()
//		{
//			return this.Connect(this.ConnectionMethod);
//		}

//		protected IRequestResult Connect(WebRequestMethod requestMethod)
//        {
//			IRequestResult result = null;
//			HttpWebResponse response = null;
//			this.cookie = null;

//			//this.isConnectionInProgress = true;
//			bool connected = false;

//			switch (requestMethod)
//			{
//				case WebRequestMethod.GET:

//					response = this.SendGetRequest(this.connectAction, this.defaultRefererAction);

//					break;

//				case WebRequestMethod.POST:

//					string postData = String.Format("{0}{1}{2}{3}", this.usernamePostDataPrefix, this.Username, this.passwordPostDataPrefix, this.Password);

//					response = this.SendPostRequest(this.connectAction, null, postData);

//					break;

//				default: throw new ArgumentOutOfRangeException("Requested method is not supported: " + requestMethod.ToString());
//			}

//			if (response != null)
//			{
//				string cookieContainer = response.Headers["Set-Cookie"];

//				if (!cookieContainer.IsNullOrEmpty())
//				{
//					string[] cookieItems = cookieContainer.Split('=', ';');
//					string cookieName = cookieItems[0].Trim();
//					string sessionKey = cookieItems[1].Trim();

//					this.cookie = new Cookie(cookieName, sessionKey, "/", this.resolvedRemoteHost);

//					connected = true;
//				}

//				response.Close();
//				(response as IDisposable).Dispose();
//			}

//			result = (connected) ? new RequestResult<bool>(true, TaskResultInfo.Succeeded, "Success") :
//								   new RequestResult<bool>(false, TaskResultInfo.Error, "Username or password is not correct.");

//			this.isConnectionInitiated = connected;
            
//            return result;
//        }

//		public virtual IRequestResult TestConnection(WorkerContext workerContext)
//		{
//			string testResultMessage = "Web Connection Test: ";

//			if (workerContext != null)
//				workerContext.ReportProgress(-1, "Testing Web connection...");

//			IRequestResult result = this.Connect(); // Connect only gets the cookies

//			if (result.Succeeded)
//				result = this.OnTestConnection();

//			testResultMessage += (result.Succeeded) ? "Success" : result.Message;
//			result.Message = testResultMessage;

//			if (workerContext != null)
//				workerContext.Result = result;

//			return result;
//		}

//		public virtual void Disconnect()
//        {
//			string responseText = String.Empty;

//            //this.isDisconnectionInProgress = true;

//			if (this.isConnectionInitiated)
//			{
//				try
//				{
//					responseText = this.SendGetRequest(this.logOffAction, this.logOffRefererAction).GetResponseTextAsync();
//				}
//				catch
//				{
//				}
//				finally
//				{
//					this.isConnectionInitiated = false;
//				}
//			}

//			this.cookie = null;

//			//this.IsConnected = false;
//			//this.isDisconnectionInProgress = false;
//		}

//        public virtual void SetLogging(string logFileName)
//        {
//            // TODO:
//            // Not implemented yet for http requests
//        }

//		public virtual void FinishUpdate()
//		{
//			// TODO:
//			// Not implemented yet
//		}

//		//      public virtual string GetResponseText(string uriAction)
//		//      {
//		//          return this.GetResponseText(uriAction, null);
//		//      }
//		//public virtual string GetResponseText(string uriAction, string refererAction)
//		//{
//		//	HttpWebResponse webResponse = this.SendGetRequest(uriAction, refererAction);
//		//	String responseText = this.GetResponseText(webResponse);

//		//	return responseText;
//		//}

//		//public virtual string GetPostResponseText(string uriAction, string postData)
//		//{
//		//	return this.GetPostResponseText(uriAction, null, postData);
//		//}

//		//public virtual string GetPostResponseText(string uriAction, string refererAction, string postData)
//		//{
//		//	HttpWebResponse webResponse = this.SendPostRequest(uriAction, refererAction, postData);
//		//	String responseText = this.GetResponseText(webResponse);

//		//	return responseText;
//		//}

//		//public virtual string GetResponseText(HttpWebResponse webResponse)
//		//      {
//		//          string result = String.Empty;
//		//          StreamReader streamReader = null;

//		//	Thread.Sleep(this.SendingInterval);

//		//	try
//		//	{
//		//              streamReader = new StreamReader(webResponse.GetResponseStream());
//		//              result = streamReader.ReadToEnd();
//		//          }
//		//          catch (Exception ex)
//		//          {
//		//              throw ex;
//		//          }
//		//          finally
//		//          {
//		//              if (streamReader != null)
//		//              {
//		//                  streamReader.Close();
//		//                  streamReader.Dispose();
//		//              }

//		//              if (webResponse != null)
//		//              {
//		//                  webResponse.Close();
//		//                  (webResponse as IDisposable).Dispose();
//		//              }
//		//          }

//		//          return result;
//		//      }

//		public virtual HttpWebResponse SendGetRequest(string action)
//        {
//            return this.SendGetRequest(action, this.defaultRefererAction);
//        }

//        public virtual HttpWebResponse SendGetRequest(string uriAction, string refererAction)
//        {
//			Thread.Sleep(this.SendingInterval);

//			HttpWebRequest webRequest = this.CreateGetRequest(uriAction, refererAction);
//			HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
			
//			//var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
//			return response;
//        }

//		public virtual HttpWebResponse SendPostRequest(string action, string postData)
//		{
//			return this.SendPostRequest(action, null, postData);
//		}

//		public virtual HttpWebResponse SendPostRequest(string uriAction, string refererAction, string postData)
//		{
//			Thread.Sleep(this.SendingInterval);

//			HttpWebRequest webRequest = this.CreatePostRequest(uriAction, refererAction, postData);
//			HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;

//			//var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
//			return response;
//		}

//		protected virtual ICredentials CreateRequestCredentials()
//		{
//			return new NetworkCredential(this.Username, this.Password);
//		}

//		private HttpWebRequest CreateGetRequest(string uriAction, string refererAction)
//		{
//			HttpWebRequest webRequest = this.CreateRequest(uriAction, refererAction, this.getContentType);
//			webRequest.Method = "GET";

//			return webRequest;
//		}

//		private HttpWebRequest CreatePostRequest(string uriAction, string refererAction, string postData)
//		{
//			HttpWebRequest webRequest = this.CreateRequest(uriAction, refererAction, this.postContentType);
//			webRequest.Method = "POST";

//			byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(postData);
//			webRequest.ContentLength = dataBytes.Length;

//			Thread.Sleep(this.SendingInterval);

//			using (Stream postStream = webRequest.GetRequestStream())
//			{
//				postStream.Write(dataBytes, 0, dataBytes.Length);
//			}

//			return webRequest;
//		}

//		private HttpWebRequest CreateRequest(string uriAction, string refererAction, string contentType)
//		{
//			string uri = String.Format("http://{0}/{1}", this.resolvedRemoteHost, uriAction);
//			HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
//			string referrerAction = (refererAction.IsNullOrEmpty()) ? this.defaultRefererAction : refererAction;

//			webRequest.Timeout = this.Timeout * 1000;
//			webRequest.KeepAlive = true;
//			webRequest.AllowAutoRedirect = false;
//			webRequest.ContentType = contentType;
//			webRequest.Referer = String.Format("http://{0}/{1}", this.resolvedRemoteHost, refererAction);
//			webRequest.UserAgent = this.userAgent;
//			webRequest.Credentials = this.CreateRequestCredentials();
//			webRequest.PreAuthenticate = true;

//			if (this.UseProxy)
//			{
//				webRequest.Proxy = new WebProxy(this.Proxy, this.ProxyPort);

//				if (!this.ProxyUsername.IsNullOrEmpty())
//					webRequest.Proxy.Credentials = new NetworkCredential(this.ProxyUsername, this.ProxyPassword);
//			}
//			else
//			{
//				webRequest.Proxy = null;
//			}

//			if (this.ConnectionMethod == WebRequestMethod.POST && this.cookie == null && !this.isPostAuthorisationAttempted)
//			{
//				this.isPostAuthorisationAttempted = true;
//				this.Connect();
//			}

//			this.OnCreateRequest(webRequest);

//			if (this.cookie != null)
//			{
//				webRequest.CookieContainer = new CookieContainer();
//				webRequest.CookieContainer.Add(this.cookie);
//			}

//			return webRequest;
//		}

//		protected virtual void OnCreateRequest(HttpWebRequest webRequest)
//		{
//		}

//		//public virtual string GetPostResponseText(string action, string data)
//  //      {
//  //          return this.GetPostResponseText(action, null, data);
//  //      }

//  //      public virtual string GetPostResponseText(string action, string refererAction, string data)
//  //      {
//  //          string result = String.Empty;
//  //          HttpWebResponse webResponse = null;
//  //          StreamReader streamReader = null;

//  //          try
//  //          {
//  //              webResponse = this.GetPostResponse(action, refererAction, data);
//  //              streamReader = new StreamReader(webResponse.GetResponseStream());
//  //              result = streamReader.ReadToEnd();
//  //          }
//  //          catch
//  //          {
//  //              throw;
//  //          }
//  //          finally
//  //          {
//  //              if (streamReader != null)
//  //              {
//  //                  streamReader.Close();
//  //                  streamReader.Dispose();
//  //              }

//  //              if (webResponse != null)
//  //              {
//  //                  webResponse.Close();
//  //                  (webResponse as IDisposable).Dispose();
//  //              }
//  //          }

//  //          return result;
//  //      }

//  //      public virtual HttpWebResponse GetPostResponse(string action, string data)
//  //      {
//  //          return this.GetPostResponse(action, null, data);
//  //      }

//  //      public virtual HttpWebResponse GetPostResponse(string action, string refererAction, string data)
//  //      {
//  //          string uri = String.Format("http://{0}/{1}", this.ResolvedRemoteHost, action);
//  //          string referer = (!String.IsNullOrEmpty(refererAction)) ? String.Format("http://{0}/{1}", this.ResolvedRemoteHost, refererAction) : null;
            
//  //          return this.GetUriPostResponse(uri, referer, data);
//  //      }

//        public void Dispose()
//        {
//			this.cookie = null;
//        }

//		protected virtual IRequestResult OnTestConnection()
//		{
//			return RequestResult<object>.Successful; // If Connect method use POST method to get the cookies and thus use authentication, possible no further test actions require. 
//		}

//		private void SetResolvedRemoteHost()
//		{
//			this.resolvedRemoteHost = this.RemoteHost;

//			IPAddress ipAddress = DnsHelper.ResolveIPAddressFromHostname(this.RemoteHost);

//			if (ipAddress != null)
//				this.resolvedRemoteHost = ipAddress.ToString();

//		}
//		//protected virtual HttpWebResponse GetUriResponse(string uri, string referer)
//		//{
//		//    //if (!this.isConnectionInProgress && !this.isDisconnectionInProgress && !this.IsConnected)
//		//    //    this.Connect();

//		//    //if (this.sessionKey.IsNullOrEmpty())
//		//    //{
//		//    //    throw new InvalidOperationException("Not connected");
//		//    //}

//		//}

//		//     protected virtual HttpWebResponse GetUriPostResponse(string uri, string referer, string data)
//		//     {
//		//         if (!this.isConnectionInProgress && !this.isDisconnectionInProgress && !this.IsConnected)
//		//             this.Connect();

//		//         if (!this.isConnectionInProgress && this.sessionKey == String.Empty)
//		//         {
//		//             throw new InvalidOperationException("Not connected");
//		//         }

//		//         HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
//		//webRequest.Timeout = this.Timeout * 1000;
//		//         webRequest.KeepAlive = true;
//		//         webRequest.AllowAutoRedirect = false;
//		//         webRequest.Method = "POST";
//		//webRequest.ContentType = this.postContentType;
//		//webRequest.Proxy = null;
//		//webRequest.Credentials = this.CreateRequestCredentials();
//		//webRequest.PreAuthenticate = true;

//		//if (!String.IsNullOrEmpty(referer))
//		//             webRequest.Referer = referer;

//		//         if (!this.isConnectionInProgress)
//		//         {
//		//             Cookie cookie = new Cookie(cookieName, this.sessionKey, "/", this.ResolvedRemoteHost);
//		//             webRequest.CookieContainer = new CookieContainer();
//		//             webRequest.CookieContainer.Add(cookie);
//		//         }

//		//         byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(data);
//		//         webRequest.ContentLength = dataBytes.Length;

//		//         Thread.Sleep(this.SendingInterval);

//		//         using (Stream postStream = webRequest.GetRequestStream())
//		//         {
//		//             postStream.Write(dataBytes, 0, dataBytes.Length);
//		//         }

//		//         Thread.Sleep(this.SendingInterval);

//		//         HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
//		//         var reader = new StreamReader(webResponse.GetResponseStream());
//		//         //string result = reader.ReadToEnd();

//		//         return webResponse;
//		//     }
//	}

//	//public enum WebRequestMethod
//	//{
//	//	GET,
//	//	POST
//	//}
//}
