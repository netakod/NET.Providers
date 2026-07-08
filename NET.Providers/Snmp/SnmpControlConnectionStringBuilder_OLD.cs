//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple;
//using Simple.Serialization;

//namespace NET.Tools.Snmp
//{
//    public class SnmpControlConnectionStringBuilder : ConnectionStringBuilder
//    {
//        public const bool DefaultUseSnmpConnection = true;
//        public const int DefaultSnmpPort = 161;
//        public const int DefaultTimeout = 6;
//        public const int DefaultNumOfRetries = 4;
//        public const SnmpProviderType DefaultProviderType = SnmpProviderType.Default;
//        public const SnmpVersion DefaultSnmpVersion = SnmpVersion.V2;
//        public const SnmpAuthenticationProtocol DefaultAuthenticationProtocol = SnmpAuthenticationProtocol.MD5;

//        public SnmpControlConnectionStringBuilder()
//            : base(SnmpControlConnectionStringModel.Instance)
//        {
//        }

//        public bool UseSnmpConnection
//        {
//            get { return this.GetPropertyValue<bool>(SnmpControlConnectionStringModel.PropertyModel.UseSnmpConnection, DefaultUseSnmpConnection); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.UseSnmpConnection, value.ToString()); }
//        }

//        public string CommunityString
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.CommunityString); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.CommunityString, value); }
//        }

//        public string RemoteHost
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.RemoteHost); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.RemoteHost, value); }
//        }

//        public int RemotePort
//        {
//            get { return this.GetPropertyValue<int>(SnmpControlConnectionStringModel.PropertyModel.RemotePort, DefaultSnmpPort); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.RemotePort, value); }
//        }
        
//        public SnmpProviderType ProviderType
//        {
//            get { return this.GetPropertyValue<SnmpProviderType>(SnmpControlConnectionStringModel.PropertyModel.ProviderType, DefaultProviderType); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.ProviderType, value.ToString()); }
//        }
        
//        public SnmpVersion SnmpVersion
//        {
//            get { return this.GetPropertyValue<SnmpVersion>(SnmpControlConnectionStringModel.PropertyModel.SnmpVersion, DefaultSnmpVersion); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.SnmpVersion, value.ToString()); }
//        }

//        public SnmpAuthenticationProtocol AuthenticationProtocol
//        {
//            get { return this.GetPropertyValue<SnmpAuthenticationProtocol>(SnmpControlConnectionStringModel.PropertyModel.AuthenticationProtocol, DefaultAuthenticationProtocol); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.AuthenticationProtocol, value.ToString()); }
//        }

//        public string AuthenticationPassword
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.AuthenticationPassword); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.AuthenticationPassword, value); }
//        }

//        public string Username
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.Username); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.Username, value); }
//        }

//        public string Password
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.Password); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.Password, value); }
//        }

//        public SnmpEncryptionProtocol EncryptionProtocol
//        {
//            get { return this.GetPropertyValue<SnmpEncryptionProtocol>(SnmpControlConnectionStringModel.PropertyModel.EncryptionProtocol); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.EncryptionProtocol, value); }
//        }

//        public string EncryptionPassword
//        {
//            get { return this.GetPropertyValue<string>(SnmpControlConnectionStringModel.PropertyModel.EncryptionPassword); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.EncryptionPassword, value); }
//        }

//        public int RemoteEngineBoots
//        {
//            get { return this.GetPropertyValue<int>(SnmpControlConnectionStringModel.PropertyModel.RemoteEngineBoots); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.RemoteEngineBoots, value); }
//        }

//        public int RemoteEngineTime
//        {
//            get { return this.GetPropertyValue<int>(SnmpControlConnectionStringModel.PropertyModel.RemoteEngineTime); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.RemoteEngineTime, value); }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.GetPropertyValue<int>(SnmpControlConnectionStringModel.PropertyModel.Timeout, DefaultTimeout); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.Timeout, value); }
//        }

//        public int NumOfRetries
//        {
//            get { return this.GetPropertyValue<int>(SnmpControlConnectionStringModel.PropertyModel.NumOfRetries, DefaultNumOfRetries); }
//            set { this.SetPropertyValue(SnmpControlConnectionStringModel.PropertyModel.NumOfRetries, value); }
//        }
//    }
//}