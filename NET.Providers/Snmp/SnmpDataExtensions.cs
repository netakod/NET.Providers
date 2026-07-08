using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace NET.Tools.Snmp
{
	public static class SnmpDataExtensions
    {
        public static IRequestResult ToRequestResult(this SnmpData snmpData)
        {
            return CreateRequestResult(snmpData, null);
        }

        public static IRequestResult ToRequestResult(this SnmpData snmpData, object defaultValue)
        {
            return CreateRequestResult(snmpData, defaultValue);
        }

        public static IRequestResult<T> ToRequestResult<T>(this SnmpData snmpData)
        {
            return CreateRequestResult(snmpData, default(T)).ToCustom<T>();
        }

        public static IRequestResult<T> ToRequestResult<T>(this SnmpData snmpData, T defaultValue)
        {
            return CreateRequestResult(snmpData, defaultValue).ToCustom<T>(defaultValue);
        }

        private static IRequestResult CreateRequestResult(SnmpData snmpData, object defaultValue)
        {
			object resultValue;
			TaskResultInfo actionResult = TaskResultInfo.Succeeded;

			//RequestResult requestResult = new RequestResult(snmpData.Value, RequestActionResult.RequestSucceeded);

			switch (snmpData.ObjectValueType)
			{
				case SnmpObjectValueType.Integer32:

					resultValue = snmpData.ToInt32();
					break;

				case SnmpObjectValueType.Counter32 | SnmpObjectValueType.Gauge32 | SnmpObjectValueType.UnsignedInteger32:

					resultValue = snmpData.ToUInt32();
					break;

				case SnmpObjectValueType.Counter64:

					resultValue = snmpData.ToUInt32();
					break;

				case SnmpObjectValueType.Null:

					resultValue = defaultValue;
					break;

				case SnmpObjectValueType.NoSuchObject | SnmpObjectValueType.NoSuchInstance:

					resultValue = snmpData.Value;
					actionResult = TaskResultInfo.NoSuchData;
					break;

				default :

					resultValue = (snmpData.Value == null) ? snmpData.ToString() : null;
					break;

					//requestResult.ResultValue = Conversion.TryChangeType(snmpData.Value, snmpData.DeclaredType);
            }

            return new RequestResult<object>(resultValue, actionResult);
        }
    }
}
