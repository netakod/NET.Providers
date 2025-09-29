using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class EnumExtensions
	{
		public static int Max(this Enum enumType)
		{
			return Enum.GetValues(enumType.GetType()).Cast<int>().Max();
		}

		public static int Min(this Enum enumType)
		{
			return Enum.GetValues(enumType.GetType()).Cast<int>().Min();
		}

		public static string GetName(this Enum enumType)
		{
			return Enum.GetName(enumType.GetType(), enumType);
		}

		public static Dictionary<Enum, string> ToDictionary(this Enum enumType, bool insertSpaceOnUpperChane = true) // where T : struct, IConvertible
		{
			Dictionary<Enum, string> result = new Dictionary<Enum, string>();

			foreach (Enum value in Enum.GetValues(enumType.GetType()).Cast<Enum>())
			{
				Enum key = value;
				string name = Enum.GetName(enumType.GetType(), value);

				if (insertSpaceOnUpperChane)
					name = name.InsertSpaceOnUpperChange();

				result.Add(key, name);
			}

			return result;
		}

		public static Dictionary<T, string> ToDictionary<T>(this Enum enumType, bool insertSpaceOnUpperChane = true) where T : struct, IConvertible
		{
			Dictionary<T, string> result = new Dictionary<T, string>();

			foreach (T value in Enum.GetValues(enumType.GetType()).Cast<Enum>().Cast<T>())
			{
				T key = value;
				string name = Enum.GetName(enumType.GetType(), value);

				if (insertSpaceOnUpperChane)
					name = name.InsertSpaceOnUpperChange();

				result.Add(key, name);
			}

			return result;
		}
	}
}
