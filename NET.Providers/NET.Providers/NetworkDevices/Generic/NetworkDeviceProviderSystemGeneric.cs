using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using NET.Tools.Snmp;
using Simple.Serialization;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Generic)]
    public class NetworkDeviceProviderSystemGeneric : NetworkDeviceProviderSystem, INetworkDeviceProviderSystem
    {
		public override async ValueTask<string> GetName()
        {
            SnmpData snmpData = await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysName));
            string result = snmpData.ToString();

            if (result != null)
                result = result.Trim();

            return result;
        }

        public override async ValueTask SetName(string name)
        {
            string valueToSet = name.IsNullOrEmpty() ? " " : name.Trim();

            try
            {
                await this.Provider.Snmp.SetAsync(String.Format("{0}.0", SnmpOIDs.System.sysName), valueToSet);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
        }

        public override ValueTask SetCommunity(string vlue)
        {
            

            // This is only temporary !!!!
            return new ValueTask();
            

            //throw new ProviderInfoException("SetCommunity is not implemented");
        }

		public override ValueTask<IEnumerable<ApplyPasswordDestination>> SetPassword(string value) => throw new ProviderInfoException("SetPassword is not implemented");

		public override async ValueTask<string> GetDescription()
        {
            string result = (await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysDescription))).ToString();

            if (result != null)
                result = result.Trim();

            return result;
        }

        public override async ValueTask<string> GetObjectID()
        {
            string result = (await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysObjectID))).ToString();

            if (result != null)
                result = result.Trim();

            return result;
        }

        public override async ValueTask<string> GetLocation()
        {
            string result = (await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation))).ToString();

            if (result != null)
                result = result.Trim();

            return result;
        }

        public override async ValueTask SetLocation(string location)
        {
            string valueToSet = location.IsNullOrEmpty() ? " " : location.Trim();

            try
            {
                await this.Provider.Snmp.SetAsync(String.Format("{0}.0", SnmpOIDs.System.sysLocation), valueToSet);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
        }

        public override async ValueTask<string> GetContact()
        {
            string result = (await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysContact))).ToString();

            if (result != null)
                result = result.Trim();

            return result;
        }

        public override async ValueTask SetContact(string contact)
        {
            string valueToSet = contact.IsNullOrEmpty() ? " " : contact.Trim();

            try
            {
                await this.Provider.Snmp.SetAsync(String.Format("{0}.0", SnmpOIDs.System.sysContact), valueToSet);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
        }

        public override async ValueTask<TimeSpan> GetUpTime()
        {
            return (await this.Provider.Snmp.GetAsync(String.Format("{0}.0", SnmpOIDs.System.sysUpTime))).ToTimeSpan();
        }
    }
}
