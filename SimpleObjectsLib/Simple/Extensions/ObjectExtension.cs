using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class ObjectExtension
	{
		// TODO: Remove ValueToString from object extension
		public static string ValueToString(this object value) => Conversion.ToString(value);

		public static bool IsDefault<T>(this T value) => (value is null) ? default(T) is null 
																		 : value.Equals(default(T));
	}
}
