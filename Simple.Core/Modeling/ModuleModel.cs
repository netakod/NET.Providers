using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Modeling
{
    public class ModuleModel : ModelElement, IModuleModel
    {
        public ModuleModel()
        {
            this.ObjectType = typeof(void);
        }

        public Type ObjectType { get; set; }
    }

    public interface IModuleModel : IModelElement
    {
        Type ObjectType { get; }
    }
}
