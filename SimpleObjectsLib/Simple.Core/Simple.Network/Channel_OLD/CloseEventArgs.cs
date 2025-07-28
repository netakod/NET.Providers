using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Network
{
    public class CloseEventArgs : EventArgs
    {
        public CloseReason Reason { get; private set; }

        public CloseEventArgs(CloseReason reason) => this.Reason = reason;
    }
}
