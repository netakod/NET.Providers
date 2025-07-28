using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Reflection;


namespace Simple.Modeling
{
	public interface IPropertyModel : IServerPropertyInfo //: IModelElement
	{
		//int Index { get; }
		//int PropertyIndex { get; }
		string Caption { get; }
		string Caption2 { get; }
		string Description { get; }
		Type PropertyType { get; }
		//int PropertyTypeId { get; }
		PropertyInfo? PropertyInfo { get; }
		Type FieldType { get; }
		Type DatastoreType { get; }
		string? DatastoreFieldName { get; }
		//int DatastoreTypeId { get; }
		DbType DbType { get; }
		OleDbType OleDbType { get; }
		//bool IsEncrypted { get; }
		//int ServerPropertyTypeId { get; }
		bool IsIndexed { get; }
		//bool IsStorable { get; }
		//bool IncludeInTransactionActionLog { get; }
		//bool IsMemberOfSerializationSequence { get; }
		//bool IsSerializationOptimizable { get; }
		bool IsNullable { get; }
		bool IsId { get; }
		//bool IsRelationTableId { get; }
		//bool IsRelationObjectId { get; }
		bool IsPreviousId { get; set; }
		bool IsOrderIndex { get; }
		//bool IsActionSetOrderIndex { get; }
		int RelationKey { get; }
		//bool CanSetOnClientUpdate { get; }

		PropertyAccessPolicy AccessPolicy { get; }

		/// <summary>
		/// If set true this property is not considered when rejecting object property changes.
		/// </summary>
		bool AvoidRejectChanges { get; }
		AccessModifier GetAccessModifier { get; }
		AccessModifier SetAccessModifier { get; }
		//bool FirePropertyValueChangeEvent { get; }

		// TODO: Remove this
		//bool AddOrRemoveInChangedProperties { get; }
		bool TrimStringBeforeComparison { get; }


		bool AutoGenerateProperty { get; }
		//object DefaultValue { get; }
		object? Owner { get; }
	}
}
