using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Simple;
using Simple.Security;

namespace Simple.Serialization
{
    public class ConnectionStringBuilder
    {
		public const string StrProtocol = "Protocol";
		public const string StrProtocolVersion = "ProtocolVersion";
		//public const string StrIsEncrypted = "IsEncrypted";

		//private static readonly ConnectionStringPropertyModelBase PropertyModel = new ConnectionStringPropertyModelBase();
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        //private IConnectionStringModel connectionStringModel = null;
        //private List<string> propertyOrders = new List<string>();
        //private List<string> propertiesToEncrypt = new List<string>();

        public ConnectionStringBuilder()
        {
            // Force sets PropeertyModel elements Name and Caption
            //ModelHelper.CreateModelCollectionByReflection<ConnectionStringPropertyModel>(PropertyModel, null);
        }

        ////[CLSCompliant(true)]
        //public ConnectionStringBuilder(IConnectionStringModel connectionStringModel)
        //    : this()
        //{
        //    this.connectionStringModel = connectionStringModel;
        //    this.IsEncrypted = true;
        //    //this.PropertyOrders.AddRange(new string[] {
        //    //    stringProtocol,
        //    //    stringProtocolVersion,
        //    //    stringPasswordEncription,
        //    //});
        //}

        //public ProviderConnectionStringBuilder(IConnectionStringModel connectionStringModel, string connectionString)
        //    : this(connectionStringModel)
        //{
        //    this.SetConnectionString(connectionString);
        //}

        public object this[string name]
        {
            get { return this.properties[name]; }
            protected set { this.properties[name] = value; }
        }

        //public virtual string ConnectionString
        //{
        //    get { return this.CreateConnectionString(); }
        //    set
        //    {
        //        this.ValidateConnectionString(value, true);
        //        this.ParseConnectionString(value, this.properties);
        //    }
        //}

        public virtual string Protocol
        {
            get { return this.GetPropertyValue<string>(StrProtocol); }
            set { this.SetPropertyValue(StrProtocol, value); }
        }

        public virtual string ProtocolVersion
        {
            get { return this.GetPropertyValue<string>(StrProtocolVersion); }
            set { this.SetPropertyValue(StrProtocolVersion, value); }
        }

        //public virtual bool IsEncrypted
        //{
        //    get { return this.GetPropertyValue<bool>(StrIsEncrypted); }
        //    protected set { this.SetPropertyValue(StrIsEncrypted, value); }
        //}

        //protected List<string> PropertyOrders
        //{
        //    get { return this.propertyOrders; }
        //}

        //protected List<string> PropertiesToEncrypt
        //{
        //    get { return this.propertiesToEncrypt; }
        //}

        
        public T GetPropertyValue<T>(string key)
        {
            T value = this.GetValue<T>(key, () => default(T));
            return value;
        }

        public T GetPropertyValue<T>(string key, T defaultValue)
        {
            T value = this.GetValue<T>(key, () => defaultValue);
            return value;
        }

        public void SetPropertyValue(string key, object value)
        {
            this.ValidateProperty(key, value, true);
            this[key] = value;
        }

        public void Clear()
        {
            this.properties.Clear();
        }

        protected string EncryptProperty(string propertyValue, ICryptoTransform encryptor) => PasswordSecurity.Encrypt(propertyValue, encryptor);
        //{
        //          if (this.IsEncrypted)
        //	{
        //		foreach (IConnectionStringPropertyModel connectionStringPropertyModel in this.connectionStringModel.Properties.Values)
        //		{
        //			if (connectionStringPropertyModel.ProtectByEncryption)
        //			{
        //				string propertyValue = this.GetPropertyValue<string>(connectionStringPropertyModel.Name);

        //				this.SetPropertyValue(connectionStringPropertyModel.Name, PasswordSecurity.EncryptPassword(propertyValue, encryptor, blockSize));
        //			}
        //		}
        //	}
        //}

        public string DecryptProperty(string encryptedPropertyValue, ICryptoTransform decryptor) => PasswordSecurity.Decrypt(encryptedPropertyValue, decryptor);
        //{
        //    if (this.IsEncrypted)
        //    {
        //        foreach (IConnectionStringPropertyModel connectionStringPropertyModel in this.connectionStringModel.Properties.Values)
        //        {
        //            if (connectionStringPropertyModel.ProtectByEncryption)
        //            {
        //                string encryptedPropertyValue = this.GetPropertyValue<string>(connectionStringPropertyModel.Name);
        //                this.SetPropertyValue(connectionStringPropertyModel.Name, PasswordSecurity.DecryptPassword(encryptedPropertyValue, encryptor, blockSize));
        //            }
        //        }
        //    }
        //}

        protected void ParseConnectionString(string connectionString, Dictionary<string, object> dictionary)
        {
            dictionary.Clear();

            string[] keyValuePairs = connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string keyValue in keyValuePairs)
            {
                string key = keyValue.Split('=')[0];
                string value = keyValue.Split('=')[1];

                dictionary[key] = value;
            }
        }

        protected virtual bool ValidateConnectionString(string connectionString, bool throwException)
        {
            return true;
        }

        protected virtual bool ValidateProperty(string propertyName, object propertyValue, bool throwException)
        {
            return true;
        }

        public string BuildConnectionString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string[] keys = new string[this.properties.Keys.Count];
            this.properties.Keys.CopyTo(keys, 0);

            //// Sort keys by preference list.
            //int index = 0;
            //foreach (string prefKey in this.propertyOrders)
            //{
            //    for (int i = index; i < keys.Length; i++)
            //    {
            //        if (keys[i] == prefKey)
            //        {
            //            string temp = keys[index];

            //            keys[index] = keys[i];
            //            keys[i] = temp;

            //            index = index + 1;

            //            break;
            //        }
            //    }
            //}

            // Build connection string.
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                object value = this.properties[key];

                stringBuilder.Append(String.Format("{0}={1};", key, value));
            }

            return stringBuilder.ToString();
        }

        public void SetConnectionString(string connctionString)
        {
            this.ValidateConnectionString(connctionString, true);
            this.ParseConnectionString(connctionString, this.properties);
        }

        private T GetValue<T>(string key, Func<T> getDefaultValue)
        {
            if (this.properties.ContainsKey(key))
            {
                object valueObject = this[key];
                
                T value = Conversion.TryChangeType<T>(valueObject, getDefaultValue);
                
                return value;
            }
            else
            {
                return getDefaultValue();
            }
        }

        public static bool IsEqual(string connString1, string connString2)
        {
            ConnectionStringBuilder builder = new ConnectionStringBuilder();

            Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>();

            builder.ParseConnectionString(connString1, dictionary1);
            builder.ParseConnectionString(connString2, dictionary2);

            if (dictionary1.Keys.Count != dictionary1.Keys.Count)
            {
                return false;
            }
            else
            {
                bool match = true;
                
                foreach (string key in dictionary1.Keys)
                {
                    if (dictionary2.ContainsKey(key))
                    {
                        string val1 = dictionary1[key].ToString();
                        string val2 = dictionary2[key].ToString();

                        if (val1 != val2)
                        {
                            match = false;
                            break;
                        }
                    }
                    else
                    {
                        match = false;
                        break;
                    }
                }

                return match;
            }
        }
    }
}
