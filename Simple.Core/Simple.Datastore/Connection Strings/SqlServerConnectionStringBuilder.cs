using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
    public class SqlServerConnectionStringBuilder : DatastoreConnectionStringBuilderBase
    {
        public SqlServerConnectionStringBuilder()
        {
            this.TrustedConnection = true;
        }
        
        public string Server { get; set; }
        public string Database { get; set; }
        public bool NetworkConnection { get; set; }
        public int NetworkPort { get; set; }
        public bool TrustedConnection { get; set; }

        public string BuildConnectionString()
        {
            string connectionString = "Server=" + this.Server + (this.NetworkConnection ? "," + this.NetworkPort : "") + ";";
            
            connectionString += "Database=" + this.Database + ";";
            connectionString += "User ID=" + this.Username + ";";
            connectionString += "Password=" + this.Password + ";";
            connectionString += "Trusted_Connection=" + (this.TrustedConnection ? "Yes" : "No") + ";";
            connectionString += "MultipleActiveResultSets=true;";
            connectionString += "Connection Timeout=" + this.ConnectionTimeout.ToString() + ";";

            return connectionString;
        }
    }
}
