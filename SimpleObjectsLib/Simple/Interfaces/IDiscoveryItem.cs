using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public interface IDiscoveryItem
    {
        bool CanImport { get; set; }
        bool Import { get; set; }
    }
}
