//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using nsoftware.IPWorksSSNMP;
//using NET.Tools.Providers;

//namespace NET.Tools.Snmp
//{
//    /// <summary>
//    /// IPWorks SSNMP v6 version of the ISnmpManagerControl implementation.
//    /// </summary>
//    public class SnmpManagerIPWorks : ISnmpManagerControl, IDisposable
//    {
//        #region |   Private Members   |

//        private Snmpmgr snmpManager = new Snmpmgr();
//        private SnmpVersion snmpManagerSNMPVersion = SnmpVersion.V2;
//        private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.MD5;
//        private SnmpEncryptionProtocol encryptionProtocol = SnmpEncryptionProtocol.AES256;

//        //DataExchangeThreadSync<List<SnmpData>> threadSync = new DataExchangeThreadSync<List<SnmpData>>();

//        #endregion

//        #region |   Constructors and Initialization   |

//        public SnmpManagerIPWorks()
//        {
//            //this.snmpManager.Timeout = 0;   //No wait for event response - go and get result value(s) via event
//            this.snmpManager.OnResponse += new Snmpmgr.OnResponseHandler(snmpManager_OnResponse);
//        }

//        #endregion

//        #region |   Public Events   |

//        public event SnmpResponseEventHandler OnResponse;

//        #endregion

//        #region |   Public Properties   |

//        public SnmpVersion SnmpVersion
//        {
//            get { return this.snmpManagerSNMPVersion; }
//            set
//            {
//                this.snmpManagerSNMPVersion = value;

//                switch (value)
//                {
//                    case SnmpVersion.V1:
//                        this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV1;
//                        break;
//                    case SnmpVersion.V2:
//                        this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
//                        break;
//                    case SnmpVersion.V3:
//                        this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV3;
//                        break;
//                }
//            }
//        }

//        public string CommunityString
//        {
//            get { return this.snmpManager.Community; }
//            set { this.snmpManager.Community = value; }
//        }

//        public string RemoteHost
//        {
//            get { return this.snmpManager.RemoteHost; }
//            set { this.snmpManager.RemoteHost = value; }
//        }

//        public int RemotePort
//        {
//            get { return this.snmpManager.RemotePort; }
//            set { this.snmpManager.RemotePort = value; }
//        }

//        public int Timeout
//        {
//            get { return this.snmpManager.Timeout; }
//            set { this.snmpManager.Timeout = value; }
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
//                        this.snmpManager.AuthenticationProtocol = SnmpmgrAuthenticationProtocols.authpHMACMD596;
//                        break;
//                    case SnmpAuthenticationProtocol.SHA1:
//                        this.snmpManager.AuthenticationProtocol = SnmpmgrAuthenticationProtocols.authpHMACSHA96;
//                        break;
//                }
//            }
//        }

//        public string AuthenticationPassword
//        {
//            get { return this.snmpManager.AuthenticationPassword; }
//            set { this.snmpManager.AuthenticationPassword = value; }
//        }

//        public string Username
//        {
//            get { return this.snmpManager.User; }
//            set { this.snmpManager.User = value; }
//        }

//        public string Password
//        {
//            get { return this.snmpManager.EncryptionPassword; }
//            set { this.snmpManager.EncryptionPassword = value; }
//        }

//        public SnmpEncryptionProtocol EncryptionProtocol
//        {
//            get { return this.encryptionProtocol; }
//            set
//            {
//                this.encryptionProtocol = value;

//                // Not supported in this ver.6 - (supported in ver. 8 but only for DES, 3DES and AES (128 bit)
//                switch (value)
//                {
//                    case SnmpEncryptionProtocol.None:
//                        break;
//                    case SnmpEncryptionProtocol.DES:
//                        break;
//                    case SnmpEncryptionProtocol.TripleDes:
//                        break;
//                    case SnmpEncryptionProtocol.AES128:
//                        break;
//                    case SnmpEncryptionProtocol.AES192:
//                        break;
//                    case SnmpEncryptionProtocol.AES256:
//                        break;
//                }
//            }
//        }

//        public string EncryptionPassword
//        {
//            get { return this.snmpManager.EncryptionPassword; }
//            set { this.snmpManager.EncryptionPassword = value; }
//        }

//        public int RemoteEngineBoots
//        {
//            get { return this.snmpManager.RemoteEngineBoots; }
//            set { this.snmpManager.RemoteEngineBoots = value; }
//        }

//        public int RemoteEngineTime
//        {
//            get { return this.snmpManager.RemoteEngineTime; }
//            set { this.snmpManager.RemoteEngineTime = value; }
//        }

//        #endregion

//        #region |   Public Methods   |

//        public SnmpData Get(string oid)
//        {
//            this.snmpManager.ObjCount = 1;
//            this.snmpManager.ObjId[1] = oid;
//            this.snmpManager.SendGetRequest();
//            SnmpData snmpData = this.CreateSingleSnmpData();
//            return snmpData;
//        }

//        public SnmpData GetNext(string oid)
//        {
//            this.snmpManager.ObjCount = 1;
//            this.snmpManager.ObjId[1] = oid;
//            this.snmpManager.SendGetNextRequest();
//            SnmpData snmpData = this.CreateSingleSnmpData();
//            return snmpData;
//        }

//        public SnmpData GetNext()
//        {
//            this.snmpManager.ObjCount = 1;
//            this.snmpManager.SendGetNextRequest();
//            SnmpData snmpData = this.CreateSingleSnmpData();
//            return snmpData;
//        }

//        //public int Get(params string[] oids)
//        //{
//        //    this.SetOIDs(oids);
//        //    int requestId = this.snmpManager.RequestId;
//        //    this.snmpManager.SendGetRequest();
//        //    return requestId;
//        //}

//        public int GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
//        {
//            this.SetOIDs(oids);
//            int requestId = this.snmpManager.RequestId;
//            this.snmpManager.SendGetBulkRequest(nonRepeaters, maxRepetitions);
//            return requestId;
//        }

//        public bool Set(string oid, object value, SnmpObjectValueType snmpObjectType)
//        {
//            this.snmpManager.ObjCount = 1;
//            this.snmpManager.ObjId[1] = oid;
//            this.snmpManager.ObjValue[1] = value != null ? value.ToString() : String.Empty;
//            this.snmpManager.ObjType[1] = (SnmpmgrObjTypes)Enum.ToObject(typeof(SnmpmgrObjTypes), System.Convert.ToInt32(snmpObjectType));
//            int requestId = this.snmpManager.RequestId;
//            this.snmpManager.SendSetRequest();

//            // TODO: Verify if SendSetRequest succeeded.
//            return true;
//        }

//        public void Discover()
//        {
//            this.snmpManager.Discover();
//        }

//        public void Dispose()
//        {
//            this.snmpManager.Dispose();
//        }

//        #endregion |   Public Methods   |

//        #region |   Private Methods   |

//        private void SetOIDs(string[] oids)
//        {
//            this.snmpManager.ObjCount = oids.Length;

//            for (int i = 0; i < oids.Length; i++)
//            {
//                this.snmpManager.ObjId[i + 1] = oids[i];
//            }
//        }

//        private SnmpData CreateSingleSnmpData()
//        {
//            return this.CreateSingleSnmpData(1);
//        }

//        private SnmpData CreateSingleSnmpData(int index)
//        {
//            string oid = this.snmpManager.ObjId[index];
//            SnmpObjectValueType objectType = (SnmpObjectValueType)Enum.ToObject(typeof(SnmpObjectValueType), Convert.ToInt32(this.snmpManager.ObjType[index]));
//            string value = this.snmpManager.ObjValue[index];

//            SnmpData snmpData = new SnmpData(oid, objectType, value);

//            if (objectType == SnmpObjectValueType.NoSuchObject || objectType == SnmpObjectValueType.NoSuchInstance)
//            {
//                throw new Exception(String.Format("Error in SNMP Reply. OID: {0}  Value Type: {1}", oid, objectType.ToString()));
//            }

//            return snmpData;
//        }

//        private void snmpManager_OnResponse(object sender, SnmpmgrResponseEventArgs e)
//        {
//            List<SnmpData> values = new List<SnmpData>();

//            for (int i = 1; i <= this.snmpManager.ObjCount; i++)
//            {
//                SnmpData snmpData = this.CreateSingleSnmpData(i);
//                values.Add(snmpData);
//            }

//            this.RaiseOnResponse(e.RequestId, e.ErrorStatus, e.ErrorIndex, e.ErrorDescription, values);
//        }

//        #endregion |   Private Methods   |

//        #region |   Raise Events   |

//        private void RaiseOnResponse(int requestId, int errorStatus, int errorIndex, string errorDescription, List<SnmpData> values)
//        {
//            if (this.OnResponse != null)
//            {
//                this.OnResponse(this, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, errorDescription, values));
//            }
//        }

//        #endregion |   Raise Events   |
//    }

//    ///// <summary>
//    ///// IPWorks SSNMP v8 version 
//    ///// </summary>
//    //public class SnmpControlIPWorks : IDisposable
//    //{
//    //    #region |   Private Members   |

//    //    private Snmpmgr snmpManager = new Snmpmgr();
//    //    private SnmpVersion snmpManagerSNMPVersion = SnmpVersion.V2;
//    //    private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.HMAC_MD5_96;

//    //    //DataExchangeThreadSync<List<SnmpData>> threadSync = new DataExchangeThreadSync<List<SnmpData>>();

//    //    #endregion

//    //    #region |   Constructors and Initialization   |

//    //    public SnmpControlIPWorks()
//    //    {
//    //        //this.snmpManager.Timeout = 0;   //No wait for event response - go and get result value(s) via event
//    //        this.snmpManager.OnResponse += new Snmpmgr.OnResponseHandler(snmpManager_OnResponse);
//    //    }

//    //    #endregion

//    //    #region |   Public Events   |

//    //    public event SnmpResponseEventHandler OnResponse;

//    //    #endregion

//    //    #region |   Public Properties   |

//    //    public SnmpVersion SnmpVersion
//    //    {
//    //        get
//    //        {
//    //            return this.snmpManagerSNMPVersion;
//    //        }
//    //        set
//    //        {
//    //            this.snmpManagerSNMPVersion = value;
//    //            switch (value)
//    //            {
//    //                case SnmpVersion.V1:
//    //                    this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV1;
//    //                    break;
//    //                case SnmpVersion.V2:
//    //                    this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
//    //                    break;
//    //                case SnmpVersion.V3:
//    //                    this.snmpManager.SNMPVersion = SnmpmgrSNMPVersions.snmpverV3;
//    //                    break;
//    //            }
//    //        }
//    //    }

//    //    public string CommunityString
//    //    {
//    //        get { return this.snmpManager.Community; }
//    //        set { this.snmpManager.Community = value; }
//    //    }



//    //    public string RemoteHost
//    //    {
//    //        get { return this.snmpManager.RemoteHost; }
//    //        set { this.snmpManager.RemoteHost = value; }
//    //    }

//    //    public int RemotePort
//    //    {
//    //        get { return this.snmpManager.RemotePort; }
//    //        set { this.snmpManager.RemotePort = value; }
//    //    }

//    //    public int Timeout
//    //    {
//    //        get { return this.snmpManager.Timeout; }
//    //        set { this.snmpManager.Timeout = value; }
//    //    }

//    //    public SnmpAuthenticationProtocol AuthenticationProtocol
//    //    {
//    //        get
//    //        {
//    //            return this.authenticationProtocol;
//    //        }
//    //        set
//    //        {
//    //            this.authenticationProtocol = value;
//    //            switch (value)
//    //            {
//    //                case SnmpAuthenticationProtocol.HMAC_MD5_96:
//    //                    this.snmpManager.AuthenticationProtocol = SnmpmgrAuthenticationProtocols.authpHMACMD596;
//    //                    break;
//    //                case SnmpAuthenticationProtocol.HMAC_SHA_96:
//    //                    this.snmpManager.AuthenticationProtocol = SnmpmgrAuthenticationProtocols.authpHMACSHA96;
//    //                    break;
//    //            }
//    //        }
//    //    }

//    //    public string AuthenticationPassword
//    //    {
//    //        get { return this.snmpManager.AuthenticationPassword; }
//    //        set { this.snmpManager.AuthenticationPassword = value; }
//    //    }

//    //    public string EncryptionPassword
//    //    {
//    //        get { return this.snmpManager.EncryptionPassword; }
//    //        set { this.snmpManager.EncryptionPassword = value; }
//    //    }

//    //    public int RemoteEngineBoots
//    //    {
//    //        get { return this.snmpManager.RemoteEngineBoots; }
//    //        set { this.snmpManager.RemoteEngineBoots = value; }
//    //    }

//    //    public int RemoteEngineTime
//    //    {
//    //        get { return this.snmpManager.RemoteEngineTime; }
//    //        set { this.snmpManager.RemoteEngineTime = value; }
//    //    }

//    //    #endregion

//    //    #region |   Public Methods   |

//    //    public SnmpData SendGetRequest(string oid)
//    //    {
//    //        this.snmpManager.Objects.Clear();
//    //        this.snmpManager.Objects.Add(new SNMPObject(oid, null, SNMPObjectTypes.otOctetString));

//    //        try
//    //        {
//    //            this.snmpManager.SendGetRequest();
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            throw new SystemException(ex.Message, ex);
//    //        }

//    //        return this.CreateSingleSnmpData();
//    //    }

//    //    public SnmpData SendGetNextRequest(string oid)
//    //    {
//    //        this.snmpManager.Objects.Clear();
//    //        this.snmpManager.Objects.Add(new SNMPObject(oid, null, SNMPObjectTypes.otOctetString));

//    //        try
//    //        {
//    //            this.snmpManager.SendGetNextRequest();
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            throw new SystemException(ex.Message, ex);
//    //        }

//    //        return this.CreateSingleSnmpData();
//    //    }

//    //    public SnmpData SendGetNextRequest()
//    //    {
//    //        this.snmpManager.SendGetNextRequest();
//    //        return this.CreateSingleSnmpData();
//    //    }

//    //    public int SendGetRequest(params string[] oids)
//    //    {
//    //        this.SetOIDs(oids);
//    //        int requestId = this.snmpManager.RequestId;
//    //        this.snmpManager.SendGetRequest();
//    //        return requestId;
//    //    }

//    //    public int SendGetBulkRequest(int nonRepeaters, int maxRepetitions, params string[] oids)
//    //    {
//    //        this.SetOIDs(oids);
//    //        int requestId = this.snmpManager.RequestId;
//    //        this.snmpManager.SendGetBulkRequest(nonRepeaters, maxRepetitions);
//    //        return requestId;
//    //    }

//    //    public void SendSetRequest(string oid, object value, SnmpObjectType snmpObjectType)
//    //    {
//    //        this.snmpManager.Objects.Clear();
//    //        SNMPObject snmpObject = new SNMPObject(oid);
//    //        snmpObject.Value = value.ToString();
//    //        snmpObject.ObjectType = (SNMPObjectTypes)Enum.ToObject(typeof(SNMPObjectTypes), System.Convert.ToInt32(snmpObjectType));
//    //        this.snmpManager.Objects.Add(snmpObject);
//    //        int requestId = this.snmpManager.RequestId;
//    //        this.snmpManager.SendSetRequest();
//    //    }

//    //    public void Dispose()
//    //    {
//    //        this.snmpManager.Dispose();
//    //    }

//    //    #endregion |   Public Methods   |

//    //    #region |   Private Members   |

//    //    private void SetOIDs(string[] oids)
//    //    {
//    //        this.snmpManager.Objects.Clear();

//    //        foreach (string oid in oids)
//    //        {
//    //            this.snmpManager.Objects.Add(new SNMPObject(oid));
//    //        }
//    //    }

//    //    private SnmpData CreateSingleSnmpData()
//    //    {
//    //        return this.CreateSingleSnmpData(0);
//    //    }

//    //    private SnmpData CreateSingleSnmpData(int index)
//    //    {
//    //        SNMPObject snmpObject = this.snmpManager.Objects[index];
//    //        SnmpObjectType objectType = (SnmpObjectType)Enum.ToObject(typeof(SnmpObjectType), Convert.ToInt32(snmpObject.ObjectType));
//    //        SnmpData snmpData = new SnmpData(snmpObject.Oid, objectType, snmpObject.Value);

//    //        return snmpData;
//    //    }

//    //    private void snmpManager_OnResponse(object sender, SnmpmgrResponseEventArgs e)
//    //    {
//    //        List<SnmpData> values = new List<SnmpData>();

//    //        for (int i = 1; i <= this.snmpManager.Objects.Count; i++)
//    //        {
//    //            SnmpData snmpData = this.CreateSingleSnmpData(i);
//    //            values.Add(snmpData);
//    //        }

//    //        this.RaiseOnResponse(e.RequestId, e.ErrorStatus, e.ErrorIndex, e.ErrorDescription, values);
//    //    }

//    //    #endregion |   Private Members   |

//    //    #region |   Raise Events   |

//    //    private void RaiseOnResponse(int requestId, int errorStatus, int errorIndex, string errorDescription, List<SnmpData> values)
//    //    {
//    //        if (this.OnResponse != null)
//    //        {
//    //            this.OnResponse(this, new SnmpResponseEventArgs(requestId, errorStatus, errorIndex, errorDescription, values));
//    //        }
//    //    }

//    //    #endregion |   Raise Events   |
//    //}
//}
