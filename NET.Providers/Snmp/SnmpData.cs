using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Simple;

namespace NET.Tools.Snmp
{
    public struct SnmpData
    {
        public static readonly SnmpData Empty = new SnmpData();

        public string OID { get; private set; }
        public SnmpObjectValueType ObjectValueType { get; private set; }
        public string Value { get; set; }
        public Type DeclaredType { get; set; }
		public SnmpErrorCode ErrorCode { get; set; }
        public SnmpRequestInfo RequestInfo { get; set; }

        public SnmpData(string oid, SnmpObjectValueType objectType, string value)
            : this(oid, objectType, value, null, SnmpErrorCode.NoError, SnmpRequestInfo.NoError)
        {
        }

        public SnmpData(string oid, SnmpObjectValueType objectType, string value, Type declaredType, SnmpErrorCode errorCode, SnmpRequestInfo requestInfo)
        {
			if (oid.Length > 0 && oid[0] == '.')
				oid = oid.Substring(1);

			this.OID = oid;
            this.ObjectValueType = objectType;
            this.Value = value;
            this.DeclaredType = declaredType;
            this.ErrorCode = errorCode;
            this.RequestInfo = requestInfo;
        }

        public int ToInt32()
        {
            int value = 0;

            if (this.ObjectValueType == SnmpObjectValueType.Integer32)
            {
                value = Convert.ToInt32(this.Value);
            }
            //else if (!(snmpData.ObjectType == SnmpObjectType.NoSuchObject || snmpData.ObjectType == SnmpObjectType.NoSuchInstance ||
            //           snmpData.ObjectType == SnmpObjectType.Null))
            //{
            //    throw new Exception("Error in converting SnmpData ObjectType " + snmpData.ObjectType.ToString() + " to Int32.");
            //}

            return value;

            //return snmpData.ObjectType == SnmpObjectType.Counter32 || snmpData.ObjectType == SnmpObjectType.Gauge32 || 
            //       snmpData.ObjectType == SnmpObjectType.UnsignedInteger32 ? Convert.ToUInt32(snmpData.Value) : 0;
        }

        public uint ToUInt32()
        {
            uint value = 0;

            if (this.ObjectValueType == SnmpObjectValueType.Integer32 || this.ObjectValueType == SnmpObjectValueType.Counter32 ||
                this.ObjectValueType == SnmpObjectValueType.Gauge32 || this.ObjectValueType == SnmpObjectValueType.UnsignedInteger32)
            {
                value = Convert.ToUInt32(this.Value);
            }
            //else if (!(snmpData.ObjectType == SnmpObjectType.NoSuchObject || snmpData.ObjectType == SnmpObjectType.NoSuchInstance ||
            //           snmpData.ObjectType == SnmpObjectType.Null))
            //{
            //    throw new Exception("Error in converting SnmpData ObjectType " + snmpData.ObjectType.ToString() + " to UInt32.");
            //}

            return value;

            //return snmpData.ObjectType == SnmpObjectType.Counter32 || snmpData.ObjectType == SnmpObjectType.Gauge32 || 
            //       snmpData.ObjectType == SnmpObjectType.UnsignedInteger32 ? Convert.ToUInt32(snmpData.Value) : 0;
        }

        public ulong ToUInt64()
        {
            ulong value = 0;

            if (this.ObjectValueType == SnmpObjectValueType.Counter64)
            {
                value = Convert.ToUInt64(this.Value);
            }
            //else if (!(snmpData.ObjectType == SnmpObjectType.NoSuchObject || snmpData.ObjectType == SnmpObjectType.NoSuchInstance ||
            //           snmpData.ObjectType == SnmpObjectType.Null))
            //{
            //    throw new Exception("Error in converting SnmpData ObjectType " + snmpData.ObjectType.ToString() + " to UInt64.");
            //}

            return value;

            //return snmpData.ObjectType == SnmpObjectType.Counter64 ? Convert.ToUInt64(snmpData.Value) : 0;
        }

        public TimeSpan ToTimeSpan()
        {
            // TODO: Napraviti da ovo radi kak spada!!!

            return this.ObjectValueType == SnmpObjectValueType.TimeTicks ? TimeSpan.Parse(this.Value) : TimeSpan.Zero;
        }

        // Ovo vezano za ProviderData prebaciti kao extenzija u Simple.Providers !!!!!!!!!!!

        //public ProviderData ToProviderData<TOutputType>()
        //{
        //    ProviderData data = this.ToProviderData();

        //    try
        //    {
        //        object value = (TOutputType)data.Value;
        //        data = new ProviderData(typeof(TOutputType), value, data.DataStatus);
        //    }
        //    catch
        //    {
        //    }

        //    return data;
        //}

        public bool IsNullOrNoSuchObject()
        {
            if (this.ObjectValueType == SnmpObjectValueType.Null || this.ObjectValueType == SnmpObjectValueType.NoSuchObject ||
                this.ObjectValueType == SnmpObjectValueType.NoSuchInstance || this.ObjectValueType == SnmpObjectValueType.EndOfMibView)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public T ToCustom<T>()
        {
            return Conversion.TryChangeType<T>(this.Value);
        }

        public object ToCustom(object value, Type declaredType)
        {
            return Conversion.TryChangeType(value, declaredType);
        }

        public override string ToString()
        {
            if (this.IsNullOrNoSuchObject())
            {
                return String.Empty;
            }
            else
            {
                return this.Value;
            }
            //if (this.Value != null)
            //{
            //    return this.Value;
            //}
            //else
            //{
            //    return base.ToString();
            //}
        }
    }
}
