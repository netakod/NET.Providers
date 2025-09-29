using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class SubclassComparer : IEqualityComparer
    {
        public SubclassComparer()
        {
        }

        public new bool Equals(object x, object y)
        {
            if (x is Type && y is Type)
            {
                if (x.Equals(y))
                {
                    return true;
                }

                return ((Type)x).IsSubclassOf((Type)y) || ((Type)y).IsSubclassOf((Type)x);
            }

            if (x is Type)
            {
                return ((Type)x).IsSubclassOf(y.GetType()) || y.GetType().IsSubclassOf((Type)x);
            }

            if (y is Type)
            {
                return ((Type)y).IsSubclassOf(x.GetType()) || x.GetType().IsSubclassOf((Type)y);
            }

            return x.GetType().IsSubclassOf(y.GetType()) || y.GetType().IsSubclassOf(x.GetType());
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}
