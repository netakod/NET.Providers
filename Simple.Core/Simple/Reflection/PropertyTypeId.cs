using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public enum PropertyTypeId
	{
		String = 0,

		Boolean = 1,
		NullableBoolean = 2,
		SByte = 3,
		NullableSByte = 4,
		Int16 = 5,
		NullableInt16 = 6,
		Int32 = 7,
		NullableInt32 = 8,
		Int64 = 9,
		NullableInt64 = 10,

		//BooleanArray = 6,
		//ByteArray = 7,
		//Int16Array = 8,
		//Int32Array = 9,
		//Int64Array = 10,


		//NullableBooleanArray = 16,
		//NullableByteArray = 17,
		//NullableInt16Array = 18,
		//NullableInt32Array = 19,
		//NullableInt64Array = 20,

		Byte = 11,
		NullableByte = 12,
		UInt16 = 13,
		NullableUInt16 = 14,
		UInt32 = 15,
		NullableUInt32 = 16,
		UInt64 = 17,
		NullableUInt64 = 18,

		//SByteArray = 25,
		//UInt16Array = 26,
		//UInt32Array = 27,
		//UInt64Array = 28,


		//NullableSByteArray = 33,
		//NullableUInt16Array = 34,
		//NullableUInt32Array = 35,
		//NullableUInt64Array = 36,

		Half = 19,
		NullableHalf = 20,
		Single = 21,
		NullableSingle = 22,
		Double = 23,
		NullableDouble = 24,
		Decimal = 25,
		NullableDecimal = 26,

		//SingleArray = 40,
		//DoubleArray = 41,
		//DecimalArray = 42,


		//NullableSingleArray = 46,
		//NullableDoubleArray = 47,
		//NullableDecimalArray = 48,

		DateTime = 27,
		NullableDateTime = 28,
		TimeSpan = 29,
		NullableTimeSpan = 30,
		//DateTimeArray = 51,
		//TimeSpanArray = 52,

		//NullableDateTimeArray = 55,
		//NullableTimeSpanArray = 56,


		BitArray = 31,
		BitVector32 = 32,
		NullableBitVector32 = 33,
		Guid = 34,
		NullableGuid = 35,
		//BitVector32Array = 59,
		//GuidArray = 60,

		//NullableBitVector32Array = 63,
		//NullableGuidArray = 64,

		Char = 36,
		NullableChar = 37,

		Binary = 38,

		//CharArray = 68,
		//NullableCharArray = 69,
		//StringArray = 70,

		//Type = 39,
		//ArrayList = 73,
	}
}
