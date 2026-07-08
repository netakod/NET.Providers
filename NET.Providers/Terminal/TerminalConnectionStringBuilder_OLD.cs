//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple;
//using Simple.Serialization;

//namespace NET.Tools.Terminal
//{
//    public class TerminalConnectionStringBuilder : ConnectionStringBuilder
//    {
//        public const bool DefaultUseTerminalConnection = true;
//        public const int DefaultTerminalPort = 23;
//        public const int DefaultTimeout = 20; // in seconds
//        public const int DefaultSendingInterval = 40; // in miliseconds
//        public const TerminalProtocol DefaultTerminalProtocol = TerminalProtocol.Telnet;
//        public const TelnetProviderType DefaultTelnetProviderType = TelnetProviderType.Default;
//        public const Ssh1ProviderType DefaultSsh1ProviderType = Ssh1ProviderType.Default;
//        public const Ssh2ProviderType DefaultSsh2ProviderType = Ssh2ProviderType.Default;
//        public const string DefaultPromptSeparator = "|";
//        public const string DefaultUsernamePrompts = "login|username|user name|user";
//        public const string DefaultPasswordPrompts = "password";
//        public const string DefaultEnableSecretPrompts = "password";
//        public const string DefaultNonPrivilegeModePrompts = ">";
//        public const string DefaultPrivilegeModeCommand = "enable";
//        public const string PrivilegeModeCommand3Com = "system-view";
//        public const string DefaultPrivilegeModePrompts = "#";
//        public const string PrivilegeModePrompt3Com = "]";
//        public const string DefaultMorePrompts = "--More--|---- More ----|-- More --|More: <space>";
//        public const string ConfigModeCommandConfigureTerminal = "configure terminal";
//        public const string ConfigModeCommandConfigure = "configure";
//        public const string DefaultExitConfigModeCommand = "exit";
//        public const string DafaultVlanDatabaseConfigCommand = "vlan database";
//        public const string DafaultExitVlanDatabaseConfigCommand = "exit";
//        public const string DafaultInterfaceConfigCommand = "interface";
//        public const string DafaultExitInterfaceConfigCommand = "exit";

//        public TerminalConnectionStringBuilder()
//            : base(TerminalConnectionStringModel.Instance)
//        {
//        }

//        public bool UseTerminalConnection
//        {
//            get { return this.GetPropertyValue<bool>(TerminalConnectionStringModel.PropertyModel.UseTerminalConnection, DefaultUseTerminalConnection); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.UseTerminalConnection, value.ToString()); }
//        }

//        public TerminalProtocol TerminalProtocol
//        {
//            get { return this.GetPropertyValue<TerminalProtocol>(TerminalConnectionStringModel.PropertyModel.TerminalProtocol, DefaultTerminalProtocol); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.TerminalProtocol, value.ToString()); }
//        }

//        public TelnetProviderType TelnetProviderType
//        {
//            get { return this.GetPropertyValue<TelnetProviderType>(TerminalConnectionStringModel.PropertyModel.TelnetProviderType, DefaultTelnetProviderType); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.TelnetProviderType, value.ToString()); }
//        }

//        public Ssh1ProviderType Ssh1ProviderType
//        {
//            get { return this.GetPropertyValue<Ssh1ProviderType>(TerminalConnectionStringModel.PropertyModel.Ssh1ProviderType, DefaultSsh1ProviderType); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.Ssh1ProviderType, value.ToString()); }
//        }

//        public Ssh2ProviderType Ssh2ProviderType
//        {
//            get { return this.GetPropertyValue<Ssh2ProviderType>(TerminalConnectionStringModel.PropertyModel.Ssh2ProviderType, DefaultSsh2ProviderType); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.Ssh2ProviderType, value.ToString()); }
//        }

//        public string RemoteHost
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.RemoteHost); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.RemoteHost, value); }
//        }

//        public int RemotePort
//        {
//            get { return this.GetPropertyValue<int>(TerminalConnectionStringModel.PropertyModel.RemotePort, DefaultTerminalPort); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.RemotePort, value); }
//        }
        
//        public string Username
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.Username); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.Username, value); }
//        }
        
//        public string Password
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.Password); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.Password, value); }
//        }

//        public string EnableSecret
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.EnableSecret); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.EnableSecret, value); }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.GetPropertyValue<int>(TerminalConnectionStringModel.PropertyModel.Timeout, DefaultTimeout); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.Timeout, value); }
//        }

//        /// <summary>
//        /// Time interval between two sendings in miliseconds.
//        /// </summary>
//        public int SendingInterval
//        {
//            get { return this.GetPropertyValue<int>(TerminalConnectionStringModel.PropertyModel.SendingInterval, DefaultSendingInterval); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.SendingInterval, value); }
//        }

//        public string PromptSeparator
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.PromptSeparator, DefaultPromptSeparator); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.PromptSeparator, value); }
//        }

//        public string UsernamePrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.UsernamePrompts, DefaultUsernamePrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.UsernamePrompts, value); }
//        }

//        public string PasswordPrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.PasswordPrompts, DefaultPasswordPrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.PasswordPrompts, value); }
//        }

//        public string EnableSecretPrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.EnableSecretPrompts, DefaultEnableSecretPrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.EnableSecretPrompts, value); }
//        }

//        public string NonPrivilegeModePrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.NonPrivilegeModePrompts, DefaultNonPrivilegeModePrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.NonPrivilegeModePrompts, value); }
//        }

//        public string MorePrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.MorePrompts, DefaultMorePrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.MorePrompts, value); }
//        }

//        public string PrivilegeModeCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.PrivilegeModeCommand, DefaultPrivilegeModeCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.PrivilegeModeCommand, value); }
//        }

//        public string PrivilegeModePrompts
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.PrivilegeModePrompts, DefaultPrivilegeModePrompts); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.PrivilegeModePrompts, value); }
//        }

//        public string ConfigModeCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.ConfigModeCommand, ConfigModeCommandConfigureTerminal); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.ConfigModeCommand, value); }
//        }

//        public string ExitConfigModeCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.ExitConfigModeCommand, DefaultExitConfigModeCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.ExitConfigModeCommand, value); }
//        }

//        public string VlanDatabaseConfigCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.VlanDatabaseConfigCommand, DafaultVlanDatabaseConfigCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.VlanDatabaseConfigCommand, value); }
//        }

//        public string ExitVlanDatabaseConfigCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.ExitVlanDatabaseConfigCommand, DafaultExitVlanDatabaseConfigCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.ExitVlanDatabaseConfigCommand, value); }
//        }

//        public string InterfaceConfigCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.InterfaceConfigCommand, DafaultInterfaceConfigCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.InterfaceConfigCommand, value); }
//        }

//        public string ExitInterfaceConfigCommand
//        {
//            get { return this.GetPropertyValue<string>(TerminalConnectionStringModel.PropertyModel.ExitInterfaceConfigCommand, DafaultExitInterfaceConfigCommand); }
//            set { this.SetPropertyValue(TerminalConnectionStringModel.PropertyModel.ExitInterfaceConfigCommand, value); }
//        }
//    }
//}