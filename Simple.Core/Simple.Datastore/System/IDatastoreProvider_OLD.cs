//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Simple;

//namespace Simple.Datastore
//{
//    public interface IDatastoreProvider : IDatastoreConnection, IDisposable
//    {
//        IList<string> GetTableNames();
//        IList<SimpleObjectKey> GetRecordKeys(string tableName, string guidFieldName);
//        IDictionary<string, object> GetRecord(string tableName, string idFieldName, object id);
//        IDictionary<string, object> GetRecord(string tableName, string idFieldName, object id, IEnumerable<string> fieldNames);
//        IList<IDictionary<string, object>> GetRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria);
//        IList<IDictionary<string, object>> GetRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria, IEnumerable<string> selectFieldNames);
//        IList<IDictionary<string, object>> GetAllRecords(string tableName);
//        IList<IDictionary<string, object>> GetAllRecords(string tableName, IEnumerable<string> fieldNames);
//		void InsertRecord(string tableName, string idFieldName, object id, IDictionary<string, object> fieldData);
//        void UpdateRecord(string tableName, string idFieldName, object id, IDictionary<string, object> fieldData);
//        void DeleteRecord(string tableName, string idFieldName, object id);
//		void DeleteRecords(string tableName, IEnumerable<WhereCriteriaElement> whereCriteria);
//		void DeleteAllRecords(string tableName);
//	}
//}
