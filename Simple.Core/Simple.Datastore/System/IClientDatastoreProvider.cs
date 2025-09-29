using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Simple.Datastore
{
    public interface IClientDatastoreProvider : IDatastoreConnection, IDisposable
    {
        IList<string> GetTableNames();
        IList<Guid> GetRecordKeys(int tableId, string guidFieldName);
        IDictionary<string, object> GetRecord(int tableId, string idFieldName, object id);
        IDictionary<string, object> GetRecord(int tableId, string idFieldName, object id, IEnumerable<string> fieldNames);
        IList<IDictionary<string, object>> GetRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria);
        IList<IDictionary<string, object>> GetRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria, IEnumerable<string> fieldNames);
        IList<IDictionary<string, object>> GetAllRecords(int tableId);
        IList<IDictionary<string, object>> GetAllRecords(int tableId, IEnumerable<string> fieldNames);
		void InsertRecord(int tableId, IDictionary<string, object> fieldData);
        void UpdateRecord(int tableId, string idFieldName, object id, IDictionary<string, object> fieldData);
        void DeleteRecord(int tableId, string idFieldName, object id);
		void DeleteRecords(int tableId, IEnumerable<WhereCriteriaElement> whereCriteria);
	}
}
