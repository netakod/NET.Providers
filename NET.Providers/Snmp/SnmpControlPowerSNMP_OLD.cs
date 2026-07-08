//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Dart.Snmp;
//using Dart.Common;

//namespace NET.Tools.Snmp
//{
//    /// <summary>
//    /// Dart PowerSNMP version of the ISnmpManagerControl implementation.
//    /// </summary>
//    public class SnmpManagerPowerSNMP : ISnmpManagerControl, IDisposable
//    {
//        #region |   Private Members   |

//        private Manager snmpManager = new Manager();
//        PowerSsnmpAgentSettings agentSettings = new PowerSsnmpAgentSettings();
//        private SnmpVersion snmpManagerSNMPVersion = SnmpVersion.V2;
//        private SnmpAuthenticationProtocol authenticationProtocol = SnmpAuthenticationProtocol.MD5;
//        private SnmpEncryptionProtocol encryptionProtocol = SnmpEncryptionProtocol.AES256;

//        #endregion

//        #region |   Public Events   |

//        public event SnmpResponseEventHandler OnResponse;

//        #endregion

//        #region |   Constructors and Initialization   |

//        public SnmpManagerPowerSNMP()
//        {
//            this.snmpManager.Message += new MessageEventHandler(snmpManager_Message);
//            this.snmpManager.Error += new ComponentErrorEventHandler(snmpManager_Error);
//            this.snmpManager.UserState += new UserStateEventHandler(snmpManager_UserState);
//            this.snmpManager.Table += new TableEventHandler(snmpManager_Table);
//        }

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
//                        this.agentSettings.Version = Dart.Snmp.SnmpVersion.One;
//                        break;
//                    case SnmpVersion.V2:
//                        this.agentSettings.Version = Dart.Snmp.SnmpVersion.Two;
//                        break;
//                    case SnmpVersion.V3:
//                        this.agentSettings.Version = Dart.Snmp.SnmpVersion.Three;
//                        break;
//                }
//            }
//        }

//        public string CommunityString
//        {
//            get { return this.agentSettings.Community; }
//            set { this.agentSettings.Community = value; }
//        }

//        public string RemoteHost
//        {
//            get { return this.agentSettings.Address; }
//            set { this.agentSettings.Address = value; }
//        }

//        public int RemotePort
//        {
//            get { return this.agentSettings.Port; }
//            set { this.agentSettings.Port = value; }
//        }

//        public int Timeout
//        {
//            get { return this.agentSettings.Timeout; }
//            set { this.agentSettings.Timeout = value; }
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
//                        this.agentSettings.Security.AuthenticationProtocol = Dart.Snmp.AuthenticationProtocol.Md5;
//                        break;
//                    case SnmpAuthenticationProtocol.SHA1:
//                        this.agentSettings.Security.AuthenticationProtocol = Dart.Snmp.AuthenticationProtocol.Sha;
//                        break;
//                }
//            }
//        }

//        public string AuthenticationPassword
//        {
//            get { return this.agentSettings.Security.AuthenticationPassword; }
//            set { this.agentSettings.Security.AuthenticationPassword = value; }
//        }

//        public string Username
//        {
//            get { return this.agentSettings.Security.Username; }
//            set { this.agentSettings.Security.Username = value; }
//        }

//        public string Password
//        {
//            get { return this.agentSettings.Security.AuthenticationPassword; }
//            set { this.agentSettings.Security.AuthenticationPassword = value; }
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
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.None;
//                        break;
//                    case SnmpEncryptionProtocol.DES:
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.Des;
//                        break;
//                    case SnmpEncryptionProtocol.TripleDes:
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.TripleDes;
//                        break;
//                    case SnmpEncryptionProtocol.AES128:
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.Aes128;
//                        break;
//                    case SnmpEncryptionProtocol.AES192:
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.Aes192;
//                        break;
//                    case SnmpEncryptionProtocol.AES256:
//                        this.agentSettings.Security.PrivacyProtocol = PrivacyProtocol.Aes256;
//                        break;
//                }
//            }
//        }

//        public string EncryptionPassword
//        {
//            get { return this.agentSettings.Security.PrivacyPassword; }
//            set { this.agentSettings.Security.PrivacyPassword = value; }
//        }

//        public int RemoteEngineBoots
//        {
//            get { return this.agentSettings.Security.EngineBoots; }
//            set { this.agentSettings.Security.EngineBoots = value; }
//        }

//        public int RemoteEngineTime
//        {
//            get { return this.agentSettings.Security.EngineTime; }
//            set { this.agentSettings.Security.EngineTime = value; }
//        }

//        #endregion

//        #region |   Public Methods   |

//        public SnmpData Get(string oid)
//        {
//            //this.snmpManager.Start();
//            //TODO:
//            return null;
//        }

//        public SnmpData GetNext(string oid)
//        {
//            //TODO:
//            return null;
//        }

//        public SnmpData GetNext()
//        {
//            //TODO:
//            return null;
//        }

//        public int Get(params string[] oids)
//        {
//            //TODO:
//            return -1;
//        }

//        public int GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids)
//        {
//            //TODO:
//            return -1;
//        }

//        public bool Set(string oid, object value, SnmpObjectValueType snmpObjectType)
//        {
//            //TODO:
//            return false;
//        }

//        public void Discover()
//        {
//            // TODO:
//        }

//        public void Dispose()
//        {
//            this.snmpManager.Dispose();
//        }

//        #endregion |   Public Methods   |


//        #region |   Private Methods   |

//        private void snmpManager_Table(object sender, TableEventArgs e)
//        {
//        }

//        private void snmpManager_UserState(object sender, UserStateEventArgs e)
//        {
//        }

//        private void snmpManager_Error(object sender, ComponentErrorEventArgs e)
//        {
//            Exception exception = e.GetException();
//            this.RaiseOnResponse(0, 0, 0, exception.Message, new List<SnmpData>());
//        }

//        private void snmpManager_Message(object sender, MessageEventArgs e)
//        {
//            this.RaiseOnResponse(0, 0, 0, e.Messages.ToString(), new List<SnmpData>());
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

//    public struct PowerSsnmpAgentSettings
//    {
//        public string Address;
//        public int Port;
//        public string Community;
//        public int Timeout;
//        public Dart.Snmp.SnmpVersion Version;
//        public bool UseMultipleGetRequests;
//        public string SelectedUser;
//        public Dart.Snmp.Security Security;

//        public override string ToString() { return this.Address; }
//    }
//}
