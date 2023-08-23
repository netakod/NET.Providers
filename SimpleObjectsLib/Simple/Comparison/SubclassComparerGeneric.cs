using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class SubclassComparer<T> : SubclassComparer, IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return base.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return base.GetHashCode(obj);
        }
    }
}
