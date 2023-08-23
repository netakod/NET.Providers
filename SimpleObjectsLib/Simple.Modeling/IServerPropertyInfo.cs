using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
	public interface IServerPropertyInfo
	{
		int PropertyIndex { get; }
		string PropertyName { get; }
		int PropertyTypeId { get; }
		int DatastoreTypeId { get; }
		bool IsRelationTableId { get; }
		bool IsRelationObjectId { get; }
		bool IsSerializationOptimizable { get; }
		bool IsClientSeriazable { get; }
		bool IsStorable { get; }
		bool IsEncrypted { get; }
		bool IncludeInTransactionActionLog { get; }
		object? DefaultValue { get; }
	}
}
