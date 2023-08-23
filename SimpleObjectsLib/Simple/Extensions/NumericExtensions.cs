using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Simple
{
	public static class NumericExtensions
	{
		public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
		{
			typeof(int),  typeof(double),  typeof(decimal),
			typeof(long), typeof(short),   typeof(sbyte),
			typeof(byte), typeof(ulong),   typeof(ushort),
			typeof(uint), typeof(float),   typeof(BigInteger)
		};

		/// <summary>
		/// Returns whether the bit at the specified position is set.
		/// </summary>
		/// <typeparam name="T">Any integer type.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <param name="position">The position of the bit to check, 0 refers to the least significant bit.</param>
		/// <returns>true if the specified bit is on, otherwise false.</returns>
		//[CLSCompliant(false)]
		public static bool IsBitSet<T>(this T value, int position) where T : struct, IConvertible
		{
			var int64Value = value.ToInt64(CultureInfo.CurrentCulture);

			return (int64Value & (1 << position)) != 0;
		}

		///// <summary>
		///// Determines if the given object is numeric type.
		///// </summary>
		///// <param name="obj">The object to verify.</param>
		///// <returns>True if the given object is numeric, False if not.</returns>
		//public static bool IsNumeric(this object obj) => obj is byte || obj is sbyte || obj is ushort || obj is uint || obj is ulong ||
		//												 obj is short || obj is int || obj is long || 
		//												 obj is float || obj is double || obj is decimal || obj is BigInteger;

		/// <summary>
		/// Determines if the given Type is numeric type.
		/// </summary>
		/// <typeparam name="T">Type T</typeparam>
		/// <param name="type">Type to verify.</param>
		/// <returns>True if the given type is numeric, False if not.</returns>
		public static bool IsNumeric(this Type type)
		{
			return (type != null) ? NumericTypes.Contains(type) : false;

			//return NumericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
		}
	}
}
