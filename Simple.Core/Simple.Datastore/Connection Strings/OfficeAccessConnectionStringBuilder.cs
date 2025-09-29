using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
    public class OfficeAccessConnectionStringBuilder : DatastoreConnectionStringBuilderBase
    {
        public OfficeAccessConnectionStringBuilder()
        {
            this.Username = "admin";
            this.Password = "";
        }
        
        public string DataSource { get; set; }

        public string BuildConnectionString()
        {
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;"; // "Provider =Microsoft.Jet.OLEDB.4.0;";

            connectionString += "Data Source=" + this.DataSource + ";";
            connectionString += "User Id=" + this.Username + ";";
            connectionString += "Password=" + this.Password + ";";

            return connectionString;
        }
    }
}
