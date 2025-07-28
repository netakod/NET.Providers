using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Simple
{
    public class DictionaryEntryComparer : IComparer
    {
        private IComparer nc = null;

        public DictionaryEntryComparer(IComparer nc)
        {
            if (nc == null)
                throw new Exception("null IComparer");
            this.nc = nc;
        }

        public int Compare(object x, object y)
        {
            if ((x is DictionaryEntry) && (y is DictionaryEntry))
            {
                return nc.Compare(((DictionaryEntry)x).Key,
                    ((DictionaryEntry)y).Key);
            }
            return -1;
        }
    }//EOC
}
