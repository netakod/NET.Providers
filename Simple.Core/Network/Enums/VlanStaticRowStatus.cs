using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Network
{
    public enum VlanStaticRowStatus
    {
        Active = 1,
        NotInService = 2,
        NotReady = 3,
        CreateAndGo = 4,
        CreateAndWait = 5,
        Destroy = 6
    }
}
