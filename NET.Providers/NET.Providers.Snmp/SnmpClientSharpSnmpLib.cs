using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
//using System.Runtime.Remoting.Messaging;
using Simple;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Simple.Network;

namespace NET.Tools.Snmp
{
	public class SnmpClientSharpSnmpLib : ISnmpClient, IDisposable
	{
		#region |   Private Members   |

		private SnmpVersion snmpVersion = SnmpVersion.V2;
		private string remoteHost = null;
		private int remotePort = 161;
		private string communityString = "public";
		private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.SHA1;
		private string authenticationPassword = String.Empty;
		private string username = String.Empty;
		private string password = String.Empty;
		private SnmpEncryptionProtocol encryptionProtocol = SnmpEncryptionProtocol.AES256;
		private string encryptionPassword = String.Empty;
		private int remoteEngineBoots;
		private int remoteEngineTime;
		private const string RequestTimeOutMessage = "Timed out. Maximum number of retries has been reached without receiving a reply.";

		private VersionCode versionCode;
		private IPEndPoint remoteEndPoint;
		private OctetString community;
		private IAuthenticationProvider authenticationProvider;
		private SecurityParameters securityParameters;

		private string lastOid;

		#endregion |   Private Members   |

		#region |   Constructors and Initialization   |

		public SnmpClientSharpSnmpLib()
		{
			this.SetVersionCode();
			this.SetRemoteEndpoint();
			this.SetCommunity();
			this.SetAuthenticationProvider();
			this.SetSecurityParameters();
		}

		#endregion

		#region |   Public Events   |

		public event SnmpResponseEventHandler OnResponse;

		/// <summary>
		/// Occurs when an SNMP agent is found using discovery.
		/// </summary>
		public event EventHandler<AgentFoundEventArgs> AgentFound;

		#endregion |   Public Events   |

		#region |   Public Properties   |

		public SnmpVersion SnmpVersion
		{
			get { return this.snmpVersion; }
			set
			{
				this.snmpVersion = value;
				this.SetVersionCode();
			}
		}

		public string Community
		{
			get { return this.communityString; }
			set
			{
				this.communityString = value;
				this.SetCommunity();
			}
		}

		public string RemoteHost
		{
			get { return this.remoteHost; }
			set
			{
				this.remoteHost = value;
				this.SetRemoteEndpoint();
			}
		}

		public int RemotePort
		{
			get { return this.remotePort; }
			set
			{
				this.remotePort = value;
				this.SetRemoteEndpoint();
			}
		}


		/// <summary>
		/// Timeout in seconds.
		/// </summary>
		
		public int Timeout { get; set; }
		
		public int NumberOfRetries { get; set; }

		public SnmpAuthenticationProtocol AuthenticationProtocol
		{
			get { return this.authenticationProtocol; }
			set
			{
				this.authenticationProtocol = value;
				this.SetAuthenticationProvider();
			}
		}

		public string AuthenticationPassword
		{
			get { return this.authenticationPassword; }
			set
			{
				this.authenticationPassword = value;
			}
		}

		public string Username
		{
			get { return this.username; }
			set
			{
				this.username = value;
				this.SetSecurityParameters();
			}
		}
		public string Password
		{
			get { return this.password; }
			set
			{
				this.password = value;
				this.SetSecurityParameters();
			}
		}

		public SnmpEncryptionProtocol EncryptionProtocol
		{
			get { return this.encryptionProtocol; }
			set
			{
				this.encryptionProtocol = value;
				this.SetSecurityParameters();
			}
		}

		public string EncryptionPassword
		{
			get { return this.encryptionPassword; }
			set
			{
				this.encryptionPassword = value;
				this.SetSecurityParameters();
			}
		}

		public int RemoteEngineBoots
		{
			get { return this.remoteEngineBoots; }
			set
			{
				this.remoteEngineBoots = value;
				this.SetSecurityParameters();
			}
		}

		public int RemoteEngineTime
		{
			get { return this.remoteEngineTime; }
			set
			{
				this.remoteEngineTime = value;
				this.SetSecurityParameters();
			}
		}

		#endregion |   Public Properties   |

		#region |   Public Methods   |

		//public SnmpData Get(string oid)
		//{
		//	IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };

		//	if (this.SnmpVersion == SnmpVersion.V3)
		//	{
		//		GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(oid), variables, this.pri priv, Messenger.MaxMessageSize, report);

		//		ISnmpMessage reply = request.GetResponse(60000, this.remoteEndPoint);

		//		if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
		//		{
		//			throw ErrorException.Create(
		//				"error in response",
		//				IPAddress.Parse("192.168.1.2"),
		//				reply);
		//		}
		//	}
		//	else
		//	{
		//		try
		//		{
		//			variables = Messenger.Get(this.versionCode, this.remoteEndPoint, this.community, variables, this.Timeout * 1000);
		//		}
		//		catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
		//		{
		//			throw new System.TimeoutException();
		//		}
		//		catch (Exception ex)
		//		{
		//			throw ex;
		//		}

		//		this.lastOid = (variables.Count > 0) ? oid : null;

		//		return variables.ToSnmpData(oid);
		//	}
		//}

		public SnmpData Get(string oid)
		{
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			//IList<Variable> resultVariables = null;
			//Exception exception = null;
			GetRequestMessage requestMessage;
			ISnmpMessage response = null;
			SnmpData result = SnmpData.Empty;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetRequestMessage(Messenger.NextRequestId, this.versionCode, this.community, variables);

				//for (int i = 0; i < this.NumberOfRetries; i++)
				//{
				//	try
				//	{
				//		resultVariables = Messenger.Get(this.versionCode, this.remoteEndPoint, this.community, variables, this.Timeout * 1000);
				//		break;
				//	}
				//	catch (Exception ex)
				//	{
				//		exception = ex;
				//		continue;
				//	}
				//}

				//if (resultVariables != null)
				//	this.lastOid = (resultVariables.Count > 0) ? oid : null;

				//return resultVariables.ToSnmpData(oid);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new Exception("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				requestMessage = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = this.GetResponse(requestMessage, this.Timeout);

			if (response != null)
				result = response.ToSnmpData();

			return result;
		}

#if NET40

		public bool GetAsync(string oid, object userToken)
		{
			return this.GetAsync(oid, userToken, this.OnResponse);

			//IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			//UserRegistry userRegistry = new UserRegistry();
			//Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//GetRequestMessage requestMessage;

			//if (this.SnmpVersion != SnmpVersion.V3)
			//{
			//	requestMessage = new GetRequestMessage(Messenger.NextRequestId, this.versionCode, this.community, variables);
			//}
			//else
			//{
			//	if (this.Username.IsNullOrEmpty())
			//	{
			//		this.lastOid = null;
			//		//throw new ArgumentException("Username need to be specified for v3.");

			//		return false;
			//	}

			//	Levels securityLevel = Levels.Reportable;
			//	IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
			//																												: DefaultAuthenticationProvider.Instance;
			//	IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
			//																							: new DefaultPrivacyProvider(authentication);
			//	Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
			//	ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
			//	requestMessage = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, Messenger.MaxMessageSize, report);
			//}

			//requestMessage.BeginGetResponse(this.remoteEndPoint, userRegistry, udpSocket, this.AsyncCallback, state: this.OnResponse); // new AsyncCallbackState(requestMessage, this.OnResponse));

			//return true;
		}

		public async Task<ISnmpMessage> GetAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
		{
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			UserRegistry userRegistry = new UserRegistry();
			Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			GetRequestMessage requestMessage;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetRequestMessage(Messenger.NextRequestId, this.versionCode, this.community, variables);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					
					throw new ArgumentException("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				requestMessage = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, Messenger.MaxMessageSize, report);
			}

			//requestMessage.BeginGetResponse(this.remoteEndPoint, userRegistry, udpSocket, this.AsyncCallback, state: new AsyncState(requestMessage, userToken, responseCallback));
			return await requestMessage.GetResponseAsync(this.remoteEndPoint, userRegistry, udpSocket);
		}

#else

//		public async Task<ISnmpMessage> GetAsync(string oid, object userToken) //, SnmpResponseEventHandler responseCallback)
		public async ValueTask<SnmpData> GetAsync(string oid) //, SnmpResponseEventHandler responseCallback)
		{
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			UserRegistry userRegistry = new UserRegistry();
			Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			GetRequestMessage requestMessage;
			ISnmpMessage response = null;
			SnmpData result = SnmpData.Empty;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				// V1 & V2 requests
				requestMessage = new GetRequestMessage(Messenger.NextRequestId, this.versionCode, this.community, variables);
			}
			else // V3 requests
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new ArgumentException("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				
				requestMessage = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = await this.GetResponseAsync(requestMessage);

			if (response != null)
				result = response.ToSnmpData();

			return result;
		}

		public bool GetAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
		{
			throw new NotImplementedException();
		}

#endif

		public SnmpData GetNext()
		{
			return this.GetNext(this.lastOid);
		}

		public SnmpData GetNext(string oid)
		{
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			GetNextRequestMessage requestMessage;
			ISnmpMessage response;
			SnmpData result = SnmpData.Empty;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetNextRequestMessage(0, this.versionCode, this.community, variables);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new  Exception("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				requestMessage = new GetNextRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, 
														   privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = this.GetResponse(requestMessage, this.Timeout);

			if (response != null)
				result = response.ToSnmpData();

			return result;
		}

		public async ValueTask<SnmpData> GetNextAsync()
		{
			return await this.GetNextAsync(this.lastOid);
		}

		public async ValueTask<SnmpData> GetNextAsync(string oid)
		{
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid)) };
			GetNextRequestMessage requestMessage;
			ISnmpMessage response;
			SnmpData result = SnmpData.Empty;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetNextRequestMessage(0, this.versionCode, this.community, variables);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new Exception("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				
				requestMessage = new GetNextRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, 
														   privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = await this.GetResponseAsync(requestMessage);

			if (response != null)
				result = response.ToSnmpData();

			return result;
		}

		public bool GetNextAsync(object userToken)
		{
			throw new NotImplementedException();
		}

		public bool GetNextAsync(string oid, object userToken)
		{
			throw new NotImplementedException();
		}

		public bool GetNextAsync(object userToken, SnmpResponseEventHandler responseCallback)
		{
			throw new NotImplementedException();
		}

		public bool GetNextAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
		{
			throw new NotImplementedException();
		}

		public SnmpData[] GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
		{
			int requestId = 0;
			IList<Variable> variables = new Variable[oids.Length];
			int bulkTimeout = this.Timeout * 7;
			GetBulkRequestMessage requestMessage = null;
			ISnmpMessage response = null;
			SnmpData[] result = null;

			for (int i = 0; i < oids.Length; i++)
				variables[i] = new Variable(new ObjectIdentifier(oids[i]));

			if (this.SnmpVersion != SnmpVersion.V3) // V1 & V2 SNMP version
			{
				requestMessage = new GetBulkRequestMessage(requestId, this.versionCode, this.community, nonRepeaters, maxRepetitions, variables);
			}
			else // this.SnmpVersion == SnmpVersion.V3
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new Exception("Username need to be specified for SNMP v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000 * 7, this.remoteEndPoint);

				requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
														   nonRepeaters, maxRepetitions, variables, privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = this.GetResponse(requestMessage, bulkTimeout);

			if (response != null)
				result = response.Pdu().Variables.ToSnmpDataArray();

			return result;
		}

		public SnmpData[] GetBulk2(int nonRepeaters, int maxRepetitions, params string[] oids)
		{
			SnmpData[] result = null;
			int requestId = 0;
			IList<Variable> variables = new Variable[oids.Length];
			int bulkTimeout = this.Timeout * 7;
			GetBulkRequestMessage requestMessage;
			ISnmpMessage response = null;

			for (int i = 0; i < oids.Length; i++)
				variables[i] = new Variable(new ObjectIdentifier(oids[i]));


			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetBulkRequestMessage(requestId, this.versionCode, this.community, nonRepeaters, maxRepetitions, variables);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					throw new Exception("Username need to be specified for SNMP v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(bulkTimeout * 1000, this.remoteEndPoint);
				requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
															nonRepeaters, maxRepetitions, variables, privacyProvider, Messenger.MaxMessageSize, report);

			}

			response = this.GetResponse(requestMessage, bulkTimeout);

			if (response != null)
				result = response.Pdu().Variables.ToSnmpDataArray();

			return result;
		}

		public async ValueTask<SnmpData[]> GetBulkAsync(int nonRepeaters, int maxRepetitions, params string[] oids)
		{
			SnmpData[] result = null;
			int requestId = 0;
			IList<Variable> variables = new Variable[oids.Length];
			int bulkTimeout = this.Timeout * 7;
			UserRegistry userRegistry = new UserRegistry();
			Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			GetBulkRequestMessage requestMessage;
			ISnmpMessage response = null; ;

			for (int i = 0; i < oids.Length; i++)
				variables[i] = new Variable(new ObjectIdentifier(oids[i]));

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetBulkRequestMessage(requestId, this.versionCode, this.community, nonRepeaters, maxRepetitions, variables);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					
					throw new Exception("Username need to be specified for SNMP v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(bulkTimeout, this.remoteEndPoint);
				
				requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
														   nonRepeaters, maxRepetitions, variables, privacyProvider, Messenger.MaxMessageSize, report);
			}

			response = await this.GetResponseAsync(requestMessage);

			if (response != null)
				result = response.Pdu().Variables.ToSnmpDataArray();

			return result;
		}

		///// <summary>
		///// Gets the request messaeg SNMP response.
		///// </summary>
		///// <param name="requestMessage">Request Message</param>
		///// <param name="timeout">Timeout in seconds.</param>
		///// <returns></returns>
		//private ISnmpMessage GetResponse(ISnmpMessage requestMessage, int timeout)
		//{
		//	ISnmpMessage response = null;
		//	Exception exception = null;

		//	for (int i = 0; i < this.NumberOfRetries; i++)
		//	{
		//		try
		//		{
		//			response = requestMessage.GetResponse(timeout * 1000, this.remoteEndPoint);
		//		}
		//		catch (Exception ex)
		//		{
		//			exception = ex;
		//			this.lastOid = null;
					
		//			continue;
		//		}

		//		if (response != null)
		//			break;
		//	}

		//	if (!this.ValidateResponse(response, exception))
		//		response = null;

		//	return response;
		//}


		public async ValueTask<SnmpData[]> GetBulkAsync2(int nonRepeaters, int maxRepetitions, params string[] oids)
		{
			SnmpData[] result = null;
			int requestId = 0;
			IList<Variable> variables = new Variable[oids.Length];
			UserRegistry userRegistry = new UserRegistry();
			Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			GetBulkRequestMessage requestMessage;
			ISnmpMessage response;

			for (int i = 0; i < oids.Length; i++)
				variables[i] = new Variable(new ObjectIdentifier(oids[i]));

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				requestMessage = new GetBulkRequestMessage(requestId, this.versionCode, this.community, nonRepeaters, maxRepetitions, variables);
				//response = await requestMessage.GetResponseAsync(this.remoteEndPoint);
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;
					
					throw new Exception("Username need to be specified for SNMP v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000 * 7, this.remoteEndPoint);
				
				requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
														   nonRepeaters, maxRepetitions, variables, privacyProvider, Messenger.MaxMessageSize, report);

				response = await requestMessage.GetResponseAsync(this.remoteEndPoint);

				if (response is ReportMessage)
				{
					if (response.Pdu().Variables.Count == 0)
					{
						this.lastOid = null;
						
						return null; // new SnmpData(oid, SnmpObjectValueType.OctetString, "Wrong report message received");
					}
					else
					{
						var id = response.Pdu().Variables[0].Id;

						if (id != Messenger.NotInTimeWindow)
						{
							this.lastOid = null;
							
							return new SnmpData[0]; // new SnmpData(oid, SnmpObjectValueType.OctetString, id.GetErrorMessage());
						}
					}

					// according to RFC 3414, send a second request to sync time.
					requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
															   nonRepeaters, maxRepetitions, variables, privacyProvider, Messenger.MaxMessageSize, report);
					//response = await requestMessage.GetResponseAsync(this.remoteEndPoint);

				}
				else if (response.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
				{
					this.lastOid = null;
					
					throw new Exception("Error in response: " + Conversion.TryChangeType<SnmpRequestInfo>(response.Pdu().ErrorStatus.ToInt32()).ToString());
				}
			}

			response = await requestMessage.GetResponseAsync(this.remoteEndPoint);

			if (response.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
			{
				this.lastOid = null;
				
				throw new Exception("Error in response: " + Conversion.TryChangeType<SnmpRequestInfo>(response.Pdu().ErrorStatus.ToInt32()).ToString());
			}

			result = response.Pdu().Variables.ToSnmpDataArray();
			this.lastOid = (result.Length > 0) ? result[result.Length - 1].OID : null;

			return result;
		}

		public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, params string[] oids)
		{
			throw new NotImplementedException();
		}

		public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, SnmpResponseEventHandler responseCallback, params string[] oids)
		{
			throw new NotImplementedException();
		}

		public bool Set(string oid, object value)
		{
			ISnmpData data = this.CreateSnmpData(value);
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid), data) };

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				try
				{
					IList<Variable> variable = Messenger.Set(this.versionCode, this.remoteEndPoint, this.community, variables, this.Timeout * 1000);
				}
				catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
				{
					throw new System.TimeoutException();
				}
				//catch (Exception ex)
				//{
				//	throw ex;
				//}

				return true;
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;

					throw new Exception("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.SetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				SetRequestMessage requestMessage = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, report);
				ISnmpMessage response;

				try
				{
					response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				}
				catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
				{
					throw new System.TimeoutException();
				}
				//catch (Exception ex)
				//{
				//	throw ex;
				//}

				if (response is ReportMessage)
				{
					if (response.Pdu().Variables.Count == 0)
						throw new Exception("Wrong report message received");

					var id = response.Pdu().Variables[0].Id;

					if (id != Messenger.NotInTimeWindow)
						throw new Exception(id.GetErrorMessage());

					// according to RFC 3414, send a second request to sync time.
					requestMessage = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, response);

					try
					{
						response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
					}
					catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
					{
						throw new System.TimeoutException();
					}
					//catch (Exception ex)
					//{
					//	throw ex;
					//}
				}
				else if (response.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
				{
					throw new Exception("Error in response: " + Conversion.TryChangeType<SnmpRequestInfo>(response.Pdu().ErrorStatus.ToInt32()).ToString());
				}
			}

			this.lastOid = oid;

			return true;
		}

		public async ValueTask<bool> SetAsync(string oid, object value)
		{
			ISnmpData data = this.CreateSnmpData(value);
			IList<Variable> variables = new List<Variable>() { new Variable(new ObjectIdentifier(oid), data) };
			bool result = false;

			if (this.SnmpVersion != SnmpVersion.V3)
			{
				var variableResult = await Messenger.SetAsync(this.versionCode, this.remoteEndPoint, this.community, variables).WithTimeout(TimeSpan.FromSeconds(this.Timeout));

				result = variableResult.Count > 0;
			}
			else
			{
				if (this.Username.IsNullOrEmpty())
				{
					this.lastOid = null;

					throw new Exception("Username need to be specified for v3.");
				}

				Levels securityLevel = Levels.Reportable;
				IAuthenticationProvider authentication = ((securityLevel & Levels.Authentication) == Levels.Authentication) ? this.authenticationProvider
																															: DefaultAuthenticationProvider.Instance;
				IPrivacyProvider privacyProvider = ((securityLevel & Levels.Privacy) == Levels.Privacy) ? new AESPrivacyProvider(new OctetString(this.Password), authentication) as IPrivacyProvider
																										: new DefaultPrivacyProvider(authentication);
				Discovery discovery = Messenger.GetNextDiscovery(SnmpType.SetRequestPdu);
				ReportMessage report = discovery.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
				SetRequestMessage requestMessage = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, report);
				ISnmpMessage response = await requestMessage.GetResponseAsync(this.remoteEndPoint);

				if (response is ReportMessage)
				{
					if (response.Pdu().Variables.Count == 0)
						throw new Exception("Wrong report message received");

					var id = response.Pdu().Variables[0].Id;

					if (id != Messenger.NotInTimeWindow)
						throw new Exception(id.GetErrorMessage());

					// according to RFC 3414, send a second request to sync time.
					requestMessage = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, privacyProvider, response);
					response = await requestMessage.GetResponseAsync(this.remoteEndPoint);

				}

				result = response.Pdu().ErrorStatus.ToInt32() != 0; // 0 == ErrorCode.NoError
			}

			this.lastOid = oid;

			return result;
		}

		public bool SetAsync(string oid, object value, object userToken, SnmpResponseEventHandler responseCallback)
		{
			throw new NotImplementedException();
		}

		// Use reflection to access this private property of Messenger.
		private static Lazy<PropertyInfo> RequestCounterProperty { get; } = new Lazy<PropertyInfo>(()
			=> typeof(Messenger).GetProperty("RequestCounter", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic));

		private static NumberGenerator RequestCounter => RequestCounterProperty.Value.GetValue(null) as NumberGenerator;
		
		/// <summary>
		/// Sets a list of variable binds.
		/// </summary>
		/// <param name="version">Protocol version.</param>
		/// <param name="endpoint">Endpoint.</param>
		/// <param name="community">Community name.</param>
		/// <param name="variables">Variable binds.</param>
		/// <param name="cancellationToken">Cancels the async request.</param>
		public static async Task<IList<Variable>> SetAsync(VersionCode version, IPEndPoint endpoint, OctetString community, IList<Variable> variables)
		{
			if (endpoint == null)
				throw new ArgumentNullException(nameof(endpoint));

			if (community == null)
				throw new ArgumentNullException(nameof(community));

			if (variables == null)
				throw new ArgumentNullException(nameof(variables));

			if (version == VersionCode.V3)
				throw new NotSupportedException("SNMP v3 is not supported");

			var message = new SetRequestMessage(RequestCounter.NextId, version, community, variables);
			var response = await message.GetResponseAsync(endpoint);
			var pdu = response.Pdu();
			
			if (pdu.ErrorStatus.ToInt32() != 0)
				throw ErrorException.Create("error in response",endpoint.Address, response);

			return pdu.Variables;
		}

		public void Discover()
		{
			//Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetBulkRequestPdu);
			Discoverer discoverer = new Discoverer();
			
			discoverer.AgentFound += DiscovererAgentFound;
			//Console.WriteLine("v1 discovery");
			discoverer.Discover(VersionCode.V1, new IPEndPoint(IPAddress.Broadcast, this.RemotePort), new OctetString(this.Community), this.Timeout * 1000);
			//Console.WriteLine("v2 discovery");
			discoverer.Discover(VersionCode.V2, new IPEndPoint(IPAddress.Broadcast, this.RemotePort), new OctetString(this.Community), this.Timeout * 1000);
			//Console.WriteLine("v3 discovery");
			discoverer.Discover(VersionCode.V3, new IPEndPoint(IPAddress.Broadcast, this.RemotePort), null, this.Timeout * 1000);
		}

		public void Dispose()
		{
			this.remoteEndPoint = null;
			this.community = null;
			this.authenticationProvider = null;
			this.securityParameters = null;
		}

		#endregion |   Public Methods   |

		#region |   Private Methods   |

		private void SetVersionCode()
		{
			if (this.snmpVersion == SnmpVersion.V1)
			{
				this.versionCode = VersionCode.V1;
			}
			else if (this.snmpVersion == SnmpVersion.V2)
			{
				this.versionCode = VersionCode.V2;
			}
			else
			{
				this.versionCode = VersionCode.V3;
			}
		}

		private void SetRemoteEndpoint()
		{
			if (!this.RemoteHost.IsNullOrEmpty())
			{
				IPAddress ipAddress = (this.RemoteHost != null) ? DnsHelper.ResolveIPAddressFromHostname(this.RemoteHost) : null;
				this.remoteEndPoint = new IPEndPoint(ipAddress, this.RemotePort);
			}
			else
			{
				this.remoteEndPoint = null;
			}
		}

		private void SetCommunity()
		{
			string community = (this.Community == null) ? String.Empty : this.Community;
			
			this.community = new OctetString(community);
		}

		private void SetAuthenticationProvider()
		{
			switch (this.AuthenticationProtocol)
			{
				case SnmpAuthenticationProtocol.MD5:

#pragma warning disable CS0618 // Type or member is obsolete
					this.authenticationProvider = new MD5AuthenticationProvider(new OctetString(this.Password ?? String.Empty));
#pragma warning restore CS0618 // Type or member is obsolete

					break;

				case SnmpAuthenticationProtocol.SHA1:

#pragma warning disable CS0618 // Type or member is obsolete
					this.authenticationProvider = new SHA1AuthenticationProvider(new OctetString(this.Password ?? String.Empty));
#pragma warning restore CS0618 // Type or member is obsolete

					break;

				case SnmpAuthenticationProtocol.SHA256:

					this.authenticationProvider = new SHA256AuthenticationProvider(new OctetString(this.Password ?? String.Empty));

					break;

				case SnmpAuthenticationProtocol.SHA384:

					this.authenticationProvider = new SHA384AuthenticationProvider(new OctetString(this.Password ?? String.Empty));

					break;

				case SnmpAuthenticationProtocol.SHA512:

					this.authenticationProvider = new SHA512AuthenticationProvider(new OctetString(this.Password ?? String.Empty));

					break;

				default:

					throw new ArgumentException("Authentication protocol is not suported: " + this.AuthenticationProtocol.ToString());
			}
		}

		private void SetSecurityParameters()
		{
			// TODO: Check if this paramaters are correct
			// NOTE: This this.securityParameters has no meaning (is not in action)!
			string engineId = String.Empty;
			string authenticationProtocols = this.AuthenticationProtocol.ToString();
			string privacyParameters = String.Empty;

			this.securityParameters = new SecurityParameters(new OctetString(engineId), new Integer32(this.RemoteEngineBoots), new Integer32(this.RemoteEngineTime), new OctetString(this.Username),
															 new OctetString(authenticationProtocols), new OctetString(privacyParameters));
		}

		private void DiscovererAgentFound(object sender, AgentFoundEventArgs e)
		{
			if (this.AgentFound != null)
				this.AgentFound(sender, e);

			//Console.WriteLine("{0} announces {1}", e.Agent, (e.Variable == null ? "it supports v3" : e.Variable.Data.ToString()));
		}

		/// <summary>
		/// Gets the request messaeg SNMP response.
		/// </summary>
		/// <param name="requestMessage"></param>
		/// <returns></returns>
		private ISnmpMessage GetResponse(ISnmpMessage requestMessage, int timeout)
		{
			ISnmpMessage response = null;
			Exception exception = null;

			for (int i = 0; i < this.NumberOfRetries; i++)
			{
				try
				{
					response = requestMessage.GetResponse(timeout, this.remoteEndPoint);

					if (response is ReportMessage)
					{
						if (response.Pdu().Variables.Count == 0)
						{
							this.lastOid = null;
							throw new Exception("Wrong report message received");
						}
						else
						{
							var id = response.Pdu().Variables[0].Id;

							if (id != Messenger.NotInTimeWindow)
							{
								this.lastOid = null;
								throw new Exception(id.GetErrorMessage());
							}
						}

						// according to RFC 3414, send a second request to sync time.
						response = requestMessage.GetResponse(timeout, this.remoteEndPoint);
					}
				}
				catch (Exception ex)
				{
					exception = ex;
					this.lastOid = null;

					continue;
				}

				if (response != null)
					break;
			}

			if (!this.ValidateResponse(response, exception))
				response = null;

			return response;
		}

		/// <summary>
		/// Gets the async request messaeg SNMP response.
		/// </summary>
		/// <param name="requestMessage"></param>
		/// <returns></returns>
		private async ValueTask<ISnmpMessage> GetResponseAsync(ISnmpMessage requestMessage)
		{
			ISnmpMessage response = null;
			Exception exception = null;

			for (int i = 0; i < this.NumberOfRetries; i++)
			{
				//try
				//{
					response = await requestMessage.GetResponseAsync(this.remoteEndPoint).WithTimeout(TimeSpan.FromSeconds(this.Timeout));

					if (response is ReportMessage)
					{
						if (response.Pdu().Variables.Count == 0)
						{
							this.lastOid = null;
							throw new Exception("Wrong report message received");
						}
						else
						{
							var id = response.Pdu().Variables[0].Id;

							if (id != Messenger.NotInTimeWindow)
							{
								this.lastOid = null;
								throw new Exception(id.GetErrorMessage());
							}
						}

						// according to RFC 3414, send a second request to sync time.
						response = await requestMessage.GetResponseAsync(this.remoteEndPoint).WithTimeout(TimeSpan.FromSeconds(this.Timeout));
					}
				//}
				//catch (Exception ex)
				//{
				//	exception = ex;
				//	this.lastOid = null;

				//	continue;
				//}

				if (response != null)
						break;

				if (i > 0)
					response = response;
			}

			if (!this.ValidateResponse(response, exception))
				response = null;

			return response;
		}

		private bool ValidateResponse(ISnmpMessage response, Exception exception)
		{
			if (response != null)
			{
				//if (response.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
				//{
				//	this.lastOid = null;
				//	throw new Exception(ErrorException.Create("Error in response", this.remoteEndPoint.Address, response).ToString());
				//}

				this.lastOid = (response.Pdu().Variables.Count > 0) ? response.Pdu().Variables.ElementAt(response.Pdu().Variables.Count - 1).Id.ToString() : null;

				return true;
			}
			else if (exception != null)
			{
				this.lastOid = null;

				if (exception is Lextm.SharpSnmpLib.Messaging.TimeoutException)
				{
					throw new System.TimeoutException();
				}
				else
				{
					throw exception;
				}
			}

			return false;
		}

		private ISnmpData CreateSnmpData(object value)
		{
			ISnmpData data;


			if (value == null)
			{
				data = new Null();
			}
			else
			{
				Type valueType = value.GetType();

				if (valueType == typeof(string))
				{
					data = new OctetString(value as string);
				}
				else if (valueType == typeof(int) || valueType == typeof(short) || valueType == typeof(byte))
				{
					data = new Integer32((int)value);
				}
				else if (valueType == typeof(long))
				{
					data = new Gauge32((long)value);
				}
				else if (valueType == typeof(TimeSpan))
				{
					data = new TimeTicks((TimeSpan)value);
				}
				else if (valueType == typeof(IPAddress))
				{
					data = new IP(((IPAddress)value).GetAddressBytes());
				}
				else
				{
					data = new OctetString(value.ToString());
				}
			}

			return data;
		}


#if NET40

		private void AsyncCallback(IAsyncResult asyncResult)
		{
			AsyncState state = asyncResult.AsyncState as AsyncState;
			ISnmpMessage response = state.Caller.EndGetResponse(asyncResult);

			// Process reply
			ISnmpPdu pdu = response.Pdu();
			IList<Variable> variables = pdu.Variables;
			SnmpData[] values = new SnmpData[variables.Count];
			int requestId = pdu.RequestId.ToInt32();
			int errorStatus = pdu.ErrorStatus.ToInt32();
			int errorIndex = pdu.ErrorIndex.ToInt32();
			SnmpRequestResult snmpResult = (SnmpRequestResult)errorStatus;
			string errorDescription = String.Empty;

			for (int i = 0; i < values.Length; i++)
			{
				Variable variable = variables[i];
				values[i] = new SnmpData(variable.Id.ToString(), (SnmpObjectValueType)variable.Data.TypeCode, variable.Data.ToString());
			}

			if (response.Version != VersionCode.V3)
			{
				this.lastOid = (values.Length > 0) ? values[0].OID : null;
				//SnmpResponseEventArgs snmpResponse = new SnmpResponseEventArgs(pdu.RequestId.ToInt32(), pdu.ErrorStatus.ToInt32(), pdu.ErrorIndex.ToInt32(), String.Empty, values, userToken: null);
				//state.ResponseCallback(snmpResult, snmpResponse);

				//return;
			}
			else
			{
				if (response is ReportMessage)
				{
					if (variables.Count == 0)
					{
						this.lastOid = null;
						errorDescription = "Wrong report message received";
					}
					else
					{
						var id = variables[0].Id;

						if (id != Messenger.NotInTimeWindow)
						{
							this.lastOid = null;
							errorDescription = id.GetErrorMessage();
						}
						else
						{
							// according to RFC 3414, send a second request to sync time.
							bool secondRequestSucceeded = false;

							if (state.Caller is GetRequestMessage)
							{
								GetRequestMessage requestMessage = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, response.Privacy, Messenger.MaxMessageSize, response);
								response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
								secondRequestSucceeded = true;
							}
							else if (state.Caller is GetNextRequestMessage)
							{
								GetNextRequestMessage requestMessage = new GetNextRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, response.Privacy, Messenger.MaxMessageSize, response);
								response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
								secondRequestSucceeded = true;
							}
							else if (state.Caller is GetBulkRequestMessage)
							{
								int nonRepeater = pdu.ErrorStatus.ToInt32();
								int maxRepetitions = pdu.ErrorIndex.ToInt32();

								GetBulkRequestMessage requestMessage = new GetBulkRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username),
																								 nonRepeater, maxRepetitions, variables,
																								 response.Privacy, Messenger.MaxMessageSize, response);
								response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
								secondRequestSucceeded = true;
							}
							else if (state.Caller is SetRequestMessage)
							{
								SetRequestMessage requestMessage = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(this.Username), variables, response.Privacy, Messenger.MaxMessageSize, response);
								response = requestMessage.GetResponse(this.Timeout * 1000, this.remoteEndPoint);
								secondRequestSucceeded = true;
							}

							if (secondRequestSucceeded) // recalculate values
							{
								pdu = response.Pdu();
								variables = pdu.Variables;
								values = new SnmpData[variables.Count];

								for (int i = 0; i < values.Length; i++)
								{
									Variable variable = variables[i];
									values[i] = new SnmpData(variable.Id.ToString(), (SnmpObjectValueType)variable.Data.TypeCode, variable.Data.ToString());
								}

								requestId = pdu.RequestId.ToInt32();
								errorStatus = pdu.ErrorStatus.ToInt32();
								errorIndex = pdu.ErrorIndex.ToInt32();
								snmpResult = (SnmpRequestResult)errorStatus;
							}
						}
					}

				}
				else if (errorStatus != 0) // != ErrorCode.NoError
				{
					this.lastOid = null;

					IPAddress remoteIpAddress = (this.remoteEndPoint != null) ? this.remoteEndPoint.Address : IPAddress.None;
					errorDescription = ErrorException.Create("Error in response", remoteIpAddress, response).ToString();
				}
			}

			state.ResponseCallback(snmpResult, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, errorDescription, values, state.UserToken));
		}

#endif

		#endregion |   Private Methods   |

		#region |   Private Raise Events   |

		private void RaiseOnResponse(SnmpRequestInfo requestResult, int requestId, int errorStatus, int errorIndex, string errorDescription, SnmpData[] values)
		{
			if (this.OnResponse != null)
			{
				//object userToken = this.userTokensByRequestId[requestId];

				this.OnResponse(requestResult, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, errorDescription, values, userToken: null));
			}

			//this.userTokensByRequestId.Remove(requestId);
		}

		#endregion |   Private Raise Events   |

		#region |   ISnmpClient Interface   |

		ValueTask<SnmpData> ISnmpClient.GetAsync(string oid) => this.GetAsync(oid);
		
		#endregion |   ISnmpClient Interface   |

		#region |   Private Classes   |

		private sealed class AsyncState
		{
			public AsyncState(ISnmpMessage caller, object userToken, SnmpResponseEventHandler responseCallback)
			{
				this.Caller = caller;
				this.UserToken = userToken;
				this.ResponseCallback = responseCallback;
			}

			public ISnmpMessage Caller { get; private set; }
			public object UserToken { get; private set; }
			public SnmpResponseEventHandler ResponseCallback { get; private set; }
		}

		#endregion |   Private Classes   |
	}
}
