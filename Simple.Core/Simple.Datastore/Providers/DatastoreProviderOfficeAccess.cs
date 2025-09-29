using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;
using Simple;
using Simple.Modeling;

namespace Simple.Datastore
{
   //[SupportedOSPlatform("windows")]
   public class DatastoreProviderOfficeAccess : SqlProviderBase, IDatastoreProvider, IDisposable
    {
		public DatastoreProviderOfficeAccess() { }

		protected new OleDbConnection DbConnection => base.DbConnection as OleDbConnection;

		//[SupportedOSPlatform("windows")]
		public new IList<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            System.Data.DataTable userTables = null;

            // We only want user tables, not system tables
            string[] restrictions = new string[4];
            restrictions[3] = "Table";

            // Get list of user tables
            userTables = this.DbConnection.GetSchema("Tables", restrictions);

            // Add list of table names to output list
            for (int i = 0; i < userTables.Rows.Count; i++)
                tableNames.Add(userTables.Rows[i][2].ToString());

            return tableNames;
        }

		//public override void InsertRecord(string tableName, IPropertyModel[] propertyModelSequence, object[] fieldValues)
		//{
		//	//if (tableName == "SystemProperties" || tableName == "SystemTransactions")
		//	//	return;

		//	//string conString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\admin\Desktop\del\SHAFI\db.accdb";
		//	//using (OleDbConnection con = new OleDbConnection(conString))
		//	//using (OleDbCommand cmd = this.DbConnection.CreateCommand())
		//	//{
		//	//string query = "INSERT INTO TEST ([Number], Amount) VALUES (?, ?)";
		//	string nameList = "";
		//	string valueList = "";
		//	string splitter = "";
		//	string datastoreName = this.GetDatastoreTableName(tableName);

		//		//OleDbCommand cmd2 = this.DbConnection.CreateCommand();
		//		//var param = cmd2.Parameters.Add(null);
		//		//param.OleDbType
		//		//string query2 = "INSERT INTO Folders ([Id], [Name], [Description]) VALUES (@Id, @Name, @Description)";

		//		//cmd2.Parameters.AddWithValue("@Id", 3);
		//		//cmd2.Parameters.AddWithValue("@Name", "TEST");
		//		//cmd2.Parameters.AddWithValue("@Description", "This is test");
		//		//cmd2.CommandText = query2;
		//		//cmd2.ExecuteNonQuery();

		//		using (IDbCommand cmd = this.BuildDbTextCommand()) //this.DbConnection.CreateCommand())
		//		{
		//			//cmd.CommandType = CommandType.Text;
		//			//cmd.Connection = this.DbConnection;
		//			//this.DbConnection.Open();

		//			try
		//			{
		//				for (int i = 0; i < propertyModelSequence.Length; i++)
		//				{
		//					IPropertyModel propertyModel = propertyModelSequence[i];
		//					string fieldName = propertyModel.Name;
		//					object value = fieldValues[i];

		//					nameList += splitter + "[" + fieldName + "]";
		//					valueList += splitter + "@" + fieldName;
		//					//valueList += splitter + "?";
		//					splitter = ", ";

		//					//cmd.Parameters.Add(new OleDbParameter(propertyModel.Name, fieldValues[i] ?? DBNull.Value));
		//					//cmd.Parameters.AddWithValue("@" + propertyModel.Name, fieldValues[i] ?? DBNull.Value);
		//					this.AddCommandParameter(cmd, propertyModel, fieldName, value);
		//					//cmd.Parameters.Add(fieldValues[i] ?? DBNull.Value);
		//					//cmd.AddParameter(propertyName, value);
		//					//cmd.Parameters.AddWithValue(cmd, propertyName, value);
		//				}

		//				cmd.CommandText = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", datastoreName, nameList, valueList);
		//				this.PrepeareConnection();
		//				cmd.ExecuteNonQuery();
		//			}
		//			catch (Exception ex)
		//			{
		//				throw ex;
		//			}
		//		}
		//	//}
		//}

		protected override void OnAddCommandParameter(IDbCommand dbCommand, IDbDataParameter dbDataParameter, IPropertyModel propertyModel, string fieldName, object? fieldValue)
		{
			if (dbDataParameter is OleDbParameter oleDbParameter)
			{
				if (propertyModel.PropertyType == typeof(DateTime))
				{
					oleDbParameter.OleDbType = OleDbType.Date;
				}
				else if (oleDbParameter.OleDbType == OleDbType.BigInt) // Access has no long (4 bytes) field, so any long value should be converted to OleDbType.Integer (BingInt fild in access). Note that Access Integer field is 2 bytes, BigInt is 4 bytes.
				{
					oleDbParameter.OleDbType = OleDbType.Integer;
				}
				else if (propertyModel.OleDbType != OleDbType.Empty)
				{
					oleDbParameter.OleDbType = propertyModel.OleDbType;
				}
			}
		}

		//public IEnumerable<RecordKey> GetRecordKeys(string tableName, string idFieldName, string creatorServerIdFieldName)
		//{
		//    List<RecordKey> result = new List<RecordKey>();
		//    string query = this.BuildSelectQuery(tableName, String.Empty, new string[] { idFieldName, creatorServerIdFieldName });

		//    using (OleDbCommand cmd = new OleDbCommand())
		//    {
		//        cmd.CommandType = CommandType.Text;
		//        cmd.Connection = this.DbConnection;
		//        cmd.CommandText = query;

		//        using (OleDbDataReader reader = cmd.ExecuteReader())
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

		//    OleDbCommand cmd = new OleDbCommand();
		//    cmd.CommandType = CommandType.Text;
		//    cmd.Connection = this.DbConnection;

		//    int key = -1;

		//    foreach (string fieldName in data.Keys)
		//    {
		//        nameList += splitter + fieldName;
		//        valueList += splitter + "@" + fieldName;;
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
		//    OleDbCommand cmd = new OleDbCommand();
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
		//    OleDbCommand cmd = new OleDbCommand();
		//    cmd.CommandType = CommandType.Text;
		//    cmd.Connection = this.DbConnection;
		//    cmd.CommandText = "Delete * From " + tableName + " Where " + idFieldName + " = " + id + " And " + creatorServerIdFieldName + " = " + creatorServerId;
		//    cmd.ExecuteNonQuery();
		//}

		protected override string GetDatastoreName() => this.DbConnection.Database;

		protected override string GetDatastoreTableName(string tableName) => tableName;

		protected override IDbConnection CreateDbConnection() => new OleDbConnection();

		protected override IDbCommand CreateDbCommand() => new OleDbCommand();
    }
}