//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple.Modeling;
//using System.Drawing;

//namespace NET.Tools.Snmp
//{
//    public class SnmpControlConnectionStringModel : ConnectionStringModel<SnmpControlConnectionStringModel>
//    {
//        public static readonly SnmpControlConnectionStringPropertyModel PropertyModel = new SnmpControlConnectionStringPropertyModel();

//        public SnmpControlConnectionStringModel()
//        {
//            this.CreatePropertyModelDictionary(PropertyModel);
//        }
//    }

//    public class SnmpControlConnectionStringPropertyModel : ConnectionStringPropertyBaseModel
//    {
//        public ConnectionStringPropertyModel UseSnmpConnection      = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel ProviderType           = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel RemoteHost             = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel RemotePort             = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel SnmpVersion            = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel CommunityString        = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel AuthenticationProtocol = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel AuthenticationPassword = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel Username               = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Password               = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel EncryptionProtocol     = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel EncryptionPassword     = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel RemoteEngineBoots      = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel RemoteEngineTime       = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Timeout                = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel NumOfRetries           = new ConnectionStringPropertyModel();
//    }
//}
