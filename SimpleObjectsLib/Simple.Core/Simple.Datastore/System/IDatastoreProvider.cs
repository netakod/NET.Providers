using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Simple;
using Simple.Modeling;
using Simple.Datastore;

namespace Simple.Datastore
{
    public interface IDatastoreProvider : IDatastoreConnection, IDisposable
    {
        //string[] GetTableNames();

		List<TKey> GetRecordKeys<TKey>(TableInfo tableInfo, int idPropertyIndex, string idFieldName);
		//List<TKey> GetRecordKeys<TKey>(string tableName, int idPropertyIndex, string idFieldName, IEnumerable<WhereCriteriaElement> whereCriteria);
		//List<string> GetFieldNames(string tableName);
		IDataReader GetRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id, IEnumerable<int>? propertyIndexes = null, Func<int, IPropertyModel>? getPropertyModel = null);
		IDataReader GetRecords(TableInfo tableInfo, IEnumerable<int>? propertyIndexes = null, IEnumerable<WhereCriteriaElement>? whereCriteria = null, Func<int, IPropertyModel>? getPropertyModel = null);
		void InsertRecord(TableInfo tableInfo, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel);
		void UpdateRecord(TableInfo tableInfo, int idPropertyIndex, object id, IEnumerable<PropertyIndexValuePair>? propertyIndexValues, Func<int, IPropertyModel> getPropertyModel);
		void DeleteRecord(TableInfo tableInfo, int idPropertyIndex, string idFieldName, object id);
		void DeleteRecords(TableInfo tableInfo, IEnumerable<WhereCriteriaElement> whereCriteria, Func<int, IPropertyModel> getPropertyModel);
	}
}
