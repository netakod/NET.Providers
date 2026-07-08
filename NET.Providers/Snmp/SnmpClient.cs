using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib.Messaging;
using Simple;
using Simple.Threading;
using Simple.Network;
//using SnmpSharpNet;


namespace NET.Tools.Snmp
{
	// TODO: Dodati nonRepiters, Repetition like properties (connection string) !!!!
	public class SnmpClient : TaskRequestActionBase, IProviderConnection, IDisposable
    {
		#region |   Private Members   |

		private SnmpProviderType providerType = SnmpProviderType.SharpSnmpLib;
		private string remoteHost = String.Empty;
		private int remotePort = 161;
		private SnmpVersion snmpVersion = SnmpVersion.V2;
		private string community = "public";
		private int timeout = 5; // in seconds
		private int numOfRetries = 4;
		private int maximumBulkRepetitions = 20;
		private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.MD5;
		private string authenticationPassword = String.Empty;
		private string encryptionPassword = String.Empty;
		private int remoteEngineBoots = 0;
		private int remoteEngineTime = 0;

		private object provider;
		//private bool isConnected = false;

		#endregion |   Private Members   |

		#region |   Public Static Members   |

		public static SnmpClient Default = new SnmpClient();

		#endregion |   Public Static Members   |

		#region |   Constructors and Initialization   |

		public SnmpClient()
        {
        }

        #endregion |   Constructors and Initialization   |

        #region |   Events   |

        public event SnmpDataEventHandler OnSendingRequest;
        //public event SnmpDataEventHandler ResponseReceived;
        public event SnmpResponseEventHandler OnResponse;

        #endregion

        #region |   Public Properties   |

        public SnmpProviderType ProviderType
        {
            get { return this.providerType; }
            set
            {
                this.providerType = value;
                this.SetProvider();
            }
        }

        public string RemoteHost 
        {
            get { return this.remoteHost; }
            set
            {
                this.remoteHost = value;
                this.Provider.RemoteHost = value;
            }
        }

        public int RemotePort
        {
            get { return this.remotePort; }
            set
            {
                this.remotePort = value;
                this.Provider.RemotePort = value;
            }
        }

        public SnmpVersion SnmpVersion
        {
            get { return this.snmpVersion; }
            set
            {
                this.snmpVersion = value;
                this.Provider.SnmpVersion = value;
            }
        }

        public string Community
        {
            get { return this.community; }
            set
            {
                this.community = value;
                this.Provider.Community = value;
            }
        }

        /// <summary>
        /// Timeout in seconds.
        /// </summary>
        public int Timeout
        {
            get { return this.timeout; }
            set
            {
                if (value > 0)
                {
                    this.timeout = value;
                    this.Provider.Timeout = value;
                }
            }
        }

        public int NumOfRetries
        {
            get { return this.numOfRetries; }
            set
            {
                if (value > 0)
                {
                    this.numOfRetries = value;
                    this.Provider.NumberOfRetries = value;
                }
            }
        }

        public SnmpAuthenticationProtocol AuthenticationProtocol
        {
            get { return this.authenticationProtocol; }
            set
            {
                this.authenticationProtocol = value;
                this.Provider.AuthenticationProtocol = value;
            }
        }

        public string AuthenticationPassword
        {
            get { return this.authenticationPassword; }
            set
            {
                this.authenticationPassword = value;
                this.Provider.Password = value;
            }
        }

        public string EncryptionPassword
        {
            get { return this.encryptionPassword; }
            set
            {
                this.encryptionPassword = value;
                this.Provider.EncryptionPassword = value;
            }
        }

        public int RemoteEngineBoots
        {
            get { return this.remoteEngineBoots; }
            set
            {
                this.remoteEngineBoots = value;
                this.Provider.RemoteEngineBoots = value;
            }
        }

        public int RemoteEngineTime
        {
            get { return this.remoteEngineTime; }
            set
            {
                this.remoteEngineTime = value;
                this.Provider.RemoteEngineTime = value;
            }
        }

		public int MaximumBulkRepetitions
		{
			get { return this.maximumBulkRepetitions; }
			set { this.maximumBulkRepetitions = (value > 0) ? value : 1; }
		}

		//public bool IsConnected
  //      {
  //          get { return this.isConnected; }
  //      }

		public object Owner { get; set; }

		#endregion |   Public Properties   |

		#region |   Protected Properties   |

		protected ISnmpClient Provider
        {
            get 
            {
                if (this.provider == null)
                    this.SetProvider();

                return this.provider as ISnmpClient; 
            }
        }

        #endregion |   Protected Properties   |

        #region |   Public Methods   |

        ///// <summary>
        ///// Setup the SNMP control using specified ConnectionPolicy to be able to send snmp requests.
        ///// </summary>
        //public IRequestResult Connect()
        //      {
        //          this.InitializeConnection();

        //	IRequestResult<SnmpData> result = this.SendRequest<SnmpData>(() => this.Get(String.Format("{0}.0", SnmpOIDs.System.sysName)));

        //	this.isConnected = result.Succeed && result.ResultValue.SnmpObjectValueType == SnmpObjectValueType.OctetString;

        //	return result;
        //      }

        //      public void Disconnect()
        //      {
        //          this.isConnected = false;
        //      }

        public async ValueTask<TaskInfo<string>> TestConnectionAsync(WorkerContext workerContext)
        {
            string location = null;
            string tempLocation = null;
            string result;
            TaskInfo<string> getDescriptionResult;
            TaskInfo<string> getLocationResult;
            TaskInfo<bool> setLocationResult;

            //int oldTimeout = this.Timeout;
            //this.Timeout *= 3;

            string testResultMessage = "SNMP Read Test: ";

            if (workerContext != null)
                workerContext.ReportProgress(-1, "Testing SNMP Read...");

            //if (!this.IsConnected)
            this.InitializeConnection();

			var descriptionSnmpData = await this.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysDescription));
			getDescriptionResult = await this.SendRequestAsync(async () => (await this.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysDescription)))
                                                                                      .ToString());
			result = getDescriptionResult.ResultValue;
			workerContext.Result = result;

			if (workerContext.ShouldCancel())
				return new TaskInfo<string>(result, TaskResultInfo.Cancelled, "Canncelled by the user");

			if (getDescriptionResult.Succeeded)
			{
				testResultMessage += "Success";
				workerContext.ReportProgress(-1, testResultMessage + "\r\nTesting SNMP Write...");
				testResultMessage += "\r\nSNMP Write Test: ";
				getLocationResult = await this.SendRequestAsync(async () => (await this.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation)))
                                                                                       .ToString());
				if (workerContext.ShouldCancel())
                    return new TaskInfo<string>(result, TaskResultInfo.Cancelled, "Canncelled by the user");

                if (getLocationResult.Succeeded)
				{
					location = getLocationResult.ResultValue;
					location = location ?? String.Empty;
					tempLocation = location + "X";

					// Set temporary location value to test set
					setLocationResult = await this.SendRequestAsync(async () => await this.SetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation), tempLocation));

					if (workerContext.ShouldCancel())
                        return new TaskInfo<string>(result, TaskResultInfo.Cancelled, "Canncelled by the user");

                    if (setLocationResult.Succeeded)
					{
						// Get temporary location value to see if is set correct
						getLocationResult = await this.SendRequestAsync(async () => (await this.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation)))
                                                                                               .ToString());
						if (workerContext.ShouldCancel())
                            return new TaskInfo<string>(result, TaskResultInfo.Cancelled, "Canncelled by the user");

                        testResultMessage += (getLocationResult.ResultValue == tempLocation) ? "Success" : "Failed";
						workerContext.ReportProgress(-1, testResultMessage);

						// Set the original location value
						setLocationResult = await this.SendRequestAsync(async () => await this.SetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation), location));

						if (workerContext.ShouldCancel())
                            return new TaskInfo<string>(result, TaskResultInfo.Cancelled, "Canncelled by the user");
                    }
                    else
					{
						workerContext.ReportProgress(-1, "SNMP write error: " + setLocationResult.Message);
						testResultMessage += setLocationResult.Message;
					}
				}
				else
				{
					workerContext.ReportProgress(-1, "SNMP write error: " + getLocationResult.Message);
					testResultMessage += getLocationResult.Message;
				}
			}
			else
			{
				workerContext.ReportProgress(-1, "SNMP read error: " + getDescriptionResult.Message);
				testResultMessage += getDescriptionResult.Message;
			}

			//this.Timeout = oldTimeout;

            return new TaskInfo<string>(result, getDescriptionResult.ResultInfo, testResultMessage);
        }

		//public int SendGetRequest(string objectId) // Async method
		//{
		//    return this.SendGetRequest(new string[] { objectId });
		//}

		//public int SendGetRequest(params string[] objectIds) // Async method
		//{
		//    return this.Snmp.SendGetRequest(objectIds);
		//}

		//public int SendGetBulkRequest(int nonRepeaters, int maxRepetitions, params string[] objectIds) 
		//{
		//    return this.Snmp.SendGetBulkRequest(nonRepeaters, maxRepetitions, objectIds);
		//}

		//public int SendSetRequest(string objectId, string value) // Async method without verification is oid realy set
		//{
		//    return this.Snmp.SendSetRequest(objectId, value);
		//}


		//public SnmpData Get(string objectId)   // Sync method
		//{
		//    return this.WaitForSnmpData(this.SendGetRequest(objectId))[0];
		//}

		//public List<SnmpData> Get(params string[] objectIds)   // Sync method
		//{
		//    return this.WaitForSnmpData(this.SendGetRequest(objectIds));
		//}

		//public List<SnmpData> GetBulk(int nonRepeaters, int maxRepetitions, string objectId)   // Sync method
		//{
		//    return this.WaitForSnmpData(this.SendGetBulkRequest(nonRepeaters, maxRepetitions, new string[] { objectId }));
		//}

		//public List<SnmpData> GetBulk(int nonRepeaters, int maxRepetitions, params string[] objectIds)   // Sync method
		//{
		//    return this.WaitForSnmpData(this.SendGetBulkRequest(nonRepeaters, maxRepetitions, objectIds));
		//}

		//public SnmpData Get(string oid)
		//      {
		//	this.RaiseOnSendingRequest(oid, null);

		//	return this.ProviderControl.Get(oid);
		//      }

		//TODO: Napraviti novo SnmpData + error code of the response
		public async ValueTask<SnmpData> GetAsync(string oid)
		{
			return await this.Provider.GetAsync(oid);
		}

		//public bool GetAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
		//      {
		//          return this.ProviderControl.GetAsync(oid, userToken, responseCallback);
		//      }

		public SnmpData GetNext()
		{
			this.RaiseOnSendingRequest("GetNext", null);

			return this.Provider.GetNext();
		}

		public async ValueTask<SnmpData> GetNextAsync()
        {
            this.RaiseOnSendingRequest("GetNext", null);

            return await this.Provider.GetNextAsync();
        }

		public SnmpData GetNext(string oid)
		{
			this.RaiseOnSendingRequest(String.Format("GetNext {0}", oid), null);

			return this.Provider.GetNext(oid);
		}

		public async ValueTask<SnmpData> GetNextAsync(string oid)
        {
            this.RaiseOnSendingRequest(String.Format("GetNext {0}", oid), null);

            return await this.Provider.GetNextAsync(oid);
        }

        //public bool GetNextAsync(object userToken)
        //{
        //    return this.ProviderControl.GetNextAsync(userToken);
        //}

        //public bool GetNextAsync(string oid, object userToken)
        //{
        //    return this.ProviderControl.GetNextAsync(oid, userToken);
        //}

        //public bool GetNextAsync(object userToken, SnmpResponseEventHandler responseCallback)
        //{
        //    return this.ProviderControl.GetNextAsync(userToken, responseCallback);
        //}

        //public bool GetNextAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback)
        //{
        //    return this.ProviderControl.GetNextAsync(oid, userToken, responseCallback);
        //}

        //     public SnmpData[] GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
        //     {
        //         this.RaiseOnSendingRequest("GetBulk", null);

        //return this.ProviderControl.GetBulk(nonRepeaters, maxRepetitions, oids);
        //     }

        public async ValueTask<SnmpData[]> GetBulkAsync(int nonRepeaters, int maxRepetitions, params string[] oids)
        {
            return await this.Provider.GetBulkAsync(nonRepeaters, maxRepetitions, oids);
        }

        //public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, params string[] oids)
        //{
        //    return this.ProviderControl.GetBulkAsync(nonRepeaters, maxRepetitions, userToken, oids);
        //}

        //public bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, SnmpResponseEventHandler responseCallback, params string[] oids)
        //{
        //    return this.ProviderControl.GetBulkAsync(nonRepeaters, maxRepetitions, userToken, responseCallback, oids);
        //}

		//public bool Set(string oid, object value)
		//{
		//	this.RaiseOnSendingRequest(String.Format("Set {0} {1}", oid, value), null);

		//	return this.ProviderControl.Set(oid, value);
		//}

		public async Task<bool> SetAsync(string oid, object value) => await this.Provider.SetAsync(oid, value);

		//public async ValueTask<bool> SetAsync(string oid, object value, SnmpObjectValueType snmpObjectType, object userToken, SnmpResponseEventHandler responseCallback)
		//{
		//	return await this.ProviderControl.SetAsync(oid, userToken, snmpObjectType, responseCallback);
		//}

		// Probat izbjeći GetTable jer je spora. Probati je zamijeniti sa Walk ako je moguće jer je mnogo brža i bazira se na GetBulk gdje je god moguće.
		private SnmpData[,] GetTable(string tableOid)
        {
            string request = String.Format("GetTable {0}", tableOid);
            this.RaiseOnSendingRequest(request, null);

            SnmpData[,] table;
            List<List<SnmpData>> columnTable = new List<List<SnmpData>>();
			SnmpData snmpData = this.GetNext(tableOid);

			//if (!snmpData.Succeed)
			//	return new SnmpData[0, 0];

			//SnmpData snmpData = snmpData.ResultValue;
			SnmpData oldSnmpData = snmpData;
            string itemOid = this.GetTableItemOid(tableOid, snmpData.OID);

            while (snmpData.OID.StartsWith(tableOid)) // && snmpData.OID.Length > tableOid.Length)
            {
                List<SnmpData> column = new List<SnmpData>();

                while (snmpData.OID.StartsWith(itemOid))
                {
                    column.Add(snmpData);
					snmpData = this.GetNext();

					//if (!snmpData.Succeed)
					//	break;

					//if (snmpData.OID == oldSnmpData.OID)
					//break;

					oldSnmpData = snmpData;
                }

                columnTable.Add(column);
                itemOid = this.GetTableItemOid(tableOid, snmpData.OID);

                //if (oldSnmpData.OID == snmpData.OID)
                //  break;
            }

            if (columnTable.Count == 0)
            {
                table = new SnmpData[0, 0];
            }
            else
            {
                table = new SnmpData[columnTable[0].Count, columnTable.Count];

                for (int j = 0; j < columnTable.Count; j++)
                    for (int i = 0; i < columnTable[j].Count; i++)
                        table[i, j] = columnTable[j][i];
            }

            return table;
        }
        //public SnmpData[,] GetTable(string tableOid) => this.GetTableAsync(tableOid).GetAwaiter().GetResult();

        public async ValueTask<SnmpData[,]> GetTableAsync(string tableOid)
        {
            SnmpData[,] table;
            int numOfRows = 0;
            int numOfColumns = 0;
            int snmpDataListIndex = 0;
            //string lastTableOidSegment = null;

            this.RaiseOnSendingRequest(String.Format("GetTable {0}", tableOid), null);

            IList<SnmpData> snmpDataList = await this.WalkAsync(tableOid);

            // Find row count first
            foreach (SnmpData snmpData in snmpDataList)
            {
                if (!snmpData.OID.StartsWith(tableOid + ".1.1"))
                    break;
                
                //string oidExtender = snmpData.OID.Substring(tableOid.Length + 1);
                //string[] tableOidSegments = oidExtender.Split('.');
                //string tableOidSegment = tableOidSegments[tableOidSegments.Length - 2];

                //if (lastTableOidSegment == null)
                //{
                //    lastTableOidSegment = tableOidSegment;
                //}
                //else if (tableOidSegment != lastTableOidSegment)
                //{
                //    break;
                //}

                numOfRows++;
           }

            if (numOfRows > 0)
                numOfColumns = snmpDataList.Count / numOfRows;

            table = new SnmpData[numOfRows, numOfColumns];

            for (int columnIndex = 0; columnIndex < numOfColumns; columnIndex++)
                for (int rowIndex = 0; rowIndex < numOfRows; rowIndex++)
                    table[rowIndex, columnIndex] = snmpDataList[snmpDataListIndex++];

            return table;
        }


		// TODO: Dodati nonRepiters, Repetition kao propertije (connection string)
		// TODO: Return type must be RequestResult<IList<SnmpData>>
		//public IList<SnmpData> Walk(string rootOid) => this.WalkAsync(rootOid).GetAwaiter().GetResult();

        public async ValueTask<IList<SnmpData>> WalkAsync(string rootOid)
        {
            //// function only works on SNMP version 1 and SNMP version 2 requests
            //if (version != SnmpVersion.Ver1 && version != SnmpVersion.Ver2)
            //{
            //    if (!_suppressExceptions)
            //    {
            //        throw new SnmpInvalidVersionException("SimpleSnmp support SNMP version 1 and 2 only.");
            //    }
            //    return null;
            //}

            if (rootOid == null || rootOid.Length < 2)
                throw new SnmpException(SnmpException.InvalidOid, "RootOid is not a valid Oid");

            List<SnmpData> resultList = new List<SnmpData>();
            string oid = rootOid;
            string request = "Walk";
            
            this.RaiseOnSendingRequest(request, null);

            while (oid != null && oid.StartsWith(rootOid))
            {
                List<SnmpData> segmentSnmpDataList = new List<SnmpData>();

                if (this.SnmpVersion == SnmpVersion.V1)
                {
                    SnmpData snmpData;

                    if (oid == rootOid)
                        snmpData = await this.GetNextAsync(oid);
                    else
                        snmpData = await this.GetNextAsync();

                    if (!SnmpData.Empty.Equals(snmpData))
                        segmentSnmpDataList.Add(snmpData);
                }
                else
                {
                    SnmpData[] result = await this.GetBulkAsync(0, this.MaximumBulkRepetitions, oid);

                    segmentSnmpDataList.AddRange(result);
                }

                //// check that we have a result
                //if (segmentSnmpDataList.Count == 0)
                //{
                //    // error of some sort happened. abort... and return empti list;
                //    return new List<SnmpData>();
                //}

                foreach (SnmpData snmpData in segmentSnmpDataList)
                {
                    if (snmpData.ObjectValueType == SnmpObjectValueType.EndOfMibView)
                        return resultList;

                    if (snmpData.OID != oid && snmpData.OID.StartsWith(rootOid))
                    {
                        resultList.Add(snmpData);
                        oid = snmpData.OID;
                    }
                    else
                    {
                        // it's faster to check if variable is null then checking lastOid.StartsWith(rootOid)
                        oid = null;
                        
                        break;
                    }
                }
            }

            return resultList;
        }


        //public void Get

        //public void Set(string oid, string value)   // Sync method
        //{
        //    do
        //    {
        //        this.WaitForSnmpData(this.SendSetRequest(oid, value));
        //    } 
        //    while (value != this.Get(oid).Value);
        //    // TODO: add a timer to return false if is not posible to set specified OID
        //}


        public void SetLogging(string logFileName)
        {
            // TODO:
        }

        public virtual ValueTask FinishUpdateAsync() => new ValueTask();

        public void Dispose()
        {
            this.DisposeControl();
        }

#endregion |   Public Methods   |

        #region |   Protected Methods   |

        //protected void OnSendingRequest(string request, object data)
        //{
        //}

        //protected void OnResponseRecieved(string request, object data)
        //{
        //}

        #endregion |   Protected Methods   |

        #region |   Private Raise Events Methods   |

        private void RaiseOnSendingRequest(string request, object data)
        {
            this.OnSendingRequest?.Invoke(this, new SnmpDataEventArgs(request, data));
        }

#endregion |   Private Raise Events Methods   |

        #region |   Private Methods   |

        private void SetProvider()
        {
            if (this.provider != null)
            {
                this.Provider.OnResponse -= this.Snmp_OnResponse;
                (this.Provider as IDisposable).Dispose();
            }

            switch (this.ProviderType)
            {
				case SnmpProviderType.SharpSnmpLib:

					this.provider = new SnmpClientSharpSnmpLib();
					break;

				//case SnmpProviderType.SnmpSharpNet:

				//	this.providerControl = new SnmpControlSnmpSharpNet();
    //                break;

                //case SnmpProviderType.IPWorks:
                //    this.providerControl = new SnmpManagerControlIPWorks();
                //    break;

                //case SnmpProviderType.PowerSNMP :
                //    this.providerControl = new SnmpManagerControlPowerSNMP();
                //    break;

                default: throw new ArgumentException("For the ProviderType " + this.ProviderType.ToString() + " provider class is not specified.");
            }

            if (this.Provider != null)
                this.Provider.OnResponse += this.Snmp_OnResponse;
        }

        private void SetProviderControlSettings()
        {
            this.Provider.RemoteHost = this.RemoteHost;
            this.Provider.RemotePort = this.RemotePort;
            this.Provider.SnmpVersion = this.SnmpVersion;
            this.Provider.Community = this.Community;
            this.Provider.Timeout = this.Timeout;
            this.Provider.NumberOfRetries = this.NumOfRetries;
            this.Provider.AuthenticationProtocol = this.AuthenticationProtocol;
            this.Provider.Password = this.AuthenticationPassword;
            this.Provider.EncryptionPassword = this.EncryptionPassword;
            this.Provider.RemoteEngineBoots = this.RemoteEngineBoots;
            this.Provider.RemoteEngineTime = this.RemoteEngineTime;
        }

        private string GetTableItemOid(string tableOid, string oid)
        {
            string[] tableOidArray = tableOid.Split(new string[] { "." }, StringSplitOptions.None);
            string[] itemOidArray = oid.Split(new string[] { "." }, tableOidArray.Length + 3, StringSplitOptions.None);
            return String.Join(".", itemOidArray, 0, itemOidArray.Length - 1);
        }

        //private List<SnmpData> WaitForSnmpData(int requestId)
        //{
        //    return this.threadSync.WaitFor(requestId, this.Timeout);
        //}

        private void InitializeConnection()
        {
            this.DisposeControl();

            this.SetProvider();
            this.SetProviderControlSettings();
        }

        //private void AddUserToken(int requestID, object userToken)
        //{
        //    lock (lockObject)
        //    {
        //        if (this.userTokensByRequestID.ContainsKey(requestID))
        //        {
        //            this.userTokensByRequestID[requestID] = userToken;
        //        }
        //        else
        //        {
        //            this.userTokensByRequestID.Add(requestID, userToken);
        //        }
        //    }
        //}

        //private object GetUserToken(int requestID)
        //{
        //    lock (lockObject)
        //    {
        //        if (this.userTokensByRequestID.ContainsKey(requestID))
        //        {
        //            return this.userTokensByRequestID[requestID];
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //private void RemoveUserToken(int requestID)
        //{
        //    lock (lockObject)
        //    {
        //        this.userTokensByRequestID.Remove(requestID);
        //    }
        //}

        private void DisposeControl()
        {
            if (this.provider != null)
            {
                (this.provider as IDisposable).Dispose();
                this.provider = null;
            }
        }

#endregion |   Private Methods   |

        #region |   Private Event Responses   |

        private void Snmp_OnResponse(SnmpRequestInfo snmpResult, SnmpResponseEventArgs e)
        {
            if (this.OnResponse != null)
                this.OnResponse(snmpResult, e);
        }

        #endregion |   Private Event Responses   |
    }

    #region |   Structs & Classes   |

    public struct SnmpSettings
    {
        public string Address;
        public int Port;
        public string Community;
        public int Timeout;
        public SnmpVersion Version;
        public SnmpSecuritySettings Security;

        public override string ToString() { return this.Address; }
    }

    /// <summary>
    /// This class controls Version 3 authentication and privacy procedures.
    /// </summary>
    /// <remarks>
    /// The Security class is used with Version 3 messages to provide message authentication and privacy. 
    /// The Security object is not used or added to a message unless NET.Tools.SnmpVersion is SnmpVersion.V3. 
    /// If a message to be sent is a Version 3 message, at a minimum you must set NET.Tools.SnmpSecuritySettings.Username
    /// to a valid user name for the Agent you are sending requests to.
    /// </remarks>
    public class SnmpSecuritySettings
    {
        public string AuthenticationPassword;
        public SnmpAuthenticationProtocol AuthenticationProtocol;
        //public byte[] ContextId;
        //public byte[] ContextName;
        public int EngineBoots;
        //public byte[] EngineIdB;
        public int EngineId;
        public int EngineTime;
        //public int Id;
        public string EncriptionPassword;
        public SnmpEncryptionProtocol EncriptionProtocol;
        public string Username;
    }

#endregion |   Structs & Classes   |

    #region |   Enums   |

    public enum SnmpProviderType
    {
		SharpSnmpLib = 0,
		//SnmpSharpNet = 1,
  //      IPWorks = 2,
  //      PowerSNMP = 3,
    }

    public enum SnmpVersion
    {
        V1 = 0,
        V2 = 1,
        V3 = 2
    }

    public enum SnmpAuthenticationProtocol
    {
        MD5 = 0,
        SHA1 = 1,
        SHA256 = 2,
        SHA384 = 3,
        SHA512 = 4,
      }

    /// <summary>
    /// Defines the algorithm to be used for Version 3 message encryption.
    /// </summary>
    public enum SnmpEncryptionProtocol
    {
        /// <summary>
        /// No encryption/decryption is desired.
        /// </summary>
        None = 0,
        /// <summary>
        /// Selects the DES algorithm to perform message encryption.
        /// </summary>
        DES = 1,
        /// <summary>
        /// Selects the Triple DES algorithm to perform message encryption. This is FIPS-140 compliant.
        /// </summary>
        TripleDes = 2,
        /// <summary>
        /// Selects the AES128 algorithm to perform message encryption.
        /// </summary>
        AES128 = 3,
        /// <summary>
        /// Selects the AES192 algorithm to perform message encryption.
        /// </summary>
        AES192 = 4,
        /// <summary>
        /// Selects the AES256 algorithm to perform message encryption.
        /// </summary>
        AES256 = 5
    }

    /// <summary>
    /// Result codes sent by UdpTarget class to the SnmpAsyncResponseCallback delegate.
    /// </summary>
    public enum SnmpRequestInfo
    {
        /// <summary>
        /// No error. Data was received from the socket.
        /// </summary>
        NoError = 0,
        /// <summary>
        /// Request is in progress. A new request can not be initiated until previous request completes.
        /// </summary>
        RequestInProgress,
        /// <summary>
        /// Request has timed out. Maximum number of retries has been reached without receiving a reply
        /// from the peer request was sent to
        /// </summary>
        Timeout,
        /// <summary>
        /// An error was encountered when attempting to send data to the peer. Request failed.
        /// </summary>
        SocketSendError,
        /// <summary>
        /// An error was encountered when attempting to receive data from the peer. Request failed.
        /// </summary>
        SocketReceiveError,
        /// <summary>
        /// Request has been terminated by the user.
        /// </summary>
        Terminated,
        /// <summary>
        /// No data was received from the peer
        /// </summary>
        NoDataReceived,
        /// <summary>
        /// Authentication error
        /// </summary>
        AuthenticationError,
        /// <summary>
        /// Privacy error
        /// </summary>
        PrivacyError,
        /// <summary>
        /// Error encoding SNMP packet
        /// </summary>
        EncodeError,
        /// <summary>
        /// Error decoding SNMP packet
        /// </summary>
        DecodeError
    }

#endregion |   Enums   |

    #region |   Delegates   |

    public delegate void SnmpResponseEventHandler(SnmpRequestInfo snmpResult, SnmpResponseEventArgs e);
    public delegate void SnmpDataEventHandler(object sender, SnmpDataEventArgs e);
    //public delegate void SnmpAsyncResponseCallback(SnmpRequestResult snmpResult, SnmpResponseEventArgs snmpResponse);

#endregion |   Delegates   |

    #region |   Event Args   |

    public class SnmpDataEventArgs : EventArgs
    {
        private string request = null;
        private object data = null;

        public SnmpDataEventArgs()
        {
        }

        public SnmpDataEventArgs(string request, object data)
        {
            this.request = request;
            this.data = data;
        }

        public string Request
        {
            get { return this.request; }
            set { this.request = value; }
        }

        public object Data
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }

    public class SnmpResponseEventArgs
    {
        public SnmpResponseEventArgs(int requestId, int errorStatus, int errorIndex, string errorDescription, SnmpData[] values, object userToken)
        {
            this.RequestId = requestId;
            this.ErrorStatus = errorStatus;
            this.ErrorIndex = errorIndex;
            this.ErrorDescription = errorDescription;
            this.Values = values;
            this.UserToken = userToken;
        }

        public int RequestId { get; private set; }
        public int ErrorStatus { get; private set; }
        public int ErrorIndex { get; private set; }
        public string ErrorDescription { get; private set; }
        public SnmpData[] Values { get; private set; }
        public object UserToken { get; set; }
    }

#endregion |   Event Args   |
}
