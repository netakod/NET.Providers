using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Snmp;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Generic)]
    public class NetworkDeviceProviderVlansGeneric : NetworkDeviceProviderVlans, INetworkDeviceProviderVlans
    {
		public override bool IsVlanSupported()
        {
            return true;
        }

        public override async ValueTask<IEnumerable<VlanInfo>> GetVlans()
        {
            List<VlanInfo> result = new List<VlanInfo>();

            string vlanOid = SnmpOIDs.Vlans.dot1qVlanStaticName;
			SnmpData snmpData = await this.Provider.Snmp.GetNextAsync(vlanOid);

            while (snmpData.OID.Contains(vlanOid) && snmpData.ObjectValueType != SnmpObjectValueType.EndOfMibView)
            {
				if (snmpData.OID.Length > vlanOid.Length)
				{
					int vlanId = Conversion.TryChangeType<int>(snmpData.OID.Substring(vlanOid.Length + 1));
					string vlanName = await this.GetName(vlanId);

					result.Add(new VlanInfo(vlanId, vlanName));
				}

				string oid = snmpData.OID;
				snmpData = await this.Provider.Snmp.GetNextAsync(oid);

				//if (!snmpData.Succeed)
				//	return result;

				if (snmpData.OID == oid) // GetNext gives the same oid - does not move to the next oid 
					return result;

				//snmpData = snmpData.ResultValue;
            }

            return result;
        }

        public override async ValueTask Add(int vlanId, string name)
        {
            bool isSet = await this.Provider.Snmp.SetAsync(SnmpOIDs.Vlans.dot1qVlanStaticRowStatus + "." + vlanId, Convert.ToInt32(VlanStaticRowStatus.CreateAndGo));
			await this.SetName(vlanId, name);
			//if (!isSet.Succeed)
			//	throw new ProviderInfoException(isSet.Message);
		}

		public override async ValueTask Remove(int vlanId)
        {
			//TODO: Prije brisanja treba maknuti dotični vlan iz svih portova kojima pripada.

			bool isSet = await this.Provider.Snmp.SetAsync(SnmpOIDs.Vlans.dot1qVlanStaticRowStatus + "." + vlanId, Convert.ToInt32(VlanStaticRowStatus.Destroy));

			//if (!result.Succeed)
			//	throw new ProviderInfoException(result.Message);
		}

		public override async ValueTask<string> GetName(int vlanId)
        {
			SnmpData snmpData = await this.Provider.Snmp.GetAsync(SnmpOIDs.Vlans.dot1qVlanStaticName + "." + vlanId);

			//if (snmpData.Succeed)
			//{
				return snmpData.Value;
			//}
			//else
			//{
			//	throw new ProviderInfoException(snmpData.Message);
			//}
        }

        public override async ValueTask SetName(int vlanId, string vlanName)
        {
            string valueToSet = vlanName.IsNullOrEmpty() ? " " : vlanName.Trim().Replace(' ', '_');
			bool isSet = await this.Provider.Snmp.SetAsync(SnmpOIDs.Vlans.dot1qVlanStaticName + "." + vlanId, valueToSet);

			//if (!isSet)
			//	throw new ProviderInfoException(result.Message);
		}
	}
}
