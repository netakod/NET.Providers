using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Network;

namespace NET.Tools.Providers
{
    public class TrunkPorts
    {
        private List<string> trunkPortInterfaceNames = null;
        private NetworkDeviceProviderInterfacesGeneric interfaces = null;

        public TrunkPorts(NetworkDeviceProviderInterfacesGeneric interfaces)
        {
            this.interfaces = interfaces;
        }
        
        public async ValueTask<List<string>> GetTrunkPortInterfaceNames()
        {
            if (this.trunkPortInterfaceNames == null)
            {
                this.trunkPortInterfaceNames = new List<string>();
                var interfaceIndexesByInterfaceName = await this.interfaces.GetInterfaceIndexesByInterfaceName();

                foreach (string interfaceName in interfaceIndexesByInterfaceName.Keys)
                {
                    var switchportInfo = await this.interfaces.GetSwitchportInfo(interfaceName);

                    if (switchportInfo.SwitchportMode == InterfaceSwitchportMode.Trunk)
                        trunkPortInterfaceNames.Add(interfaceName);
                }
            }

            return this.trunkPortInterfaceNames;
        }

        public async ValueTask UpdateTrunkPortInterfaceNames(string interfaceName)
        {
			var switchportInfo = await this.interfaces.GetSwitchportInfo(interfaceName);

			this.UpdateTrunkPortInterfaceNames(interfaceName, switchportInfo.SwitchportMode);
        }

        public void UpdateTrunkPortInterfaceNames(string interfaceName, InterfaceSwitchportMode switchportMode)
        {
            if (this.trunkPortInterfaceNames != null)
            {
                if (switchportMode == InterfaceSwitchportMode.Access || switchportMode == InterfaceSwitchportMode.DoubleTagging)
                {
                    if (this.trunkPortInterfaceNames.Contains(interfaceName))
                        this.trunkPortInterfaceNames.Remove(interfaceName);
                }
                else if (switchportMode == InterfaceSwitchportMode.Trunk)
                {
                    if (!this.trunkPortInterfaceNames.Contains(interfaceName))
                        this.trunkPortInterfaceNames.Add(interfaceName);
                }
            }
        }

        public async ValueTask InterfaceAdded(string interfaceName)
        {
            if (this.trunkPortInterfaceNames != null)
            {
				var switchportInfo = await this.interfaces.GetSwitchportInfo(interfaceName);

				if (switchportInfo.SwitchportMode == InterfaceSwitchportMode.Trunk)
                    trunkPortInterfaceNames.Add(interfaceName);
            }
        }

        public void InterfaceRemoved(string interfaceName)
        {
            if (this.trunkPortInterfaceNames != null && this.trunkPortInterfaceNames.Contains(interfaceName))
                trunkPortInterfaceNames.Remove(interfaceName);
        }
    }
}
