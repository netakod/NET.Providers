//using System.Data.OleDb;
//using System.Management;
using Simple;
using Simple.Modeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
	// TODO: Add NumOfRetry for datastore connection
	public abstract class SqlProviderBase : IDatastoreProvider, IDatastoreConnection, IDisposable
	{
        private IDbConnection? dbConnection = null;
        private string connectionString = String.Empty;
		private readonly object lockDataReader = new object();
		//private string[] tableNamesByTableId = null;
		//private string datastoreName = String.Empty;

		//protected const string queryGetLastID = "Select @@Identity";
		//protected const string querySelectTableNames = "SELECT * FROM {0}.information_schema.tables WHERE table_type = 'BASE TABLE'";

		public SqlProviderBase()
		{
		}

        //protected string DatastoreName
        //{
        //    get { return this.datastoreName; }
        //}

		//public string[] TableNamesByTableId
		//{
		//	get { return this.tableNamesByTableId; }
		//}


		public string ConnectionString
        {
            get { return this.connectionString; }
			set 
			{ 
				this.connectionString = value;

				if (this.dbConnection != null)
					this.dbConnection.ConnectionString = this.ConnectionString;
			}
		}

        public bool Connected
        {
            get { return this.dbConnection != null && this.DbConnection?.State == ConnectionState.Open; }
        }

        protected IDbConnection? DbConnection
        {
            get
            {
                if (this.dbConnection == null)
                {
                    this.dbConnection = this.CreateDbConnection();
                    this.dbConnection.ConnectionString = this.ConnectionString;
                }

                return this.dbConnection;
            }
        }

        public virtual void Connect()
        {
            this.DbConnection?.Open();
        }

        public virtual void Disconnect()
        {
            this.DbConnection?.Close();
            this.DbConnection?.Dispose();
            this.dbConnection = null;
        }

        public virtual void Dispose()
        {
            if (this.dbConnection != null)
            {
                this.dbConnection.Dispose();
                this.dbConnection = null;
            }
        }

        public virtual List<string> GetTableNames()
        {
			string tableName = this.GetDatastoreName();
			string query = this.BuildSelectQuery(tableName + ".information_schema.tables", whereCriteria: "[table_type] = 'BASE TABLE'");
			
			return this.GetSingleElementQueryResult<string>(query, reader => reader["TABLE_NAME"].ToString());
        }

        public virtual List<TKey> GetRecordKeys<TKey>(TableInfo tableInfo, int idPropertyIndex, string idFieldName)
        {
			string datastoreTableName = this.GetDatastoreTableName(tableInfo.TableName);
			string query = this.BuildSelectQuery(datastoreTableName, whereCriteria: String.Empty, fieldNames: idFieldName);
			
			return this.GetSingleElementQueryResult<TKey>(query);
        }

		//public virtual List<TKey> GetRecordKeys<TKey>(string tableName, int idPropertyIndex, string idFieldName, IEnumerable<WhereCriteriaElement> whereCriteria)
		//{
		//	string whereCriteriaText = this.CreateWhereCriteriaText(whereCriteria);
		//	string query = this.BuildSelectQuery(this.GetDatastoreTableName(tableName), whereCriteriaText, fieldNames: idFieldName);
		//	return this.GetSingleElementQueryResult<TKey>(query);
		//}

		//public virtual List<string> GetFieldNames(string tableName)
		//{
		//	string query = "SELECT TOP 0 * FROM " + this.GetDatastoreTableName(tableName);

		//	using (IDataReader dataReader = this.ExecuteSelectQuery(query))
		//	{
		//		List<string> result = new List<string>(dataReader.FieldCount);

		//		for (int i = 0; i < dataReader.FieldCount; i++)
		//			result[i] = dataReader.GetName(i);

		//		dataReader.Close();

		//		return result;
		//	}
		//}

		//public IDataReader GetRecord(string tableName, string idFieldName, object id)
		//{
		//    return this.GetRecord(tableName, idFieldName, id);
		//}

		public virtual IDataReader GetRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id, IEnumerable<int>? propertyIndexes, Func<int, IPropertyModel>? getPropertyModel)
        {
            string whereCriteria = String.Format("[{0}] = {1}", idFieldName, id.ToString());
			string[] fieldNames = this.GetFieldNames(propertyIndexes, getPropertyModel);
            string query = this.BuildSelectQuery(this.GetDatastoreTableName(tableInfo.TableName), whereCriteria, fieldNames);

			return this.ExecuteSelectQuery(query);

			//IEnumerable<IDictionary<string, object>> result = this.ExecuteSelectQuery(query);

   //         return result.Count() > 0 ? result.ElementAt(0) : null;
        }

		//public IDataReader GetRecords(string tableName)
		//{
		//	return this.GetRecords(tableName, fieldNames: null);
		//}

		//public IDataReader GetRecords(string tableName, params string[] fieldNames)
		//{
		//	return this.GetRecords(tableName, whereCriteria: null, fieldNames: fieldNames);
		//}

		//public IDataReader GetRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria)
		//      {
		//          return this.GetRecords(tableName, whereCriteria, null);
		//      }

		public virtual IDataReader GetRecords(TableInfo tableInfo, IEnumerable<int>? propertyIndexes = null, IEnumerable<WhereCriteriaElement>? whereCriteria = null, Func<int, IPropertyModel>? getPropertyModel = null)
        {
            string whereCriteriaText = this.CreateWhereCriteriaText(whereCriteria, getPropertyModel);
			string[] fieldNames = this.GetFieldNames(propertyIndexes, getPropertyModel);
			string query = this.BuildSelectQuery(this.GetDatastoreTableName(tableInfo.TableName), whereCriteriaText, fieldNames);

			return this.ExecuteSelectQuery(query);
        }


		public virtual void InsertRecord(TableInfo tableInfo, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel)
        {
            string nameList = "";
            string valueList = "";
            string splitter = "";
			string datastoreTableName = this.GetDatastoreTableName(tableInfo.TableName);

			using (IDbCommand cmd = this.BuildDbTextCommand())
			{
				if (propertyIndexValues != null)
				{
					for (int i = 0; i < propertyIndexValues.Count(); i++)
					{
						var item = propertyIndexValues.ElementAt(i);
						IPropertyModel propertyModel = getPropertyModel(item.PropertyIndex);
						string fieldName = propertyModel.DatastoreFieldName;

						nameList += splitter + "[" + fieldName + "]";
						valueList += splitter + "@" + fieldName; //"?";
						splitter = ", ";

						this.AddCommandParameter(cmd, propertyModel, fieldName, item.PropertyValue);
					}
				}

				cmd.CommandText = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", datastoreTableName, nameList, valueList);
				
				this.PrepeareConnection();

				// Get the key if autonumbering
				//object keyObject = dbCommand.ExecuteScalar();
				//int key = Simple.Reflection.Converter.TryChangeType<int>(keyObject, -1);

				cmd.ExecuteNonQuery();
			}
        }

		public virtual void UpdateRecord(TableInfo tableInfo, int idPropertyIndex, object id, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel)
		{
			string nameList = "";
			string splitter = "";
			string datastoreTableName = this.GetDatastoreTableName(tableInfo.TableName);
			string idFieldName = getPropertyModel(idPropertyIndex).DatastoreFieldName;

			using (IDbCommand cmd = this.BuildDbTextCommand())
			{
				if (propertyIndexValues != null)
				{
					for (int i = 0; i < propertyIndexValues.Count(); i++)
					{
						var item = propertyIndexValues.ElementAt(i);
						IPropertyModel propertyModel = getPropertyModel(item.PropertyIndex);
						string fieldName = propertyModel.DatastoreFieldName;

						nameList += String.Format("{0}[{1}] = @{2}", splitter, fieldName, fieldName);
						splitter = ", ";

						//cmd.AddParameter(propertyName, value);
						this.AddCommandParameter(cmd, propertyModel, fieldName, item.PropertyValue);
					}
				}

				cmd.CommandText = String.Format("UPDATE {0} SET {1} WHERE [{2}] = {3}", datastoreTableName, nameList, idFieldName, id); ;
				this.PrepeareConnection();
				cmd.ExecuteNonQuery();
			}
        }

        public virtual void DeleteRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id)
        {
            string datastoreTableName = this.GetDatastoreTableName(tableInfo.TableName);
            string query = String.Format("DELETE FROM {0} WHERE [{1}] = {2}", datastoreTableName, idFieldName, id);

			using (IDbCommand cmd = this.BuildDbCommand(query))
			{
				this.PrepeareConnection();
				cmd.ExecuteNonQuery();
			}
        }

		public virtual void DeleteRecords(TableInfo tableInfo, IEnumerable<WhereCriteriaElement> whereCriteria, Func<int, IPropertyModel> getPropertyModel)
		{
			string datastoreTableName = this.GetDatastoreTableName(tableInfo.TableName);
			string whereCriteriaText = (whereCriteria != null && whereCriteria.Count() > 0) ? " WHERE " + this.CreateWhereCriteriaText(whereCriteria, getPropertyModel) : String.Empty;
			string query = String.Format("DELETE FROM {0}{1}", datastoreTableName, whereCriteriaText);

			using (IDbCommand cmd = this.BuildDbCommand(query))
			{
				this.PrepeareConnection();
				cmd.ExecuteNonQuery();
			}
		}

		//public void DeleteAllRecords(string tableName)
		//{
		//	string datastoreTableName = this.GetDatastoreTableName(tableName);
		//	string query = String.Format("Delete From {0}", datastoreTableName);

		//	IDbCommand dbCommand = this.BuildDbCommand(query);
		//	this.PrepeareConnection();
		//	dbCommand.ExecuteNonQuery();
		//}
		
		//public void SetDatastoreName(string datastoreName)
        //{
        //    this.datastoreName = datastoreName;
        //}

        protected abstract IDbConnection CreateDbConnection();
        protected abstract IDbCommand CreateDbCommand();
		protected abstract string GetDatastoreName();
        protected abstract string GetDatastoreTableName(string tableName);
		//{
		//    return String.IsNullOrEmpty(this.datastoreName) ? tableName : String.Format("{0}.dbo.{1}", this.datastoreName, tableName);
		//}

		//protected virtual string GetQueryFieldName(string originalFieldName)
		//{
		//	return "[" + originalFieldName + "]";
		//}
		//protected string BuildSelectQuery(string tableName, string whereCriteria)
		//{
		//    return this.BuildSelectQuery(tableName, whereCriteria, new string[] { });
		//}

		//protected T[] GetSingleElementQueryResult<T>(string query, bool recordCountIncluded = false) // includeRecordCount must be true in query!
		//{
		//	Func<IDataReader, T> getElementFunc;

		//	if (recordCountIncluded)
		//	{
		//		getElementFunc = (reader) => ((T)reader[1]);
		//	}
		//	else
		//	{
		//		getElementFunc = (reader) => ((T)reader[0]);
		//	}

		//	return this.GetSingleElementQueryResult<T>(query, getElementFunc, recordCountIncluded);
		//}

		protected List<T> GetSingleElementQueryResult<T>(string query)
		{
			return this.GetSingleElementQueryResult<T>(query, (reader) => reader[0]);
		}

		protected List<T> GetSingleElementQueryResult<T>(string query, Func<IDataReader, object> getElementFunc) //, bool recordCountIncluded) // includeRecordCount must be true in query!
		{
			List<T> result = new List<T>();

			lock (this.lockDataReader)
			{
				this.PrepeareConnection();
				
				using (IDbCommand dbCommand = this.BuildDbCommand(query))
				{
					IDataReader dataReader = dbCommand.ExecuteReader();

					try
					{
						while (dataReader.Read())
						{
							object data = getElementFunc(dataReader);
							T value = Conversion.TryChangeType<T>(data);

							result.Add(value);
						}
					}
					catch { }
					finally
					{
						if (dataReader != null)
							dataReader.Close();
					}
				}
			}

			return result;
		}

		protected string BuildSelectQuery(string tableName, string whereCriteria, params string[] fieldNames)
        {
            string selectFields = String.Empty;
            string selectFilterCriteria = String.Empty;
            string separator = String.Empty;

			if (fieldNames != null)
			{
				foreach (string filedName in fieldNames)
				{
					selectFields += separator + "[" + filedName + "]";
					separator = ", ";
				}
			}

            if (selectFields == String.Empty)
                selectFields = "*";

            if (!whereCriteria.IsNullOrWhiteSpace())
                selectFilterCriteria = " WHERE " + whereCriteria;

			//string datastoreTableName = this.GetDatastoreTableName(tableName);

			//string query = (includeRecordCount) ? String.Format("With Total As ( Select Count(*) as [{0}] From {2} ) Select Total.[{0}], {1} From Total, {2}{3}", 
			//													DatastoreProvider.RecordCountFieldName, selectFields, tableName, selectFilterCriteria) :
			//									  String.Format("Select {0} From {1}{2}", selectFields, tableName, selectFilterCriteria);
			string query = String.Format("SELECT {0} FROM {1}{2}", selectFields, tableName, selectFilterCriteria);

			return query;
		}

		//protected IList<IDictionary<string, object>> ExecuteSelectQuery(string query)

		protected IDataReader ExecuteSelectQuery(string query)
        {
            this.PrepeareConnection();

			IDbCommand cmd = this.BuildDbCommand(query);
			IDataReader dataReader = cmd.ExecuteReader();

			return dataReader;

			//IDbCommand dbCommend = this.BuildDbCommand(query);
			//IDataReader dataReader = null;
			//List<IDictionary<string, object>> result = new List<IDictionary<string, object>>();

			//try
			//{
   //             dataReader = dbCommend.ExecuteReader();
   //             string[] readerFieldNames = new string[dataReader.FieldCount];

   //             for (int i = 0; i < readerFieldNames.Length; i++)
   //                 readerFieldNames[i] = dataReader.GetName(i);

   //             while (dataReader.Read())
   //             {
   //                 Dictionary<string, object> row = new Dictionary<string, object>();

   //                 for (int columnIndex = 0; columnIndex < readerFieldNames.Length; columnIndex++) // string fieldName in readerFieldNames)
   //                 {
   //                     string fieldName = readerFieldNames[columnIndex];
   //                     object fieldValue = dataReader[columnIndex];

   //                     if (fieldName.IsNullOrEmpty() || fieldName.Trim().Length == 0)
   //                         fieldName = String.Format("Column{0}", columnIndex);

			//			if (fieldValue == default(DBNull))
			//				fieldValue = null;

   //                     //if (fieldValue != null)
   //                         row.Add(fieldName, fieldValue);

   //                     columnIndex++;
   //                 }

   //                 result.Add(row);
   //             }
   //         }
   //         finally
   //         {
   //             if (dataReader != null)
   //                 dataReader.Close();
   //         }

            //return result;
        }

        protected virtual string CreateWhereCriteriaText(IEnumerable<WhereCriteriaElement>? whereCriteria, Func<int, IPropertyModel>? getPropertyModel)
        {
            string result = String.Empty;

			if (whereCriteria != null && getPropertyModel != null)
			{
				for (int i = 0; i < whereCriteria.Count(); i++)
				{
					WhereCriteriaElement whereCriteriaElement = whereCriteria.ElementAt(i);
					IPropertyModel propertyModel = getPropertyModel(whereCriteriaElement.PropertyIndex);

					if (i > 0)
						result += String.Format(" {0} ", this.CreateLogicalComparatorText(whereCriteriaElement.ComparatorWithPreviousElement));

					result += String.Format("[{0}] {1} '{2}'", propertyModel.DatastoreFieldName, this.CreateWhereComparatorText(whereCriteriaElement.Comparator), whereCriteriaElement.FieldValue);
				}
			}

            return result;
        }

        protected virtual string CreateWhereComparatorText(WhereComparator whereComparator)
        {
            string result = String.Empty;

            switch (whereComparator)
            {
                case WhereComparator.Equal:

					result = "=";
                    break;

                case WhereComparator.NotEqual:

					result = "<>";
                    break;

                default:
                    throw new Exception("Unsupported WhereComparator " + whereComparator.ToString());
            }

            return result;
        }

        protected virtual string CreateLogicalComparatorText(LogicalComparator logicalComparator)
        {
            string result = String.Empty;

            switch (logicalComparator)
            {
                case LogicalComparator.AND:

					result = "AND";
                    break;

                case LogicalComparator.OR:

					result = "OR";
                    break;

                default:
                    throw new Exception("Unsupported LogicalComparator " + logicalComparator.ToString());
            }

            return result;
        }

		protected void AddCommandParameter(IDbCommand dbCommand, IPropertyModel propertyModel, string fieldName, object? fieldValue)
		{
			IDbDataParameter dbDataParameter = dbCommand.CreateParameter();

			dbDataParameter.ParameterName = fieldName; //"@" + fieldName;
			dbDataParameter.Value = fieldValue;

			if (((propertyModel.IsRelationTableId || propertyModel.IsRelationObjectId) && fieldValue != null && !Comparison.IsGreaterThanZero(fieldValue)) || fieldValue == null)
				dbDataParameter.Value = DBNull.Value;
			else if (fieldValue.GetType().IsEnum) // if field value is enum (not propertyModel.PropertyType.IsEnum), eg. SystemTransaction.Status, convers it to int. // propertyModel.PropertyType.IsEnum)
				dbDataParameter.Value = (int)fieldValue;

				//if (propertyModel.PropertyTypeId == (int)PropertyTypeId.Binary) // && fieldValue == DBNull.Value
				//	dbDataParameter.Size = -1; // varbynary(MAX) unlimited size

				dbCommand.Parameters.Add(dbDataParameter);

			this.OnAddCommandParameter(dbCommand, dbDataParameter, propertyModel, fieldName, fieldValue);
		}

		protected virtual void OnAddCommandParameter(IDbCommand dbCommand, IDbDataParameter dbDataParameter, IPropertyModel propertyModel, string fieldName, object? fieldValue) { }

		protected IDbCommand BuildDbTextCommand()
        {
            return this.BuildDbCommand(String.Empty);
        }

        protected IDbCommand BuildDbCommand(string query)
        {
			IDbCommand dbCommand = this.CreateDbCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.Connection = this.DbConnection;
            dbCommand.CommandText = query;

            return dbCommand;
        }

		protected void PrepeareConnection()
        {
            while (this.DbConnection?.State != ConnectionState.Open)
               if (this.DbConnection?.State == ConnectionState.Broken || this.DbConnection?.State == ConnectionState.Closed)
                    this.Connect();
        }

		private string[] GetFieldNames(IEnumerable<int>? propertyIndexes, Func<int, IPropertyModel>? getPropertyModel)
		{
			string[] fieldNames;

			if (propertyIndexes != null && getPropertyModel != null)
			{
				fieldNames = new string[propertyIndexes.Count()];

				for (int i = 0; i < propertyIndexes.Count(); i++)
				{
					int propertyIndex = propertyIndexes.ElementAt(i);
					IPropertyModel propertyModel = getPropertyModel(propertyIndex);

					fieldNames[i] = propertyModel.DatastoreFieldName;
				}
			}
			else
			{
				fieldNames = new string[0];
			}

			return fieldNames;
		}
	}

	//public static class SqlProviderExtensions
	//{
	//	public static void AddWithValue(this IDataParameterCollection dataParameterCollection, IDbCommand dbCommand, string fieldName, object fieldValue)
	//	{
	//		IDbDataParameter param = dbCommand.CreateParameter();

	//		param.ParameterName = fieldName;
	//		param.Value = fieldValue ?? DBNull.Value;

	//		dataParameterCollection.Add(param);
	//	}

	//	public static IDbDataParameter AddParameter(this IDbCommand dbCommand, string fieldName, object fieldValue)
	//	{
	//		IDbDataParameter param = dbCommand.CreateParameter();

	//		param.ParameterName = fieldName;
	//		param.Value = fieldValue ?? DBNull.Value;

	//		dbCommand.Parameters.Add(param);

	//		return param;
	//	}
	//}
}
