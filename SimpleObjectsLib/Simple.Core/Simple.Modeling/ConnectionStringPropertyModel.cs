using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Modeling
{
    public class ConnectionStringPropertyModel : ModelElement, IConnectionStringPropertyModel
    {
        public bool ProtectByEncryption { get; set; }
    }

    public interface IConnectionStringPropertyModel : IModelElement
    {
        bool ProtectByEncryption { get; }
    }
}
