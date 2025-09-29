using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Simple;

namespace Simple.Datastore
{
    public class DatastoreProviderSqlServer : SqlProviderBase, IDatastoreProvider, IDisposable
    {
        string datastoreName = String.Empty;
        //string datastoreVersion ="0.0.0.0";
        Version datastoreVersion = new Version();

		public DatastoreProviderSqlServer() { }

		protected new SqlConnection DbConnection => base.DbConnection as SqlConnection;

        public override void Connect()
        {
            base.Connect();

            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);
			//this.ExecuteSelectQuery(String.Format("EXEC sp_dboption '{0}', 'single user', 'true'", connectionStringBuilder.InitialCatalog));
			//this.ExecuteSelectQuery(String.Format("EXEC sys.databases '{0}', 'user_access', '1'", connectionStringBuilder.InitialCatalog));
			//this.ExecuteSelectQuery("EXEC sp_configure 'show advanced options', 1");
			//this.ExecuteSelectQuery("EXEC sp_configure 'user connections', 1");
			using (IDataReader dataReader = this.ExecuteSelectQuery("SELECT SERVERPROPERTY ('productversion'), SERVERPROPERTY ('productlevel'), SERVERPROPERTY ('edition')"))
			{
				if (dataReader.Read())
				{
					string versionString = dataReader[0].ToString();
					this.datastoreVersion = new Version(versionString);
				}

				dataReader.Close();
			}

			// Comment this line if you wish more DB connection instances !!!!
			//this.ExecuteSelectQuery(String.Format("USE master; ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", connectionStringBuilder.InitialCatalog)); // Only one database connection at the time
			//this.ExecuteSelectQuery(String.Format("USE master; ALTER DATABASE {0} SET MULTI_USER WITH ROLLBACK IMMEDIATE;", connectionStringBuilder.InitialCatalog)); // Allow multi user database acess

			this.datastoreName = connectionStringBuilder.InitialCatalog;
        }

        public override void Disconnect()
        {
            //SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);
            //this.ExecuteSelectQuery(String.Format("EXEC sp_dboption '{0}', 'single user', 'false'", connectionStringBuilder.InitialCatalog));
//            this.ExecuteSelectQuery(String.Format("EXEC sys.databases '{0}', 'user_access', '0'", connectionStringBuilder.InitialCatalog));
			//this.ExecuteSelectQuery(String.Format("USE master; ALTER DATABASE {0} SET MULTI_USER WITH ROLLBACK IMMEDIATE;", connectionStringBuilder.InitialCatalog));
			//this.ExecuteSelectQuery("EXEC sp_configure 'user connections', 0");
			this.datastoreName = String.Empty;

            base.Disconnect();
        }

		protected override string GetDatastoreName() => this.datastoreName;

		protected override string GetDatastoreTableName(string tableName)
        {
            string result = String.Empty;

            //if (this.datastoreVersion.Major >= 11) // SQL Server 2012 and above
            //{
				result = String.IsNullOrWhiteSpace(this.datastoreName) ? tableName : String.Format("{0}.dbo.{1}", this.datastoreName, tableName);
            //}
            //else // SQL Server 2008 and below
            //{
            //    result = tableName;
            //}

            return result;
        }

        //public override IList<string> GetTableNames()
        //{
        //    List<string> tableNames = new List<string>();
        //    SqlCommand cmd = new SqlCommand("SELECT * FROM information_schema.tables WHERE table_type = 'BASE TABLE'", this.DbConnection);
        //    SqlDataReader rdr = cmd.ExecuteReader();

        //    while (rdr.Read())
        //    {
        //        tableNames.Add(rdr[0].ToString());
        //    }

        //    return tableNames;
        //}

        //public IEnumerable<RecordKey> GetRecordKeys(string tableName, string idFieldName, string creatorServerIdFieldName)
        //{
        //    List<RecordKey> result = new List<RecordKey>();
        //    string query = this.BuildSelectQuery(tableName, String.Empty, new string[] { idFieldName, creatorServerIdFieldName });

        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = this.DbConnection;
        //        cmd.CommandText = query;

        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                int id = (int)reader[idFieldName];
        //                int creatorServerId = (int)reader[creatorServerIdFieldName];
        //                RecordKey recordKey = new RecordKey(id, creatorServerId);

        //                result.Add(recordKey);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public IDictionary<string, object> GetRecord(string tableName, string idFieldName, string creatorServerIdFieldName, int id, int creatorServerId)
        //{
        //    return this.GetRecord(tableName, idFieldName, creatorServerIdFieldName, id, creatorServerId, new List<string>());
        //}

        //public IDictionary<string, object> GetRecord(string tableName, string idFieldName, string creatorServerIdFieldName, int id, int creatorServerId, IEnumerable<string> fieldNames)
        //{
        //    string whereCriteria = String.Format("{0} = {1} And {2} = {3}", idFieldName, id.ToString(), creatorServerIdFieldName, creatorServerId.ToString());
        //    string query = this.BuildSelectQuery(tableName, whereCriteria, fieldNames);
        //    IEnumerable<IDictionary<string, object>> result = this.ExecuteSelectQuery(query);

        //    return result.Count() > 0 ? result.ElementAt(0) : null;
        //}

        //public IEnumerable<IDictionary<string, object>> GetRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria)
        //{
        //    return this.GetRecords(tableName, whereCriteria, new string[] { });
        //}

        //public IEnumerable<IDictionary<string, object>> GetRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria, IEnumerable<string> fieldNames)
        //{
        //    IEnumerable<IDictionary<string, object>> result = null;
        //    string whereCriteriaText = this.CreateWhereCriteriaText(whereCriteria);
        //    string query = this.BuildSelectQuery(tableName, whereCriteriaText, fieldNames);

        //    result = this.ExecuteSelectQuery(query);
        //    return result;
        //}

        //public IEnumerable<IDictionary<string, object>> GetAllRecords(string tableName)
        //{
        //    return this.GetAllRecords(tableName, new List<string>());
        //}

        //public IEnumerable<IDictionary<string, object>> GetAllRecords(string tableName, IEnumerable<string> fieldNames)
        //{
        //    IEnumerable<IDictionary<string, object>> result = null;
        //    string whereCriteriaText = String.Empty;
        //    string query = this.BuildSelectQuery(tableName, whereCriteriaText, fieldNames);
        //    result = this.ExecuteSelectQuery(query);

        //    return result;
        //}

        //public void InsertRecord(string tableName, IDictionary<string, object> data)
        //{
        //    string query = "Insert Into";
        //    string nameList = "";
        //    string valueList = "";
        //    string splitter = "";

        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Connection = this.DbConnection;

        //    int key = -1;

        //    foreach (string fieldName in data.Keys)
        //    {
        //        nameList += splitter + fieldName;
        //        valueList += splitter + "@" + fieldName;
        //        ;
        //        splitter = ", ";

        //        object value = data[fieldName];
        //        cmd.Parameters.AddWithValue("@" + fieldName, value);
        //    }

        //    query += " " + tableName + " (" + nameList + ") Values (" + valueList + ")";

        //    cmd.CommandText = query;
        //    cmd.ExecuteNonQuery();

        //    cmd.CommandText = queryGetID;
        //    key = (int)cmd.ExecuteScalar();
        //}

        //public void UpdateRecord(string tableName, string idFieldName, string creatorServerIdFieldName, int id, int creatorServerId, IDictionary<string, object> fieldData)
        //{
        //    string query = "Update";
        //    string nameList = "";
        //    string splitter = "";
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Connection = this.DbConnection;

        //    foreach (string fieldName in fieldData.Keys)
        //    {
        //        nameList += splitter + fieldName + " = ?";
        //        splitter = ", ";

        //        cmd.Parameters.AddWithValue(fieldName, fieldData[fieldName]);
        //    }

        //    query += " " + tableName + " Set " + nameList + " Where " + idFieldName + " = " + id + " And " + creatorServerIdFieldName + " = " + creatorServerId;

        //    cmd.CommandText = query;
        //    cmd.ExecuteNonQuery();
        //}

        //public void DeleteRecord(string tableName, string idFieldName, string creatorServerIdFieldName, int id, int creatorServerId)
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Connection = this.DbConnection;
        //    cmd.CommandText = "Delete * From " + tableName + " Where " + idFieldName + " = " + id + " And " + creatorServerIdFieldName + " = " + creatorServerId;
        //    cmd.ExecuteNonQuery();
        //}

        protected override IDbConnection CreateDbConnection() => new SqlConnection();

        protected override IDbCommand CreateDbCommand() => new SqlCommand();
    }
}
