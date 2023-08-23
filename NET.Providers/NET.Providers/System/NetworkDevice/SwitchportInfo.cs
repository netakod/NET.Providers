using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Network;

namespace NET.Tools.Providers
{
	public struct SwitchportInfo
	{
		public SwitchportInfo(InterfaceSwitchportMode switchportMode, int vlanId)
		{
			this.SwitchportMode = switchportMode;
			this.VlanId = vlanId;
		}

		public InterfaceSwitchportMode SwitchportMode { get; private set; }
		public int VlanId { get; private set; }
	}
}
