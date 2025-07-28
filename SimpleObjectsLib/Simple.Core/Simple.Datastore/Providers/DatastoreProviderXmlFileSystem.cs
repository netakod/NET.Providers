using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using Simple;
using Simple.Modeling;
using Simple.Collections;

namespace Simple.Datastore
{
	public class DatastoreProviderXmlFileSystem : IDatastoreProvider
	{
		private const string xmlDatastoreRecordFileExtension = "xrec";
		private bool isConnected = false;
		private XmlDatastoreConnectionStringBuilder connectionStringBuilder = new XmlDatastoreConnectionStringBuilder();
		private Dictionary<string, DirectoryInfo> tableDirectoryInfosByTableName = new Dictionary<string, DirectoryInfo>();
		//private string[] tableNamesByTableId = null;
		private string datastoreName = String.Empty;

		public DatastoreProviderXmlFileSystem() { }

		public string ConnectionString { get => this.connectionStringBuilder.BuildConnectionString(); set => this.connectionStringBuilder.SetConnectionString(value); }

		public bool Connected { get => this.isConnected; set => this.isConnected = value; }

		public void SetDatastoreName(string datastoreName) => this.datastoreName = datastoreName;

		//public string[] TableNamesByTableId
		//{
		//	get { return this.tableNamesByTableId; }
		//}

		public void Connect() => this.Connected = Directory.Exists(this.connectionStringBuilder.DataSourceFolder);

		public void Disconnect() => this.Connected = false;

		public string[] GetTableNames()
		{
			List<string> result = new List<string>();
			DirectoryInfo datastoreLocationDirectory = new System.IO.DirectoryInfo(this.connectionStringBuilder.DataSourceFolder);

			foreach (DirectoryInfo directoryInfo in datastoreLocationDirectory.GetDirectories())
				result.Add(directoryInfo.Name);

			return result.ToArray();
		}

		public List<TKey> GetRecordKeys<TKey>(TableInfo tableInfo, int idPropertyIndex, string idFieldName)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableInfo.TableName);
			FileInfo[] recordFiles = tableDirectory.GetFiles(String.Format("*{0}", xmlDatastoreRecordFileExtension));
			List<TKey> values = new List<TKey>(recordFiles.Length);

			for (int i = 0; i < recordFiles.Length; i++)
				values[i] = Conversion.TryChangeType<TKey>(recordFiles[i].Name);

			return values;
		}

		//public List<TKey> GetRecordKeys<TKey>(string tableName, int idPropertyIndex, string idFieldName, IEnumerable<WhereCriteriaElement> whereCriteria)
		//{
		//	IList<IDictionary<string, object>> records = this.GetRecordsInternal(tableName, whereCriteria);
		//	List<TKey> values = new List<TKey>(records.Count);

		//	foreach (var record in records)
		//	{
		//		TKey key = Conversion.TryChangeType<TKey>(record[idFieldName]);

		//		values.Add(key);
		//	}

		//	return values;
		//}

		//public List<string> GetFieldNames(string tableName)
		//{
		//	List<object> keys = this.GetRecordKeys<object>(tableName, null);

		//	if (keys.Count() > 0)
		//	{
		//		IDictionary<string, object> recordData = this.GetRecordInternal(tableName, keys[0]);

		//		return recordData.Keys.ToList();
		//	}
		//	else
		//	{
		//		return new List<string>();
		//	}
		//}

		public IDataReader GetRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id, IEnumerable<int>? propertyIndexes = null, Func<int, IPropertyModel>? getPropertyModel = null)
		{
			IDictionary<string, object?> valuesByFieldName = this.GetRecordInternal(tableInfo.TableName, id);
			DataTable dataTable = new DataTable();

			if (valuesByFieldName.Count > 0)
			{
				foreach (string fieldName in valuesByFieldName.Keys)
					dataTable.Columns.Add(fieldName);

				DataRow dataRow = dataTable.NewRow();

				foreach (var item in valuesByFieldName)
					dataRow[item.Key] = item.Value;

				dataTable.Rows.Add(dataRow);
			}

			return dataTable.CreateDataReader();
		}

		public IDataReader GetRecords(TableInfo tableInfo, IEnumerable<int>? propertyIndexes = null, IEnumerable<WhereCriteriaElement>? whereCriteria = null, Func<int, IPropertyModel>? getPropertyModel = null)
		{
			IList<IDictionary<string, object>> records = GetRecordsInternal(tableInfo.TableName, whereCriteria, getPropertyModel);
			DataTable dataTable = new DataTable();

			if (records.Count > 0)
			{
				//DataColumn recordCountColumn = null;

				//if (includeRecordCountField)
				//	recordCountColumn = dataTable.Columns.Add();

				foreach (string fieldName in records[0].Keys)
					dataTable.Columns.Add(fieldName);

				DataRow dataRow = dataTable.NewRow();

				foreach (var record in records)
				{
					foreach (var valuesByFieldName in record)
					{
						//if (includeRecordCountField)
						//	dataRow[recordCountColumn] = records.Count;

						dataRow[valuesByFieldName.Key] = valuesByFieldName.Value;
					}
				}

				dataTable.Rows.Add(dataRow);
			}

			return dataTable.CreateDataReader();

		}




		//public IList<IDictionary<string, object>> GetAllRecords(string tableName)
		//{
		//	return this.GetAllRecords(tableName, new List<string>());
		//}

		//public IList<IDictionary<string, object>> GetAllRecords(string tableName, IEnumerable<string> fieldNames)
		//{
		//	return this.GetRecords(tableName, new List<WhereCriteriaElement>(), fieldNames);
		//}


		public void InsertRecord(TableInfo tableInfo, IEnumerable<PropertyIndexValuePair>? propertyIndexeValues, Func<int, IPropertyModel> getPropertyModel)
		{
			int propertyCount = propertyIndexeValues?.Count() ?? 0;
			SimpleDictionary<string, object?> recordData = new SimpleDictionary<string, object?>(propertyCount);
			object id = 0;

			if (propertyCount > 0)
			{
				//recordData.Add(keyFieldName, key);
				id = propertyIndexeValues.ElementAt(0).PropertyValue!;

				for (int i = 0; i < propertyIndexeValues.Count(); i++)
				{
					var item = propertyIndexeValues.ElementAt(i);
					IPropertyModel propertyModel = getPropertyModel(item.PropertyIndex);
					string fieldName = propertyModel.DatastoreFieldName;

					recordData.Add(fieldName, item.PropertyValue);
				}
			}

			this.WriteRecord(tableInfo.TableName, id, recordData);
		}

		public void UpdateRecord(TableInfo tableInfo, int idPropertyIndex, object id, IEnumerable<PropertyIndexValuePair>? propertyIndexeValues, Func<int, IPropertyModel> getPropertyModel)
		{
			IDictionary<string, object?> recordData = this.GetRecordInternal(tableInfo.TableName, id);
			int propertyCount = propertyIndexeValues?.Count() ?? 0;

			for (int i = 0; i < propertyCount; i++)
			{
				var item = propertyIndexeValues.ElementAt(i);
				IPropertyModel propertyModel = getPropertyModel(item.PropertyIndex);
				string fieldName = propertyModel.DatastoreFieldName;

				recordData[fieldName] = item.PropertyValue;
			}

			this.WriteRecord(tableInfo.TableName, id, recordData);
		}

		public void DeleteRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableInfo.TableName);
			File.Delete(String.Format("{0}\\{1}.{2}", tableDirectory.FullName, id.ToString(), xmlDatastoreRecordFileExtension));
		}

		public void DeleteRecords(TableInfo tableInfo, IEnumerable<WhereCriteriaElement> whereCriteria, Func<int, IPropertyModel> getPropertyModel)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableInfo.TableName);
			FileInfo[] recordFiles = tableDirectory.GetFiles(String.Format("*{0}", xmlDatastoreRecordFileExtension));

			foreach (FileInfo fileInfo in recordFiles)
			{
				TextReader textReeader = new StreamReader(fileInfo.FullName);
				SimpleDictionary<string, object> fieldData = XmlHelper.DeserializeObject<SimpleDictionary<string, object>>(textReeader.ReadToEnd());

				if (this.MatchWhereCriteria(fieldData, whereCriteria, getPropertyModel))
					fileInfo.Delete();
			}
		}

		public void DeleteAllRecords(string tableName)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableName);
			File.Delete(String.Format("{0}\\*.{1}", tableDirectory.FullName, xmlDatastoreRecordFileExtension));
		}

		public void Dispose()
		{
			//this.connectionStringBuilder = null;
			//this.tableDirectoryInfosByTableName = null;
		}

		private DirectoryInfo GetTableDirectory(string tableName)
		{
			DirectoryInfo? result;

			if (!this.tableDirectoryInfosByTableName.TryGetValue(tableName, out result))
			{
				result = Directory.CreateDirectory(String.Format("{0}\\{1}", this.connectionStringBuilder.DataSourceFolder, tableName));
				this.tableDirectoryInfosByTableName.Add(tableName, result);
			}

			return result;
		}

		private IDictionary<string, object?> GetRecordInternal(string tableName, object id)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableName);
			TextReader textReeader = new StreamReader(String.Format("{0}\\{1}.{2}", tableDirectory.FullName, id.ToString(), xmlDatastoreRecordFileExtension));
			SimpleDictionary<string, object?> valuesByFieldName = XmlHelper.DeserializeObject<SimpleDictionary<string, object?>>(textReeader.ReadToEnd());

			return valuesByFieldName;
		}

		private IList<IDictionary<string, object>> GetRecordsInternal(string tableName, IEnumerable<WhereCriteriaElement>? whereCriteria, Func<int, IPropertyModel>? getPropertyModel)
		{
			List<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableName);
			FileInfo[] recordFiles = tableDirectory.GetFiles(String.Format("*{0}", xmlDatastoreRecordFileExtension));

			foreach (FileInfo fileInfo in recordFiles)
			{
				TextReader textReeader = new StreamReader(fileInfo.FullName);
				SimpleDictionary<string, object> fieldData = XmlHelper.DeserializeObject<SimpleDictionary<string, object>>(textReeader.ReadToEnd());

				if (this.MatchWhereCriteria(fieldData, whereCriteria, getPropertyModel))
					result.Add(fieldData);
			}

			return result;
		}

		private void WriteRecord(string tableName, object key, IDictionary<string, object?> recordData)
		{
			DirectoryInfo tableDirectory = this.GetTableDirectory(tableName);
			TextWriter textWriter = new StreamWriter(String.Format("{0}\\{1}.{2}", tableDirectory.FullName, key.ToString(), xmlDatastoreRecordFileExtension), append: false);
			
			textWriter.Write(XmlHelper.SerializeObject(new SimpleDictionary<string, object?>(recordData)));
			textWriter.Close();
		}

		private bool MatchWhereCriteria(IDictionary<string, object> fieldData, IEnumerable<WhereCriteriaElement>? whereCriteria, Func<int, IPropertyModel>? getPropertyModel)
		{
			bool result = true;

			if (whereCriteria != null && getPropertyModel != null && whereCriteria.Count() > 0)
			{
				bool currentCompare = false;

				for (int i = 0; i < whereCriteria.Count(); i++)
				{
					WhereCriteriaElement whereCriteriaElement = whereCriteria.ElementAt(i);
					IPropertyModel propertyModel = getPropertyModel(whereCriteriaElement.PropertyIndex);
					object fieldValue = this.GetFieldValue(fieldData, propertyModel.DatastoreFieldName);
					currentCompare = this.Compare(fieldValue, whereCriteriaElement.FieldValue, whereCriteriaElement.Comparator);

					if (i > 0)
						result = this.Compare(result, currentCompare, whereCriteriaElement.ComparatorWithPreviousElement);
				}

				if (whereCriteria.Count() == 1)
					result = currentCompare;

				return result;
			}
			else
			{
				return result;
			}
		}

		private bool Compare(object dataValue1, object dataValue2, WhereComparator whereComparator)
		{
			bool result = false;
			bool dataValueCompare = ReflectionHelper.Equals(dataValue1, dataValue2);

			switch (whereComparator)
			{
				case WhereComparator.Equal:

					result = dataValueCompare;

					break;

				case WhereComparator.NotEqual:

					result = !dataValueCompare;

					break;

				default:
					throw new Exception("Unsupported WhereComparator " + whereComparator.ToString());
			}

			return result;
		}

		private bool Compare(bool value1, bool value2, LogicalComparator logicalComparator)
		{
			bool result = false;

			switch (logicalComparator)
			{
				case LogicalComparator.AND:

					result = value1 && value2;

					break;

				case LogicalComparator.OR:

					result = value1 || value2;

					break;

				default:
					throw new Exception("Unsupported LogicalComparator " + logicalComparator.ToString());
			}

			return result;
		}

		private object GetFieldValue(IDictionary<string, object> fieldData, string fieldName)
		{
			object result;

			if (fieldData.TryGetValue(fieldName, out result))
				result = null;
			
			return result;
		}
	}
}
