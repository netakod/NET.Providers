using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class TypeExtension
	{
		public static bool IsNullable(this Type value)
		{
			return ReflectionHelper.IsNullable(value);
		}

		public static bool IsSameOrSubclassOf(this Type value, Type type)
		{
			return value.Equals(type) || value.IsSubclassOf(type);
		}

		public static int GetPropertyTypeId(this Type value)
		{
			return PropertyTypes.GetPropertyTypeId(value);
		}
	}
}
