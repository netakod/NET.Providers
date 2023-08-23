//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using Simple;
//using SnmpSharpNet;
//using Lextm.SharpSnmpLib.Messaging;

//namespace NET.Tools.Snmp
//{
//    // TODO: Create Async methods from new thread while CreateUdpTarget can take some time to resolve remote hostname if not specified an ip address.
    
//    /// <summary>
//    /// SNMP#Net version of the ISnmpManagerControl implementation.
//    /// </summary>
//    public class SnmpControlSnmpSharpNet : ISnmpControl, IDisposable
//    {
//        #region |   Private Members   |

//        IpAddress agent = new IpAddress();
//        AgentParameters nonSecureAgentParameters = new AgentParameters();
//        SecureAgentParameters secureAgentParameters = new SecureAgentParameters();
//        IAgentParameters agentParameters;
//		UdpTarget udpTarget = null;
//		int remotePort = 161;
//        int timeout = 10;
//        int numOfRetries = 4;
//        private SnmpVersion snmpVersion = SnmpVersion.V2;
//        private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.MD5;
//        private SnmpEncryptionProtocol encryptionProtocol = SnmpEncryptionProtocol.AES256;
//        private string remoteHost = String.Empty;
//        private string lastOid = null;
//        private Hashtable userTokensByRequestId = new Hashtable();

//		#endregion |   Private Members   |

//		#region |   Private Events   |

//		/// <summary>
//		/// SNMP request internal callback
//		/// </summary>
//		private event SnmpResponseEventHandler responseCallback;

//        #endregion |   Private Events   |

//        #region |   Public Events   |

//        public event SnmpResponseEventHandler OnResponse;

//        #endregion

//        #region |   Constructors and Initialization   |

//        public SnmpControlSnmpSharpNet()
//        {
//            this.agentParameters = (this.snmpVersion == SnmpVersion.V3) ? this.secureAgentParameters : this.nonSecureAgentParameters as IAgentParameters;
//			this.SetUdpTarget();
//        }

//        #endregion

//        #region |   Public Properties   |

//        public SnmpVersion SnmpVersion
//        {
//            get { return this.snmpVersion; }
//            set
//            {
//                this.snmpVersion = value;

//                switch (value)
//                {
//                    case SnmpVersion.V1:
//                        this.nonSecureAgentParameters.Version = SnmpSharpNet.SnmpVersion.Ver1;
//                        this.agentParameters = this.nonSecureAgentParameters;
//                        break;

//                    case SnmpVersion.V2:
//                        this.nonSecureAgentParameters.Version = SnmpSharpNet.SnmpVersion.Ver2;
//                        this.agentParameters = this.nonSecureAgentParameters;
//                        break;
                    
//                    case SnmpVersion.V3:
//                        this.nonSecureAgentParameters.Version = SnmpSharpNet.SnmpVersion.Ver3;
//                        this.agentParameters = this.secureAgentParameters;
//                        break;
//                }

//                this.ResetTarget();
//            }
//        }

//        public string CommunityString
//        {
//            get { return this.nonSecureAgentParameters.Community.ToString(); }
//            set
//            {
//                this.nonSecureAgentParameters.Community.Set(value);
//                this.ResetTarget();
//            }
//        }

//        public string RemoteHost
//        {
//            get { return this.remoteHost; } // this.agent.ToString(); }
//            set
//            {
//                this.remoteHost = value;
//                this.ResetTarget();
//				this.SetUdpTarget();
//            }
//        }

//        public int RemotePort
//        {
//            get { return this.remotePort; }
//            set
//            {
//                this.remotePort = value;
//                this.ResetTarget();
//            }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.timeout; }
//            set
//            {
//                if (value > 0)
//                {
//                    this.timeout = value;
//                    this.ResetTarget();
//                }

//				this.SetUdpTarget();
//            }
//        }

//        public int NumberOfRetries
//        {
//            get { return this.numOfRetries; }
//            set
//            {
//                if (value > 0)
//                {
//                    this.numOfRetries = value;
//                    this.ResetTarget();
//                }

//				this.SetUdpTarget();
//            }
//        }

//        public SnmpAuthenticationProtocol AuthenticationProtocol
//        {
//            get { return this.authenticationProtocol; }
//            set
//            {
//                this.authenticationProtocol = value;

//                switch (value)
//                {
//                    case SnmpAuthenticationProtocol.MD5:
//                        this.secureAgentParameters.Authentication = AuthenticationDigests.MD5;
//                        break;
                    
//                    case SnmpAuthenticationProtocol.SHA1:
//                        this.secureAgentParameters.Authentication = AuthenticationDigests.SHA1;
//                        break;
//                }

//                this.ResetTarget();
//            }
//        }

//        public string AuthenticationPassword
//        {
//            get { return this.secureAgentParameters.AuthenticationSecret.ToString(); }
//            set
//            {
//                this.secureAgentParameters.AuthenticationSecret.Set(value);
//                this.ResetTarget();
//            }
//        }

//        public string Username
//        {
//            get { return this.secureAgentParameters.SecurityName.ToString(); }
//            set
//            {
//                this.secureAgentParameters.SecurityName.Set(value);
//                this.ResetTarget();
//            }
//        }

//        public string Password
//        {
//            get { return this.secureAgentParameters.AuthenticationSecret.ToString(); }
//            set
//            {
//                this.secureAgentParameters.AuthenticationSecret.Set(value);
//                this.ResetTarget();
//            }
//        }

//        public SnmpEncryptionProtocol EncryptionProtocol
//        {
//            get { return this.encryptionProtocol; }
//            set
//            {
//                this.encryptionProtocol = value;

//                switch (value)
//                {
//                    case SnmpEncryptionProtocol.None:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.None;
//                        break;
                    
//                    case SnmpEncryptionProtocol.DES:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.DES;
//                        break;
                    
//                    case SnmpEncryptionProtocol.TripleDes:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.TripleDES;
//                        break;
                    
//                    case SnmpEncryptionProtocol.AES128:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.AES128;
//                        break;
                    
//                    case SnmpEncryptionProtocol.AES192:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.AES192;
//                        break;
                    
//                    case SnmpEncryptionProtocol.AES256:
//                        this.secureAgentParameters.Privacy = PrivacyProtocols.AES256;
//                        break;
//                }

//                this.ResetTarget();
//            }
//        }

//        public string EncryptionPassword
//        {
//            get { return this.secureAgentParameters.PrivacySecret.ToString(); }
//            set
//            {
//                this.secureAgentParameters.PrivacySecret.Set(value);
//                this.ResetTarget();
//            }
//        }

//        public int RemoteEngineBoots
//        {
//            get { return this.secureAgentParameters.EngineBoots; }
//            set
//            {
//                this.secureAgentParameters.EngineBoots.Set(value.ToString());
//                this.ResetTarget();
//            }
//        }

//        public int RemoteEngineTime
//        {
//            get { return this.secureAgentParameters.EngineTime; }
//            set
//            {
//                this.secureAgentParameters.EngineTime.Set(value.ToString());
//                this.ResetTarget();
//            }
//        }

//        #endregion

//        #region |   Public Methods   |

//        public SnmpData Get(string oid)
//        {
//			if (this.udpTarget == null)
//				return null; // new RequestResult<SnmpData>(null, RequestActionResult.Error, "UDP target is null");

//			SnmpData result = null;
//			Pdu pdu = this.CreatePduGet(oid);
//			SnmpPacket packet = this.udpTarget.Request(pdu, this.agentParameters);

//			// If result is null then agent didn't reply or we couldn't parse the reply.
//			if (packet != null)
//            {
//				SnmpData[] resultList = this.ProcessRecievedSnmpPacket(packet, this.udpTarget.Address.ToString());

//				if (resultList.Length > 0)
//                {
//                    result = resultList[0];
//                }
//                else
//                {
//                    // The value is null
//                    result = new SnmpData(oid, SnmpObjectValueType.OctetString, String.Empty);
//                }
                
//                this.lastOid = oid;
//            }

//			return result;
//        }

//#if NET40

//        public bool GetAsync(string oid,  object userToken)
//        {
//            Pdu pdu = this.CreatePduGet(oid);
//            return this.RequestAsync(pdu, userToken);
//        }

//#else

//		public Task<SnmpData> GetAsync(string oid)
//		{
//			Pdu pdu = this.CreatePduGet(oid);

//			// TODO:; Implement method this.RequestAsync(pdu, userToken) that returns Task value;
//			throw new NotImplementedException();
//			//return this.RequestAsync(pdu, userToken);
//		}

//#endif
//		public bool GetAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
//        {
//            Pdu pdu = this.CreatePduGet(oid);
//            return this.RequestAsync(pdu, userToken, responseCallback);
//        }

//        //// TODO:
//        //public void GetAsyncCancel()
//        //{
//        //    // TODO:
//        //}

//        public SnmpData GetNext()
//        {
//            return this.GetNext(this.lastOid);
//        }

//        public SnmpData GetNext(string oid)
//        {
//			if (this.udpTarget == null)
//				return null; // new RequestResult<SnmpData>(null, RequestActionResult.Error, "UDP target is null");

//			SnmpData result = null;
//			Pdu pdu = this.CreatePduGetNext(oid);
//			SnmpPacket request = this.udpTarget.Request(pdu, agentParameters);
		
//            // If result is null then agent didn't reply or we couldn't parse the reply.
//            if (request != null)
//            {
//				SnmpData[] resultList = this.ProcessRecievedSnmpPacket(request, this.udpTarget.Address.ToString());

//				if (resultList.Length > 0)
//                {
//                    result = resultList[0];
//                }
//                else
//                {
//                    result = new SnmpData(oid, SnmpObjectValueType.OctetString, String.Empty);
//                }

//                this.lastOid = result.OID;
//            }
//            else
//            {
//                this.lastOid = null;
//            }

//            return result;
//        }

//        public Task<SnmpData> GetNextAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public bool GetNextAsync(object userToken)
//        {
//            return this.GetNextAsync(this.lastOid, userToken);
//        }

//        public bool GetNextAsync(string oid, object userToken)
//        {
//            Pdu pdu = this.CreatePduGetNext(oid);
//            return this.RequestAsync(pdu, userToken);
//        }

//        public bool GetNextAsync(object userToken, SnmpResponseEventHandler responseCallback)
//        {
//            return this.GetNextAsync(this.lastOid, userToken, responseCallback);
//        }

//        public bool GetNextAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
//        {
//            Pdu pdu = this.CreatePduGetNext(oid);
//            return this.RequestAsync(pdu, userToken, responseCallback);
//        }

//        public SnmpData[] GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
//        {
//			SnmpData[] result = null;

//			if (this.udpTarget == null)
//				return new SnmpData[0]; // new RequestResult<IList<SnmpData>>(null, RequestActionResult.Error, "UDP target is null");

//            Pdu pdu = this.CreatePduGetBulk(nonRepeaters, maxRepetitions, oids);
//			SnmpPacket request = this.udpTarget.Request(pdu, this.agentParameters);

//			// If result is null then agent didn't reply or we couldn't parse the reply.
//			if (request != null)
//            {
//				result = this.ProcessRecievedSnmpPacket(request, this.udpTarget.Address.ToString());
//				this.lastOid = request.Pdu.VbList[request.Pdu.VbList.Count - 1].Oid.ToString();
//            }
//            else
//            {
//                this.lastOid = null;
//            }

//            //target.Close();

//            return result;
//        }

//        public Task<SnmpData[]> GetBulkAsync(int nonRepeaters, int maxRepetitions, params string[] oids)
//        {
//            throw new NotImplementedException();
//        }

//        public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, params string[] oids)
//        {
//            Pdu pdu = this.CreatePduGetBulk(nonRepeaters, maxRepetitions, oids);
//            return this.RequestAsync(pdu, userToken);
//        }

//        public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, SnmpResponseEventHandler responseCallback, params string[] oids)
//        {
//            Pdu pdu = this.CreatePduGetBulk(nonRepeaters, maxRepetitions, oids);
//            return this.RequestAsync(pdu, userToken, responseCallback);
//        }

//        public bool Set(string oid, object value)
//        {
//			if (this.udpTarget == null)
//				return false; // new RequestResult<bool>(false, RequestActionResult.Error, "UDP target is null");

//			bool succeeded = false;
//			Pdu pdu = this.CreatePduSet(oid, value);
//			SnmpPacket request = this.udpTarget.Request(pdu, this.agentParameters);

//			// If result is null then agent didn't reply or we couldn't parse the reply.
//			if (request != null)
//            {
//				SnmpData[] resultList = this.ProcessRecievedSnmpPacket(request, this.udpTarget.Address.ToString());

//				if (request.Pdu.ErrorStatus == 0)
//                    succeeded = true;

//                this.lastOid = oid;
//            }

//            return succeeded;
//        }

//        public Task<bool> SetAsync(string oid, object value)
//        {
//            Pdu pdu = this.CreatePduSet(oid, value);
//            object userToken = new object();
            
//            return new Task<bool>(() => this.RequestAsync(pdu, userToken));
//        }

//        public bool SetAsync(string oid, object value, object userToken, SnmpResponseEventHandler responseCallback)
//        {
//            Pdu pdu = this.CreatePduSet(oid, value);
//            return this.RequestAsync(pdu, userToken, responseCallback);
//        }

//        public bool RequestAsync(Pdu pdu, object userToken)
//        {
//            bool requestSucceeded = false;

//            if (this.udpTarget != null)
//            {
//                requestSucceeded = this.udpTarget.RequestAsync(pdu, this.agentParameters, this.SnmpAsyncResponse);
                
//                if (requestSucceeded)
//                    this.userTokensByRequestId[pdu.RequestId] = userToken;
//            }
//            else
//            {
//                Thread onResponseThread = new Thread(() => this.OnResponse((SnmpRequestInfo)pdu.ErrorStatus, new SnmpResponseEventArgs(pdu.RequestId, -1, -1, "Hostname or IP Address Error", null, userToken)));
//                onResponseThread.IsBackground = true;
//                onResponseThread.Priority = ThreadPriority.BelowNormal;
//                onResponseThread.Start();
//            }

//            return requestSucceeded;
//        }

//        public bool RequestAsync(Pdu pdu, object userToken, SnmpResponseEventHandler responseCallback)
//        {
//            this.responseCallback = null;
//            this.responseCallback += responseCallback;

//            bool requestSucceeded = false;

//            if (this.udpTarget != null)
//            {
//                requestSucceeded = this.udpTarget.RequestAsync(pdu, this.agentParameters, new SnmpAsyncResponse(this.SendToCallback));
//                this.userTokensByRequestId[pdu.RequestId] = userToken;
//            }
//            else
//            {
//                this.userTokensByRequestId[pdu.RequestId] = userToken;

//                Thread sendToCallbackThread = new Thread(() => this.SendToCallback(AsyncRequestResult.SocketSendError, null));
//                sendToCallbackThread.IsBackground = true;
//                sendToCallbackThread.Priority = ThreadPriority.BelowNormal;
//                sendToCallbackThread.Start();
//            }

//            return requestSucceeded;
//        }

//        public void Discover()
//        {
//            if (this.udpTarget == null)
//                throw new Exception("Hostname or IP Address Error");
            
//            SecureAgentParameters param = new SecureAgentParameters();

//            if (!this.udpTarget.Discovery(param))
//            {
//				this.udpTarget.Close();
//                throw new Exception("Discovery failed. Unable to continue...");
//            }
//        }

//        public void Dispose()
//        {
//			this.userTokensByRequestId.Clear();
//			this.userTokensByRequestId = null;
//			this.agent = null;
//			this.nonSecureAgentParameters = null;
//			this.secureAgentParameters = null;
//		}

//#endregion |   Public Methods   |

//        #region |   Private Methods   |

//        private void SetUdpTarget()
//        {
//			try
//            {
//				IPAddress ipAddress = DnsHelper.ResolveIPAddressFromHostname(this.RemoteHost);

//				if (ipAddress != null)
//				{
//					this.agent.Set(ipAddress);
//				}
//				else
//				{
//					this.agent.Set(this.RemoteHost); // Set an IPv4 address
//				}

//				this.udpTarget = new UdpTarget((IPAddress)this.agent, this.RemotePort, this.Timeout * 1000, this.NumberOfRetries);
//            }
//            catch
//            {
//				this.udpTarget = null;
//            }
//        }

//        private Pdu CreatePduGet(string oid)
//        {
//            Pdu pdu = new Pdu();

//            pdu.Type = PduType.Get;
//            pdu.VbList.Add(oid);

//            return pdu;
//        }

//        private Pdu CreatePduGetNext(string oid)
//        {
//            Pdu pdu = new Pdu();

//            pdu.Type = PduType.GetNext;
//            pdu.VbList.Add(oid);

//            // When Pdu class is first constructed, RequestId is set to a random value
//            // that needs to be incremented on subsequent requests made using the
//            // same instance of the Pdu class.
//            if (pdu.RequestId != 0)
//            {
//                pdu.RequestId += 1;
//            }

//            // Initialize request PDU with the last retrieved Oid
//            pdu.VbList.Add(oid);

//            return pdu;
//        }

//        private Pdu CreatePduGetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
//        {
//            Pdu pdu = new Pdu();

//            pdu.Type = PduType.GetBulk;
//            pdu.NonRepeaters = nonRepeaters;
//            pdu.MaxRepetitions = maxRepetitions;

//            // When Pdu class is first constructed, RequestId is set to a random value
//            // that needs to be incremented on subsequent requests made using the
//            // same instance of the Pdu class.
//            if (pdu.RequestId != 0)
//            {
//                pdu.RequestId += 1;
//            }

//            // Initialize request PDU with the last retrieved Oid
//            foreach (string oid in oids)
//            {
//                pdu.VbList.Add(oid);
//            }

//            return pdu;
//        }

//        private Pdu CreatePduSet(string oid, object value)
//        {
//			SnmpObjectValueType snmpObjectType;

//			if (value == null)
//			{
//				snmpObjectType = SnmpObjectValueType.OctetString;
//			}
//			else if (value.GetType() == typeof(int))
//			{
//				snmpObjectType = SnmpObjectValueType.Integer32;
//			}
//			else
//			{
//				snmpObjectType = SnmpObjectValueType.OctetString;
//			}

//			Pdu pdu = new Pdu();
//            string valueString = (value != null) ? value.ToString() : String.Empty;
//            AsnType asnType = valueString.ToAsnType(snmpObjectType);

//            pdu.Type = PduType.Set;
//            pdu.VbList.Add(new Oid(oid), asnType);

//            return pdu;
//        }

//        private SnmpData[] ProcessRecievedSnmpPacket(SnmpPacket snmpPacket, string targetAddress)
//        {
//            SnmpData[] snmpDataList = null;

//            // If result is null then agent didn't reply or we couldn't parse the reply.
//            if (snmpPacket != null)
//            {
//                snmpDataList = snmpPacket.Pdu.VbList.ToSnmpDataList();

//                // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
//                if (snmpPacket.Pdu.ErrorStatus != 0) // && snmpPacket.Pdu.ErrorStatus != 2)
//                {
//                    this.RaiseOnResponse((SnmpRequestInfo)snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.RequestId, snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.ErrorIndex, "Error in SNMP reply.", snmpDataList);
//                    this.ResetTarget();
                    
//                    string oid = (snmpPacket.Pdu.VbCount > 0) ? snmpPacket.Pdu[0].Oid.ToString() : String.Empty;

//                    throw new Exception(String.Format("Error in SNMP reply. Error: {0}  Index: {1}  OID: {2}", snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.ErrorIndex, oid));
//                }
//                else
//                {
//                    // agent reported an error with the request
//                    this.RaiseOnResponse((SnmpRequestInfo)snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.RequestId, snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.ErrorIndex, String.Empty, snmpDataList);
//                }
//            }
//            else
//            {
//                string errorDescription = "No response received from SNMP station" + targetAddress;

//                this.ResetTarget();
//                this.RaiseOnResponse((SnmpRequestInfo)snmpPacket.Pdu.ErrorStatus, - 1, -1, -1, errorDescription, null);

//                throw new Exception(String.Format(errorDescription));
//            }

//            return snmpDataList;
//        }


//        private void ResetTarget()
//        {
//            this.lastOid = null;
//        }

//        private void SnmpAsyncResponse(AsyncRequestResult result, SnmpPacket snmpPacket)
//        {
//            SnmpData[] snmpDataList = (result == AsyncRequestResult.NoError && snmpPacket != null) ? snmpPacket.Pdu.VbList.ToSnmpDataList() : new SnmpData[0];
//            this.RaiseOnResponse((SnmpRequestInfo)result, snmpPacket.Pdu.RequestId, snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.ErrorIndex, String.Empty, snmpDataList);
//        }

//        private void SendToCallback(AsyncRequestResult result, SnmpPacket snmpPacket)
//        {
//            object userToken = null;
//			int errorStatus = -1;
//			int errorIndex = -1;
//			int requestId = -1;

//			SnmpData[] snmpDataList = (result == AsyncRequestResult.NoError && snmpPacket != null) ? snmpPacket.Pdu.VbList.ToSnmpDataList() : new SnmpData[0];
//            //this.RaiseOnResponse(snmpPacket.Pdu.RequestId, snmpPacket.Pdu.ErrorStatus, snmpPacket.Pdu.ErrorIndex, String.Empty, snmpDataList);

//			if (snmpPacket != null && snmpPacket.Pdu != null)
//			{
//				userToken = this.userTokensByRequestId[snmpPacket.Pdu.RequestId];
//				this.userTokensByRequestId.Remove(snmpPacket.Pdu.RequestId);

//				errorStatus = snmpPacket.Pdu.ErrorStatus;
//				errorIndex = snmpPacket.Pdu.ErrorIndex;
//				requestId = -1;
//			}

//            this.responseCallback((SnmpRequestInfo)result, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, String.Empty, snmpDataList, userToken));
//        }

//        #endregion |   Private Methods   |

//        #region |   Raise Events   |

//        private void RaiseOnResponse(SnmpRequestInfo requestResult, int requestId, int errorStatus, int errorIndex, string errorDescription, SnmpData[] values)
//        {
//            if (this.OnResponse != null)
//            {
//                object userToken = this.userTokensByRequestId[requestId];

//                this.OnResponse(requestResult, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, errorDescription, values, userToken));
//            }

//            this.userTokensByRequestId.Remove(requestId);
//        }

//        #endregion |   Raise Events   |

//    }

//    #region |   Helper Classes   |

//	public static class SnmpSharpNetExtensions
//	{
//		public static SnmpData[] ToSnmpDataList(this VbCollection value)
//		{
//			SnmpData[] result = new SnmpData[value.Count];

//			for (int i = 0; i < value.Count; i++)
//			{
//				Vb vb = value[i];
//				result[i] = vb.ToSnmpData();
//			}

//			return result;
//		}

//		private static SnmpData ToSnmpData(this Vb value)
//		{
//			return new SnmpData(value.Oid.ToString(), (SnmpObjectValueType)Enum.ToObject(typeof(SnmpObjectValueType), value.Value.Type), value.Value.ToString());
//		}

//		public static AsnType ToAsnType(this object value, SnmpObjectValueType snmpObjectType)
//		{
//			AsnType result = null;

//			switch (snmpObjectType)
//			{
//				case SnmpObjectValueType.Integer32:
//					result = new Integer32(Conversion.TryChangeType<int>(value));
//					break;

//				case SnmpObjectValueType.OctetString:

//					//if (value == null || value.ToString().IsNullOrEmpty())
//					//{
//					//    result = new OctetString(null as string);
//					//}
//					//else
//					//{
//					var typedValued = Conversion.TryChangeType<string>(value);
//					result = new OctetString(typedValued);
//					//}

//					break;

//				case SnmpObjectValueType.Null:
//					result = new Null();
//					break;

//				case SnmpObjectValueType.ObjectIdentifier:
//					result = new Oid(Conversion.TryChangeType<string>(value));
//					break;

//				case SnmpObjectValueType.IPAddress:
//					result = new IpAddress(Conversion.TryChangeType<string>(value));
//					break;

//				case SnmpObjectValueType.Counter32:
//					result = new Counter32(Conversion.TryChangeType<uint>(value));
//					break;

//				case SnmpObjectValueType.Gauge32:
//					result = new Gauge32(Conversion.TryChangeType<uint>(value));
//					break;

//				case SnmpObjectValueType.TimeTicks:
//					result = new TimeTicks(Conversion.TryChangeType<string>(value));
//					break;

//				case SnmpObjectValueType.Opaque:
//					result = new Opaque(Conversion.TryChangeType<string>(value));
//					break;

//				case SnmpObjectValueType.NetAddress:
//					throw new ArgumentOutOfRangeException("Method ObjectToAsnType: Unsuported SnmpObject Type " + snmpObjectType.ToString());

//				case SnmpObjectValueType.Counter64:

//					if (value is ulong)
//					{
//						result = new Counter64(Conversion.TryChangeType<ulong>(value));
//					}
//					else
//					{
//						result = new Counter64(Conversion.TryChangeType<long>(value));
//					}

//					break;

//				case SnmpObjectValueType.UnsignedInteger32:
//					result = new UInteger32(Conversion.TryChangeType<uint>(value));

//					break;
//				case SnmpObjectValueType.NoSuchObject:
//					result = new NoSuchObject();

//					break;
//				case SnmpObjectValueType.NoSuchInstance:
//					result = new NoSuchInstance();

//					break;
//				case SnmpObjectValueType.EndOfMibView:
//					result = new EndOfMibView();

//					break;

//				default: throw new ArgumentOutOfRangeException("Method ObjectToAsnType: Unsuported SnmpObject Type " + snmpObjectType.ToString());
//			}

//			return result;
//		}
//	}

//    #endregion |   Helper Classes   |
//}
