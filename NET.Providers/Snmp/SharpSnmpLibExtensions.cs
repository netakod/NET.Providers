using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace NET.Tools.Snmp
{
	public static class SharpSnmpLibExtensions
	{
		public static SnmpData ToSnmpData(this IEnumerable<Variable> variables, string oid)
		{
			return (variables.Count() > 0) ? new SnmpData(oid, (SnmpObjectValueType)variables.ElementAt(0).Data.TypeCode, variables.ElementAt(0).Data.ToString())
										   : new SnmpData(oid, SnmpObjectValueType.OctetString, String.Empty);
		}

		public static SnmpData ToSnmpData(this ISnmpMessage snmpMessage)
		{
			return snmpMessage.Pdu().ToSnmpData();
		}

		public static SnmpData ToSnmpData(this ISnmpPdu snmpPdu)
		{
			string oid = snmpPdu.Variables[0].Id.ToString();
			SnmpObjectValueType objectType = (SnmpObjectValueType)snmpPdu.Variables[0].Data.TypeCode;
			string value = snmpPdu.Variables[0].Data.ToString();

			return new SnmpData(oid, objectType, value) { ErrorCode = (SnmpErrorCode)snmpPdu.ErrorStatus.ToInt32() };
		}

		public static SnmpData[] ToSnmpDataArray(this IEnumerable<Variable> variables)
		{
			SnmpData[] result = new SnmpData[variables.Count()];

			for (int i = 0; i < result.Length; i++)
				result[i] = variables.ElementAt(i).ToSnmpData();

			return result;
		}

		public static SnmpData ToSnmpData(this Variable variable)
		{
			SnmpData result = SnmpData.Empty;
			
			if (variable != null)
				result = new SnmpData(variable.Id.ToString(), (SnmpObjectValueType)variable.Data.TypeCode, variable.Data.ToString());

			return result;
		}
	}
}
