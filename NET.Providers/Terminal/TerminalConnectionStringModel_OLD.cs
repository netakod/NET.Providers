//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple.Modeling;

//namespace NET.Tools.Terminal
//{
//    public class TerminalConnectionStringModel : ConnectionStringModel<TerminalConnectionStringModel>
//    {
//        public static readonly TerminalConnectionStringPropertyModel PropertyModel = new TerminalConnectionStringPropertyModel();

//        public TerminalConnectionStringModel()
//        {
//            this.CreatePropertyModelDictionary(PropertyModel);
//        }
//    }

//    public class TerminalConnectionStringPropertyModel : ConnectionStringPropertyBaseModel
//    {
//        public ConnectionStringPropertyModel UseTerminalConnection   = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel TerminalProtocol        = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel TelnetProviderType      = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Ssh1ProviderType        = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Ssh2ProviderType        = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel RemoteHost              = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel RemotePort              = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Username                = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel Password                = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel EnableSecret            = new ConnectionStringPropertyModel() { ProtectByEncryption = true };
//        public ConnectionStringPropertyModel Timeout                 = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel SendingInterval         = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel PromptSeparator         = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel UsernamePrompts         = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel PasswordPrompts         = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel EnableSecretPrompts     = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel NonPrivilegeModePrompts = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel MorePrompts             = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel PrivilegeModeCommand    = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel PrivilegeModePrompts    = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel ConfigModeCommand       = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel ExitConfigModeCommand   = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel VlanDatabaseConfigCommand = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel ExitVlanDatabaseConfigCommand = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel InterfaceConfigCommand  = new ConnectionStringPropertyModel();
//        public ConnectionStringPropertyModel ExitInterfaceConfigCommand = new ConnectionStringPropertyModel();
//    }
//}
