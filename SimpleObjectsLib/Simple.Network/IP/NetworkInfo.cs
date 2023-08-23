using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Network
{
    public class NetworkInfo
    {
        private string ipSubnetMask; // = String.Empty;
        private int subnetMaskPrefix; // = 0;

        public NetworkInfo(string ipAddress, string ipSubnetMask)
        {
            this.ipSubnetMask = String.Empty;
            this.subnetMaskPrefix = 0;

            this.IpAddressText = ipAddress;
            this.SubnetMask = ipSubnetMask;
            this.SubnetMaskPrefix = IpHelper.GetSubnetPrefix(ipSubnetMask);
        }

        public NetworkInfo(string ipAddress, int subnetMaskPrefix)
        {
            this.ipSubnetMask = String.Empty;
            this.subnetMaskPrefix = 0;
            
            this.IpAddressText = ipAddress;
            this.SubnetMask = IpHelper.GetSubnetMask(subnetMaskPrefix);
            this.subnetMaskPrefix = subnetMaskPrefix;
        }

        //public static readonly NetworkInfo Empty = new NetworkInfo("0.0.0.0", 0);

        public string IpAddressText { get; set; }

        public string SubnetMask
        {
            get { return this.ipSubnetMask; }
            set
            {
                this.ipSubnetMask = value;
                this.subnetMaskPrefix = IpHelper.GetSubnetPrefix(value);
            }
        }

        public int SubnetMaskPrefix
        {
            get { return this.subnetMaskPrefix; }
            set
            {
                this.subnetMaskPrefix = value;
                this.ipSubnetMask = IpHelper.GetSubnetMask(value);
            }
        }

        public string GetNetworkIpAddress()
        {
            return IpHelper.GetNetworkIpAddress(this.IpAddressText, this.SubnetMaskPrefix);
        }


        public string GetBroadcastIpAddress()
        {
            return IpHelper.GetBroadcastIpAddress(this.IpAddressText, this.SubnetMask);
        }

        public bool IsInSameSubnet(string ipAddressSameSubnetCandidate)
		{
            return IpHelper.IsInSameSubnet(this.IpAddressText, this.SubnetMask, ipAddressSameSubnetCandidate);
		}

        public bool Validate()
        {
            return IpHelper.ValidateIpAddress(this.IpAddressText) && this.SubnetMaskPrefix >= 0 && this.SubnetMaskPrefix <= 32;
        }

		public override string ToString()
		{
			return IpHelper.CreateNetworkIpAddressText(this.IpAddressText, this.SubnetMaskPrefix);
		}

        public override bool Equals(object obj)
        {
            if (obj is NetworkInfo)
                return this.Equals((NetworkInfo)obj);

            return false;
        }

		public override int GetHashCode() => base.GetHashCode();

		public bool Equals(NetworkInfo? ipAddressInfo) => ipAddressInfo != null && this == ipAddressInfo;

        public static bool operator ==(NetworkInfo? a, NetworkInfo? b) => a != null && b != null && a.SubnetMask == b.SubnetMask && a.SubnetMaskPrefix == b.SubnetMaskPrefix;

        public static bool operator !=(NetworkInfo? a, NetworkInfo? b) => !(a == b);
    }
}
