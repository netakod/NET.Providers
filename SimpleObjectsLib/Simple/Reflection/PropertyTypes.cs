using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class PropertyTypes
	{
		private static Type[] PropertyTypesByPropertyTypeId = new Type[(int)PropertyTypeId.String.Max() + 1]; // Enum.GetValues(typeof(TypeId)).Cast<int>().Max() + 1];
		private static Dictionary<Type, int> PropertyTypeIdsByType = new Dictionary<Type, int>(Count);

		//public static int TypeIdObject = (int)TypeId.Object;
		//public static int TypeIdBoolean = (int)TypeId.Boolean;
		//public static int TypeIdByte = (int)TypeId.Byte;
		//public static int TypeIdInt16 = (int)TypeId.Int16;
		//public static int TypeIdInt32 = (int)TypeId.Int32;
		//public static int TypeIdInt64 = (int)TypeId.Int64;
		//public static int TypeIdBooleanArray = (int)TypeId.BooleanArray;
		//public static int TypeIdByteArray = (int)TypeId.ByteArray;
		//public static int TypeIdInt16Array = (int)TypeId.Int16Array;
		//public static int TypeIdInt32Array = (int)TypeId.Int32Array;
		//public static int TypeIdInt64Array = (int)TypeId.Int64Array;
		//public static int TypeIdNullableBoolean = (int)TypeId.NullableBoolean;
		//public static int TypeIdNullableByte = (int)TypeId.NullableByte;
		//public static int TypeIdNullableInt16 = (int)TypeId.NullableInt16;
		//public static int TypeIdNullableInt32 = (int)TypeId.NullableInt32;
		//public static int TypeIdNullableInt64 = (int)TypeId.NullableInt64;
		//public static int TypeIdNullableBooleanArray = (int)TypeId.NullableBooleanArray;
		//public static int TypeIdNullableByteArray = (int)TypeId.NullableByteArray;
		//public static int TypeIdNullableInt16Array = (int)TypeId.NullableInt16Array;
		//public static int TypeIdNullableInt32Array = (int)TypeId.NullableInt32Array;
		//public static int TypeIdNullableInt64Array = (int)TypeId.NullableInt64Array;
		//public static int TypeIdSByte = (int)TypeId.SByte;
		//public static int TypeIdUInt16 = (int)TypeId.UInt16;
		//public static int TypeIdUInt32 = (int)TypeId.UInt32;
		//public static int TypeIdUInt64 = (int)TypeId.UInt64;
		//public static int TypeIdSByteArray = (int)TypeId.SByteArray;
		//public static int TypeIdUInt16Array = (int)TypeId.UInt16Array;
		//public static int TypeIdUInt32Array = (int)TypeId.UInt32Array;
		//public static int TypeIdUInt64Array = (int)TypeId.UInt64Array;
		//public static int TypeIdNullableSByte = (int)TypeId.NullableSByte;
		//public static int TypeIdNullableUInt16 = (int)TypeId.NullableUInt16;
		//public static int TypeIdNullableUInt32 = (int)TypeId.NullableUInt32;
		//public static int TypeIdNullableUInt64 = (int)TypeId.NullableUInt64;
		//public static int TypeIdNullableSByteArray = (int)TypeId.NullableSByteArray;
		//public static int TypeIdNullableUInt16Array = (int)TypeId.NullableUInt16Array;
		//public static int TypeIdNullableUInt32Array = (int)TypeId.NullableUInt32Array;
		//public static int TypeIdNullableUInt64Array = (int)TypeId.NullableUInt64Array;
		//public static int TypeIdSingle = (int)TypeId.Single;
		//public static int TypeIdDouble = (int)TypeId.Double;
		//public static int TypeIdDecimal = (int)TypeId.Decimal;
		//public static int TypeIdSingleArray = (int)TypeId.SingleArray;
		//public static int TypeIdDoubleArray = (int)TypeId.DoubleArray;
		//public static int TypeIdDecimalArray = (int)TypeId.DecimalArray;
		//public static int TypeIdNullableSingle = (int)TypeId.NullableSingle;
		//public static int TypeIdNullableDouble = (int)TypeId.NullableDouble;
		//public static int TypeIdNullableDecimal = (int)TypeId.NullableDecimal;
		//public static int TypeIdNullableSingleArray = (int)TypeId.NullableSingleArray;
		//public static int TypeIdNullableDoubleArray = (int)TypeId.NullableDoubleArray;
		//public static int TypeIdNullableDecimalArray = (int)TypeId.NullableDecimalArray;
		//public static int TypeIdDateTime = (int)TypeId.DateTime;
		//public static int TypeIdTimeSpan = (int)TypeId.TimeSpan;
		//public static int TypeIdDateTimeArray = (int)TypeId.DateTimeArray;
		//public static int TypeIdTimeSpanArray = (int)TypeId.TimeSpanArray;
		//public static int TypeIdNullableDateTime = (int)TypeId.NullableDateTime;
		//public static int TypeIdNullableTimeSpan = (int)TypeId.NullableTimeSpan;
		//public static int TypeIdNullableDateTimeArray = (int)TypeId.NullableDateTimeArray;
		//public static int TypeIdNullableTimeSpanArray = (int)TypeId.NullableTimeSpanArray;
		//public static int TypeIdBitVector32 = (int)TypeId.BitVector32;
		//public static int TypeIdGuid = (int)TypeId.Guid;
		//public static int TypeIdBitVector32Array = (int)TypeId.BitVector32Array;
		//public static int TypeIdGuidArray = (int)TypeId.GuidArray;
		//public static int TypeIdNullableBitVector32 = (int)TypeId.NullableBitVector32;
		//public static int TypeIdNullableGuid = (int)TypeId.NullableGuid;
		//public static int TypeIdNullableBitVector32Array = (int)TypeId.NullableBitVector32Array;
		//public static int TypeIdNullableGuidArray = (int)TypeId.NullableGuidArray;
		//public static int TypeIdChar = (int)TypeId.Char;
		//public static int TypeIdNullableChar = (int)TypeId.NullableChar;
		//public static int TypeIdString = (int)TypeId.String;
		//public static int TypeIdCharArray = (int)TypeId.CharArray;
		//public static int TypeIdNullableCharArray = (int)TypeId.NullableCharArray;
		//public static int TypeIdStringArray = (int)TypeId.StringArray;
		//public static int TypeIdBitArray = (int)TypeId.BitArray;
		//public static int TypeIdArrayList = (int)TypeId.ArrayList;
		//public static int TypeIdType = (int)TypeId.Type;

		static PropertyTypes()
		{
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Object] = typeof(object);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.String] = typeof(String);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Boolean] = typeof(Boolean);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableBoolean] = typeof(Boolean?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.SByte] = typeof(SByte);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableSByte] = typeof(SByte?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int16] = typeof(Int16);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt16] = typeof(Int16?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int32] = typeof(Int32);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt32] = typeof(Int32?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int64] = typeof(Int64);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt64] = typeof(Int64?);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.BooleanArray] = typeof(Boolean[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.ByteArray] = typeof(Byte[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int16Array] = typeof(Int16[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int32Array] = typeof(Int32[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Int64Array] = typeof(Int64[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableBooleanArray] = typeof(Boolean?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableByteArray] = typeof(Byte?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt16Array] = typeof(Int16?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt32Array] = typeof(Int32?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableInt64Array] = typeof(Int64?[]);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Byte] = typeof(Byte);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableByte] = typeof(Byte?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt16] = typeof(UInt16);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt16] = typeof(UInt16?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt32] = typeof(UInt32);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt32] = typeof(UInt32?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt64] = typeof(UInt64);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt64] = typeof(UInt64?);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.SByteArray] = typeof(SByte[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt16Array] = typeof(UInt16[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt32Array] = typeof(UInt32[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.UInt64Array] = typeof(UInt64[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableSByteArray] = typeof(SByte?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt16Array] = typeof(UInt16?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt32Array] = typeof(UInt32?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableUInt64Array] = typeof(UInt64?[]);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Half] = typeof(Half);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableHalf] = typeof(Half?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Single] = typeof(Single);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableSingle] = typeof(Single?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Double] = typeof(Double);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDouble] = typeof(Double?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Decimal] = typeof(Decimal);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDecimal] = typeof(Decimal?);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.SingleArray] = typeof(Single[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.DoubleArray] = typeof(Double[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.DecimalArray] = typeof(Decimal[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableSingleArray] = typeof(Single?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDoubleArray] = typeof(Double?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDecimalArray] = typeof(Decimal?[]);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.DateTime] = typeof(DateTime);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDateTime] = typeof(DateTime?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.TimeSpan] = typeof(TimeSpan);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableTimeSpan] = typeof(TimeSpan?);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.DateTimeArray] = typeof(DateTime[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.TimeSpanArray] = typeof(TimeSpan[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableDateTimeArray] = typeof(DateTime?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableTimeSpanArray] = typeof(TimeSpan?[]);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.BitArray] = typeof(BitArray);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.BitVector32] = typeof(BitVector32);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableBitVector32] = typeof(BitVector32?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Guid] = typeof(Guid);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableGuid] = typeof(Guid?);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.BitVector32Array] = typeof(BitVector32[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.GuidArray] = typeof(Guid[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableBitVector32Array] = typeof(BitVector32?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableGuidArray] = typeof(Guid?[]);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Char] = typeof(Char);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableChar] = typeof(Char?);
			PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Binary] = typeof(Byte[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.CharArray] = typeof(Char[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.NullableCharArray] = typeof(Char?[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.StringArray] = typeof(String[]);
			//PropertyTypesByPropertyTypeId[(int)PropertyTypeId.Type] = typeof(Type);


			for (int i = 0; i < PropertyTypesByPropertyTypeId.Length; i++)
			{
				Type? type = PropertyTypesByPropertyTypeId[i];

				if (type != null)
					PropertyTypeIdsByType.Add(type, i);
			}
		}

		public static int Count
		{
			get { return PropertyTypesByPropertyTypeId.Length; }
		}

		public static int GetPropertyTypeId(Type propertyType)
		{
			int propertyTypeId;

			if (!PropertyTypeIdsByType.TryGetValue(propertyType, out propertyTypeId))
			{
				if (propertyType.IsEnum || propertyType.GenericTypeArguments.Count() > 0 && propertyType.GenericTypeArguments[0].IsEnum) // propertyType.IsEnum)
				{
					if (Nullable.GetUnderlyingType(propertyType) != null) // it is nullable enum
						propertyTypeId = (int)PropertyTypeId.NullableInt32;
					else
						propertyTypeId = (int)PropertyTypeId.Int32;
				}
				else
				{
					propertyTypeId = -1;
				}
			}

			return propertyTypeId;
		}

		public static Type GetPropertyType(PropertyTypeId typeId)
		{
			return GetPropertyType((int)typeId);
		}

		public static Type GetPropertyType(int propertyTypeId)
		{
			return PropertyTypesByPropertyTypeId[propertyTypeId];
		}

		public static bool Contains(int propertyTypeId)
		{
			return propertyTypeId >= 0 && propertyTypeId < PropertyTypesByPropertyTypeId.Length;
		}

		public static bool Contains(Type type)
		{
			return PropertyTypeIdsByType.ContainsKey(type);
		}

		public static string GetTypeName(Type propertyType)
		{
			return ReflectionHelper.GetTypeName(propertyType);
		}
	}
}
