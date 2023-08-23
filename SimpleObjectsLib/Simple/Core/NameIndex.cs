using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class NameIndex
    {
        public NameIndex(int index, string name)
            : this(index, name, null)
        {
        }

        public NameIndex(int index, string name, string description)
        {
            this.Index = index;
            this.Name = name;
            this.Description = description;
        }

        public int Index { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
