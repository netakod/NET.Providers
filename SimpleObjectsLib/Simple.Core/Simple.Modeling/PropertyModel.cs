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
	
	// TODO: Remove inheritance from ModelElement
	public class PropertyModel : IPropertyModel, IServerPropertyInfo, ICloneable // : ModelElement
	{
		private Type propertyType;
		private Type datastoreType;
		public static int UnspecifiedIndex = -1;

		public static List<Type> SerializationOptimizableTypeList = new List<Type>()
		{
			//typeof(object),
			//typeof(Boolean),
			//typeof(Byte),
			typeof(Int16),
			typeof(Int32),
			typeof(Int64),
			//typeof(Boolean[]),
			//typeof(Byte[]),
			//typeof(Int16[]),
			//typeof(Int32[]),
			//typeof(Int64[]),
			//typeof(Boolean?),
			//typeof(Byte?),
			typeof(Int16?),
			typeof(Int32?),
			typeof(Int64?),
			//typeof(Boolean?[]),
			//typeof(Byte?[]),
			//typeof(Int16?[]),
			//typeof(Int32?[]),
			//typeof(Int64?[]),
			typeof(SByte),
			typeof(UInt16),
			typeof(UInt32),
			typeof(UInt64),
			//typeof(SByte[]),
			//typeof(UInt16[]),
			//typeof(UInt32[]),
			//typeof(UInt64[]),
			typeof(SByte?),
			typeof(UInt16?),
			typeof(UInt32?),
			typeof(UInt64?),
			//typeof(SByte?[]),
			//typeof(UInt16?[]),
			//typeof(UInt32?[]),
			//typeof(UInt64?[]),
			typeof(Single),
			typeof(Double),
			typeof(Decimal),
			//typeof(Single[]),
			//typeof(Double[]),
			//typeof(Decimal[]),
			typeof(Single?),
			typeof(Double?),
			typeof(Decimal?),
			//typeof(Single?[]),
			//typeof(Double?[]),
			//typeof(Decimal?[]),
			typeof(DateTime),
			typeof(TimeSpan),
			//typeof(DateTime[]),
			//typeof(TimeSpan[]),
			typeof(DateTime?),
			typeof(TimeSpan?),
			//typeof(DateTime?[]),
			//typeof(TimeSpan?[]),
			typeof(BitVector32),
			typeof(Guid), // ????
			//typeof(BitVector32[]),
			//typeof(Guid[]),
			typeof(BitVector32?),
			typeof(Guid?), // ????
			//typeof(BitVector32?[]),
			//typeof(Guid?[]),
			//typeof(Char),
			//typeof(Char?),
			//typeof(String),
			//typeof(Char[]),
			//typeof(Char?[]),
			//typeof(String[]), // Do not use string generic serialization optimization for property values, since we do not know string context
			typeof(BitArray),
			typeof(ArrayList),
			//typeof(Type)
		};

		public PropertyModel(Type propertyType)
			: this(PropertyModel.UnspecifiedIndex, propertyName: null, propertyType)
		{
		}

		//public PropertyModel(int index, int propertyTypeId)
		//	: this(index, propertyTypeId, ObjectTypes.GetType(propertyTypeId))
		//{
		//}

		public PropertyModel(int propertyIndex, Type propertyType)
			: this(propertyIndex, propertyName: String.Empty, propertyType)
		{
		}

		//public PropertyModel(Type propertyType, string propertyName)
		//	: this(propertyRepository.Key, ObjectTypes.GetTypeId(propertyType), propertyType)
		//{
		//	this.PropertyRepository = propertyRepository;
		//}

		//public PropertyModel(int index, Type propertyType, string propertyName)
		//	: this(index, propertyType)
		//{
		//	this.Name = propertyName;
		//}

		public PropertyModel(int propertyIndex, string propertyName, Type propertyType)
        {
			//this.Index = propertyIndex;
			this.PropertyIndex = propertyIndex;
			//this.Name = propertyName;
			this.PropertyName = propertyName;
			this.propertyType = propertyType;
			this.PropertyType = propertyType;
			this.FieldType = propertyType;
			this.datastoreType = propertyType;
			this.DatastoreType = propertyType;
			
			//this.IsStorable = true;
			//this.IsClientToServerSeriazable = true;
			//this.IsServerToClientSeriazable = true;
			//this.IsClientToServerToClientSeriazable = true;
			//this.IncludeInTransactionActionLog = true;
			//this.AccessPolicy = PropertyAccessPolicy.ReadWrite;
			//this.GetAccessModifier = AccessModifier.Public;
			//this.SetAccessModifier = AccessModifier.Public;
			//this.FirePropertyValueChangeEvent = true;
			//this.AddOrRemoveInChangedProperties = true;
			//this.AutoGenerateProperty = true;
			this.IsSerializationOptimizable = SerializationOptimizableTypeList.Contains(propertyType) || propertyType.IsEnum;

			// TODO: Add DefaultObjectValue
			this.DefaultValue = propertyType.GetDefaultValue(); 
			this.IsNullable = (this.DefaultValue == null);
		}

		//public SimpleProperty PropertyRepository { get; private set; }
		//public int Index { get; set; }
		public int PropertyIndex { get; set; }
		public string PropertyName { get; set; }
		public string Caption { get; set; } = String.Empty;
		public string Caption2 { get; set; } = String.Empty;
		public string Description { get; set; } = String.Empty;
		public PropertyInfo? PropertyInfo { get; set; }
		public bool IsIndexed { get; set; }
		//public int ServerPropertyTypeId { get; set; }
		public PropertyAccessPolicy AccessPolicy { get; set; } = PropertyAccessPolicy.ReadWrite;

		public Type PropertyType
		{
			get { return this.propertyType; }
			set
			{
				this.propertyType = value;
				this.PropertyTypeId = PropertyTypes.GetPropertyTypeId(value);
			}
		}

		public int PropertyTypeId { get; set; }
		public Type FieldType { get; set; }

		public Type DatastoreType
		{
			get { return this.datastoreType; }
			set
			{
				this.datastoreType = value;
				this.DatastoreTypeId = PropertyTypes.GetPropertyTypeId(value);
			}
		}

		public string? DatastoreFieldName { get; set; } = null;
		public int DatastoreTypeId { get; private set; }
		public DbType DbType { get; set; }
		public OleDbType OleDbType { get; set; }
		public bool IsStorable { get; set; } = true;
		public bool IncludeInTransactionActionLog { get; set; } = true;

		public bool IsClientToServerSeriazable { get; set; } = true;
		public bool IsServerToClientSeriazable { get; set; } = true;
		public bool IsServerToClientTransactionInfoSeriazable { get; set; } = true; 

		public bool IsSerializationOptimizable { get; set; }

		public bool IsNullable { get; set; }
		public bool IsId { get; set; }
		public bool IsRelationTableId { get; set; }
		public bool IsRelationObjectId { get; set; }
		public bool IsPreviousId { get; set; }
		public bool IsOrderIndex { get; set; }
		//public bool IsActionSetOrderIndex { get; set; }
		public int RelationKey { get; set; }
		public bool IsEncrypted { get; set; }
		//public bool CanSetOnClientUpdate { get; set; }
		
		///// <summary>
		///// If different than null and if IsRelationKey is True this property specify related datastore realation key TableId field propery.
		///// This property will no longer require when Guid key will be replaced by object Id (long) as  a key.
		///// </summary>
		//public IPropertyModel TableIdAsRelationKeyPropertyModel { get; set; }
		
		//{
		//	get { return this.isKey || this.IsRelationKey; }
		//	set { this.isKey = value; }
		//}
		//public bool IsRelationKey { get; set; }

		/// <summary>
		/// If set true this property is not considered when rejecting property changes.
		/// </summary>
		public bool AvoidRejectChanges { get; set; }
		public AccessModifier GetAccessModifier { get; set; } = AccessModifier.Public;
		public AccessModifier SetAccessModifier { get; set; } = AccessModifier.Public;
		//public bool FirePropertyValueChangeEvent { get; set; } = true;
		//public bool AddOrRemoveInChangedProperties { get; set; } = true;
		public bool TrimStringBeforeComparison { get; set; } = true;

		public bool AutoGenerateProperty { get; set; } = true;
		public object? DefaultValue { get; set; }
		public object? Owner { get; set; }

		public PropertyModel Clone()
		{
			return (this.MemberwiseClone() as PropertyModel)!;
		}

		public override string ToString() => this.PropertyName;

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
	}

	//  public enum DatastoreFieldType
	//  {
	//      Default,
	//Byte,
	//Short,
	//Int,
	//Long,
	//Guid,
	//Decimal,
	//Float,
	//DateTime,
	//      Memo
	//  }
}
