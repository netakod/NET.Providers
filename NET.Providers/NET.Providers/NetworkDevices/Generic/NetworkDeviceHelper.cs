using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace NET.Tools.Providers
{
	public static class NetworkDeviceHelper
	{
		public static int GetVlanIdFromVlanInterfaceName(string vlanInterfaceName)
		{
			const string strVlan = "vlan";
			int vlanId = 0;

			if (vlanInterfaceName.ToLower().TrimStart().StartsWith(strVlan))
			{
				string strVlanId = vlanInterfaceName.Replace(strVlan, "").Trim();
				
				vlanId = Conversion.TryChangeType<int>(strVlanId);
			}

			return vlanId;
		}
	}
}
