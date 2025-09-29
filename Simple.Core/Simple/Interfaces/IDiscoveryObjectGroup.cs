using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public interface IDiscoveryItemGroup : IDiscoveryItem
    {
        IEnumerable Collection { get; }
        Type ObjectType { get; }
    }
}
