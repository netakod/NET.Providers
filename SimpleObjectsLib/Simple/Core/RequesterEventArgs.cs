using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class RequesterEventArgs : EventArgs
    {
        public RequesterEventArgs(object requester)
        {
            this.Requester = requester;
        }

        public object Requester { get; private set; }
    }
}
