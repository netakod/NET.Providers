//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data.OleDb;
//using System.Collections;
//using Simple;

//namespace Simple.Datastore
//{
//    public class ClientDatastore //: IDatastoreProvider
//    {
//		IClientDatastoreProvider datastoreProvider = null;
//		private object lockObject = new object();

//		public ClientDatastore(IClientDatastoreProvider clientDatastoreProvider)
//        {
//			this.datastoreProvider = clientDatastoreProvider;
//        }

//		private IClientDatastoreProvider DatastoreProvider
//		{
//			get { return this.datastoreProvider; }
//		}

//		public string ConnectionString
//        {
//            get { return this.DatastoreProvider.ConnectionString; }
//            set { this.DatastoreProvider.ConnectionString = value; }
//        }

//        public bool Connected
//        {
//            get { return this.DatastoreProvider.Connected; }
//        }


//        //public void SetDatastoreName(string datastoreName)
//        //{
//        //    this.datastoreProvider.SetDatastoreName(datastoreName);
//        //}

//        public void Connect()
//        {
//            lock (lockObject)
//            {
//                this.DatastoreProvider.Connect();
//            }
//        }

//        public void Disconnect()
//        {
//            lock (lockObject)
//            {
//                this.DatastoreProvider.Disconnect();
//            }
//        }

//        public IList<string> GetTableNames()
//        {
//            lock (lockObject)
//            {
//                return this.DatastoreProvider.GetTableNames();
//            }
//        }

//        public IList<Guid> GetRecordKeys(int tableId, string guidFieldName)
//        {
//            lock (lockObject)
//            {
//				return this.DatastoreProvider.GetRecordKeys(tableId, guidFieldName);
//			}
//        }

//        public IDictionary<string, object> GetRecord(int tableId, string idFieldName, object id)
//        {
//            lock (lockObject)
//            {
//                return this.GetRecord(tableId, idFieldName, id, new string[] { });
//            }
//        }

//        public IDictionary<string, object> GetRecord(int tableId, string idFieldName, object id, IEnumerable<string> fieldNames)
//        {
//            lock (lockObject)
//            {
//				return this.DatastoreProvider.GetRecord(tableId, idFieldName, id, fieldNames);
//			}
//        }

//        public IList<IDictionary<string, object>> GetRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria)
//        {
//            lock (lockObject)
//            {
//				return this.DatastoreProvider.GetRecords(tableId, whereCriteria);
//			}
//        }
//        public IList<IDictionary<string, object>> GetRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria, IEnumerable<string> fieldNames)
//        {
//            lock (lockObject)
//            {
//				return this.DatastoreProvider.GetRecords(tableId, whereCriteria, fieldNames);
//			}
//        }

//        public IList<IDictionary<string, object>> GetAllRecords(int tableId)
//        {
//            lock (lockObject)
//            {
//                return this.DatastoreProvider.GetAllRecords(tableId);
//            }
//        }

//        public IList<IDictionary<string, object>> GetAllRecords(int tableId, IEnumerable<string> fieldNames)
//        {
//            lock (lockObject)
//            {
//                return this.DatastoreProvider.GetAllRecords(tableId, fieldNames);
//            }
//        }

//        public void InsertRecord(int tableId, IDictionary<string, object> fieldData)
//        {
//            lock (lockObject)
//            {
//				this.DatastoreProvider.InsertRecord(tableId, fieldData);
//            }
//        }

//        public void UpdateRecord(int tableId, string idFieldName, object id, IDictionary<string, object> fieldData)
//        {
//            lock (lockObject)
//            {
//                this.DatastoreProvider.UpdateRecord(tableId, idFieldName, id, fieldData);
//            }
//        }

//        public void DeleteRecord(int tableId, string idFieldName, object id)
//        {
//            lock (lockObject)
//            {
//                this.DatastoreProvider.DeleteRecord(tableId, idFieldName, id);
//            }
//        }

//		public void DeleteRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria)
//		{
//			lock (lockObject)
//			{
//				this.DatastoreProvider.DeleteRecords(tableId, whereCriteria);
//			}
//		}

//        public void Dispose()
//        {
//            this.datastoreProvider = null;
//        }
//    }
//}
