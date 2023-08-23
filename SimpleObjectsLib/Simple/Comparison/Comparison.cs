using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Simple
{
    public class Comparison
    {
        public static bool CompareTypes(Type type1, Type type2)
        {
            return CompareTypes(type1, type2, TypeComparisonCriteria.SameType);
        }

        public static bool CompareTypes(Type type1, Type type2, TypeComparisonCriteria comparisonCriteria)
        {
            if (type1.Equals(type2))
            {
                return true;
            }

            bool comparison = false;

            switch (comparisonCriteria)
            {
                case TypeComparisonCriteria.Subclass:
                    
					comparison = type1.IsSubclassOf(type2) || type2.IsSubclassOf(type1);
                    
					break;

                case TypeComparisonCriteria.Instance:
                    
					object? obj1 = Activator.CreateInstance(type1);
                    object? obj2 = Activator.CreateInstance(type2);
                    
					comparison = type1.IsInstanceOfType(obj2) || type2.IsInstanceOfType(obj1);
                    obj1 = obj2 = null;
                    
					break;
            }

            return comparison;
        }

        public static bool IsEmpty(object value)
        {
            bool result = false;

            if (value == null)
            {
                result = true;
            }
            else if (value.GetType() == typeof(string) && ((string)value ?? String.Empty).Trim().Length == 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

		public static bool IsInteger(object valueObject)
		{
			if (valueObject == null)
				return false;

			int value = Conversion.TryChangeType<int>(valueObject);

			if (value == 0 && valueObject.ToString()?.Trim() != "0")
				return false;

			return true;
		}

		public new static bool Equals(object? value1, object? value2) => IsEqual(value1, value2);

		public static bool Equals(object? value1, object? value2, bool trimBeforeStringComparison) => IsEqual(value1, value2, trimBeforeStringComparison);

		public static bool IsEqual(object? value1, object? value2)
        {
            return IsEqual(value1, value2, trimBeforeStringComparison: false); // trimBeforeComparisonIfTypeIsString must be set explicitly to true.
        }

        public static bool IsEqual(object? value1, object? value2, bool trimBeforeStringComparison)
        {
            Type type1, type2;

            if (value1 == null && value2 == null)
                return true;

            if ((value1 == null && value2 != null && value2.GetType() == typeof(string) && ((string)value2).Trim().Length == 0) ||
                (value2 == null && value1 != null && value1.GetType() == typeof(string) && ((string)value1).Trim().Length == 0))
                return true;

            if (value1 == null || value2 == null)
                return false;

            type1 = value1.GetType();
            type2 = value2.GetType();

			if (type1 != type2)
			{
				if (type1.IsEnum)
					return Enum.Parse(type1, value2.ToString() ?? String.Empty).Equals(value1);

				if (type2.IsEnum)
					return Enum.Parse(type2, value1.ToString() ?? String.Empty).Equals(value2);

				return ((type1 == typeof(string) && type2 == typeof(DBNull) && ((string)value1).Trim().Length == 0) ||
						(type2 == typeof(string) && type1 == typeof(DBNull) && ((string)value2).Trim().Length == 0));
			}

            if (type1 == typeof(int))
				return (int)value1 == (int)value2;

            if (type1 == typeof(string))
            {
                if (trimBeforeStringComparison)
                    return ((string)value1).Trim().Equals(((string)value2).Trim());
                else
					return ((string)value1).Equals(((string)value2));
			}

            if (type1 == typeof(DBNull))
                return type2 == typeof(DBNull);

            if (value1 == null)
                return value2 == null;

            return value1.Equals(value2);
        }

		public static bool IsEqual<T>([MaybeNull] T value1, [MaybeNull] T value2)
		{
			return IsEqual(value1, value2, trimBeforeComparisonIfTypeIsString: false); // trimBeforeComparisonIfTypeIsString must be set explicitly to true.
		}

		public static bool IsEqual<T>([MaybeNull] T value1, [MaybeNull] T value2, bool trimBeforeComparisonIfTypeIsString)
		{
			return IsEqual((object?)value1, (object?)value2, trimBeforeComparisonIfTypeIsString); // trimBeforeComparisonIfTypeIsString must be set explicitly to true.
		}

		//public static bool IsEqual<T>(T? value1, T? value2, bool trimBeforeComparisonIfTypeIsString)
		//{
		//	Type type1, type2;

		//	if (value1 == null && value2 == null)
		//		return true;

		//	if ((value1 == null && value2 != null && value2.GetType() == typeof(string) && (value2.ToString() ?? String.Empty).Trim().Length == 0) ||
		//		(value2 == null && value1 != null && value1.GetType() == typeof(string) && (value1.ToString() ?? String.Empty).Trim().Length == 0))
		//		return true;

		//	if (value1 == null || value2 == null)
		//		return false;

		//	type1 = value1.GetType();
		//	type2 = value2.GetType();

		//	if (type1 != type2)
		//	{
		//		if (type1.IsEnum)
		//			return Enum.Parse(type1, value2.ToString() ?? String.Empty).Equals(value1);

		//		if (type2.IsEnum)
		//			return Enum.Parse(type2, value1.ToString() ?? String.Empty).Equals(value2);

		//		return ((type1 == typeof(string) && type2 == typeof(DBNull) && (value1.ToString() ?? String.Empty).Trim().Length == 0) ||
		//				(type2 == typeof(string) && type1 == typeof(DBNull) && (value2.ToString() ?? String.Empty).Trim().Length == 0));
		//	}

		//	if (type1 == typeof(int))
		//		return value1.Equals(value2);

		//	if (type1 == typeof(string))
		//	{
		//		if (trimBeforeComparisonIfTypeIsString)
		//		{
		//			string str1 = (string)value1;
		//			string str2 = (string)value2;

		//			return str1.Trim() == str2.Trim();
		//		}
		//		else
		//		{
		//			return value1.Equals(value2);
		//		}
		//	}

		//	if (type1 == typeof(DBNull))
		//		return type2 == typeof(DBNull);

		//	if (value1 == null)
		//		return value2 == null;

		//	return value1.Equals(value2);
		//}

		/// <summary>
		/// Determines whether the specified byte arrays are equal.
		/// </summary>
		/// <param name="array1">The first byte array to compare.</param>
		/// <param name="array2">The socond byte array to compare.</param>
		/// <returns>true if array1 is the same as array2; otherwise, false.</returns>
		public static bool IsEqual(byte[] array1, byte[] array2)
		{
			if (array1.Length != array2.Length)
				return false;

			var length = array1.Length;
			var tailIdx = length - length % sizeof(Int64);

			//check in 8 byte chunks
			for (var i = 0; i < tailIdx; i += sizeof(Int64))
				if (BitConverter.ToInt64(array1, i) != BitConverter.ToInt64(array2, i))
					return false;

			//check the remainder of the array, always shorter than 8 bytes
			for (var i = tailIdx; i < length; i++)
				if (array1[i] != array2[i])
					return false;

			return true;
		}

		public static bool IsGreaterThanZero<T>(T value) where T : IComparable<T>
		{
			return value.CompareTo(default(T)) > 0;
		}

		public static bool IsGreaterThanZero(object value)
		{
			if (value != null && value.GetType().IsValueType)
				return System.Convert.ToDouble(value) > 0;
			
			return false;
		}

		public static bool SequenceEqual(byte[] array1, byte[] array2)
		{
			return array1.SequenceEqual<byte>(array2);
		}

		// Copyright (c) 2008-2013 Hafthor Stefansson
		// Distributed under the MIT/X11 software license
		// Ref: http://www.opensource.org/licenses/mit-license.php.
		private static unsafe bool UnsafeCompare(byte[] array1, byte[] array2)
		{
			if (array1 == null || array2 == null || array1.Length != array2.Length)
				return false;
			fixed (byte* p1 = array1, p2 = array2)
			{
				byte* x1 = p1, x2 = p2;
				int l = array1.Length;

				for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
					if (*((long*)x1) != *((long*)x2))
						return false;

				if ((l & 4) != 0)
				{
					if (*((int*)x1) != *((int*)x2))
						return false;

					x1 += 4;
					x2 += 4;
				}

				if ((l & 2) != 0)
				{
					if (*((short*)x1) != *((short*)x2))
						return false;

					x1 += 2;
					x2 += 2;
				}

				if ((l & 1) != 0)
					if (*((byte*)x1) != *((byte*)x2))
						return false;

				return true;
			}
		}

		public static unsafe bool EqualBytesLongUnrolled(byte[] array1, byte[] array2)
		{
			if (array1 == array2)
				return true;

			if (array1.Length != array2.Length)
				return false;

			fixed (byte* bytes1 = array1, bytes2 = array2)
			{
				int len = array1.Length;
				int rem = len % (sizeof(long) * 16);
				long* b1 = (long*)bytes1;
				long* b2 = (long*)bytes2;
				long* e1 = (long*)(bytes1 + len - rem);

				while (b1 < e1)
				{
					if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
						*(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
						*(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
						*(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
						*(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
						*(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
						*(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
						*(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
						return false;
					
					b1 += 16;
					b2 += 16;
				}

				for (int i = 0; i < rem; i++)
					if (array1[len - 1 - i] != array2[len - 1 - i])
						return false;

				return true;
			}
		}

		public static unsafe bool NewMemCmp(byte[] array1, byte[] array2)
		{
			if (array1 == array2)
				return true;

			if (array1.Length != array2.Length)
				return false;

			fixed (byte* bytes1 = array1, bytes2 = array2)
			{
				ulong* b1 = (ulong*)bytes1;
				ulong* b2 = (ulong*)bytes2;
				byte* lastAddr = bytes1 + array1.Length;
				byte* lastAddrMinus32 = lastAddr - 32;

				while (bytes1 < lastAddrMinus32) // unroll the loop so that we are comparing 32 bytes at a time.
				{
					if (*b1 != *b2)
						return false;

					if (*(b1 + 8) != *(b2 + 8))
						return false;

					if (*(b1 + 16) != *(b2 + 16))
						return false;

					if (*(b1 + 24) != *(b2 + 24))
						return false;

					b1 += 32;
					b2 += 32;
				}

				while (bytes1 < lastAddr)
				{
					if (*bytes1 != *bytes2)
						return false;

					b1++;
					b2++;
				}

				return true;
			}
		}
	}
}
