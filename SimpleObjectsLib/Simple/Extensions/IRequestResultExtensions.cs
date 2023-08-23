using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
	public static class IRequestResultExtension
	{
		public static IRequestResult<T> ToCustom<T>(this IRequestResult requestResult)
		{
			return new CustomRequestResult<T>(requestResult);
		}

		public static IRequestResult<T> ToCustom<T>(this IRequestResult requestResult, T value)
		{
			return new CustomRequestResult<T>(requestResult, value);
		}

		public static IRequestResult<T> ToCustom<T>(this IRequestResult requestResult, T value, T defaultValue)
		{
			return new CustomRequestResult<T>(requestResult, value, defaultValue);
		}

		public static T ToCustomValue<T>(this IRequestResult requestResult)
		{
			return Conversion.TryChangeType<T>(requestResult.ResultValue);
		}

		public static T ToCustomValue<T>(this IRequestResult requestResult, T defaultValue)
		{
			return Conversion.TryChangeType<T>(requestResult.ResultValue, defaultValue);
		}
	}
}