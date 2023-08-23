using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public interface IModuleProperty : IPropertyValue
    {
        IModuleProperty GetModule(string moduleName);
    }
}
