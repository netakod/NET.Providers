using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Runtime.Versioning;
using Simple;
using Simple.Modeling;

namespace Simple.Datastore
{
    public class DatastoreProvider : IDatastoreProvider, IDatastoreConnection, IDisposable
	{
        private IDatastoreProvider? provider = null;
        private DatastoreProviderType datastoreProviderType;
        private Dictionary<DatastoreProviderType, Func<IDatastoreProvider>> datastoreCreatorsByProviderType = new Dictionary<DatastoreProviderType, Func<IDatastoreProvider>>();
		private object lockObject = new object();

		public static string RecordCountFieldName = "_RecordCount_";

        public DatastoreProvider()
        {
            // Datastore Provider definitions
            // New provider types add here!!!
            this.datastoreCreatorsByProviderType.Add(DatastoreProviderType.OfficeAccess, () => new DatastoreProviderOfficeAccess());
            this.datastoreCreatorsByProviderType.Add(DatastoreProviderType.SqlServer, () => new DatastoreProviderSqlServer());
            //this.datastoreCreatorsByProviderType.Add(DatastoreProviderType.FileSystem,	() => new DatastoreProviderXmlDatastore());

            // Sets the default provider
            this.DatastoreType = DatastoreProviderType.SqlServer;
        }

        public DatastoreProviderType DatastoreType
        {
            get { return this.datastoreProviderType; }
            set
            {
                string connectionString = string.Empty;
                
                if (this.provider != null)
                {
                    connectionString = this.provider.ConnectionString;
                    this.provider.Dispose();
                    this.provider = null;
                }

				this.datastoreProviderType = value;

				Func<IDatastoreProvider> createDatastore;

				if (this.datastoreCreatorsByProviderType.TryGetValue(value, out createDatastore))
                {
					this.provider = createDatastore();
					this.provider.ConnectionString = connectionString;
				}
            }
        }
        
        public string ConnectionString
        {
            get { return this.Provider.ConnectionString; }
            set { this.Provider.ConnectionString = value; }
        }

        public bool Connected
        {
            get { return this.Provider.Connected; }
        }

        private IDatastoreProvider Provider
        {
            get { return this.provider; }
        }

        //public void SetDatastoreName(string datastoreName)
        //{
        //    this.datastoreProvider.SetDatastoreName(datastoreName);
        //}

        public void Connect()
        {
            lock (lockObject)
            {
                this.provider.Connect();
            }
        }

        public void Disconnect()
        {
            lock (lockObject)
            {
                this.provider.Disconnect();
            }
        }

        //public List<string> GetTableNames()
        //{
        //    lock (lockObject)
        //    {
        //        return this.Provider.GetTableNames();
        //    }
        //}

        public List<TKey> GetRecordKeys<TKey>(TableInfo tableInfo, int idPropertyIndex, string idFieldName)
        {
            lock (lockObject)
            {
                return this.Provider.GetRecordKeys<TKey>(tableInfo, idPropertyIndex, idFieldName);
            }
        }

		//public List<TKey> GetRecordKeys<TKey>(string tableName, int idPropertyIndex, string idFieldName, IEnumerable<WhereCriteriaElement> whereCriteria)
		//{
		//	lock (lockObject)
		//	{
		//		return this.Provider.GetRecordKeys<TKey>(tableName, idPropertyIndex, idFieldName, whereCriteria);
		//	}
		//}

		//public List<string> GetFieldNames(string tableName)
		//{
		//	lock (lockObject)
		//	{
		//		return this.Provider.GetFieldNames(tableName);
		//	}
		//}

		public IDataReader GetRecord(TableInfo tableInfo, int idPropertyIndex, string idPropertyName, object id, IEnumerable<int>? propertyIndexes = null, Func<int, IPropertyModel>? getPropertyModel = null)
        {
            lock (lockObject)
            {
                return this.Provider.GetRecord(tableInfo, idPropertyIndex, idPropertyName, id, propertyIndexes, getPropertyModel);
            }
        }

        public IDataReader GetRecords(TableInfo tableInfo, IEnumerable<int>? propertyIndexes = null, IEnumerable<WhereCriteriaElement>? whereCriteria = null, Func<int, IPropertyModel>? getPropertyModel = null)
        {
            lock (lockObject)
            {
                return this.Provider.GetRecords(tableInfo, propertyIndexes, whereCriteria, getPropertyModel);
            }
        }

        public void InsertRecord(TableInfo tableInfo, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel)
        {
            lock (lockObject)
            {
				this.Provider.InsertRecord(tableInfo, propertyIndexValues, getPropertyModel);
            }
        }

        public void UpdateRecord(TableInfo tableInfo, int idPropertyIndex, object id, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel)
        {
            lock (lockObject)
            {
                this.Provider.UpdateRecord(tableInfo, idPropertyIndex, id, propertyIndexValues, getPropertyModel);
            }
        }

        public void DeleteRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id)
        {
            lock (lockObject)
            {
                this.Provider.DeleteRecord(tableInfo, idPropertyIndex, idFieldName, id);
            }
        }

		public void DeleteRecords(TableInfo tableInfo, IEnumerable<WhereCriteriaElement> whereCriteria, Func<int, IPropertyModel> getPropertyModel)
		{
			lock (lockObject)
			{
				this.Provider.DeleteRecords(tableInfo, whereCriteria, getPropertyModel);
			}
		}

        public void Dispose()
        {
            this.provider = null;
        }
    }
}
