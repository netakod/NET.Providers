//(c) Vasian Cepa 2005
// Version 2
using System;
using System.Collections;
using System.Collections.Generic;

namespace Simple
{
    // emulates StrCmpLogicalW, but not fully
    public class StringLogicalComparer : IComparer<string>, IComparer
    {
        public static int Compare(object? a, object? b)
        {
            return CompareObject(a, b);
        }

        public static int Compare(string? a, string? b)
        {
            return CompareString(a, b);
        }

        public static int CompareObject(object? a, object? b)
        {
            return CompareString(Convert.ToString(a), Convert.ToString(b));
        }
        
        public static int CompareString(string? a, string? b)
        {
            try
            {
                //get rid of special cases
                if ((a == null) && (b == null))
                    return 0;
                else if (a == null)
                    return -1;
                else if (b == null)
                    return 1;

                if ((a.Equals(string.Empty) && (b.Equals(string.Empty))))
                    return 0;
                else if (a.Equals(string.Empty))
                    return -1;
                else if (b.Equals(string.Empty))
                    return -1;

                //WE style, special case
                bool sp1 = Char.IsLetterOrDigit(a, 0);
                bool sp2 = Char.IsLetterOrDigit(b, 0);

                if (sp1 && !sp2)
                    return 1;
                if (!sp1 && sp2)
                    return -1;

                int i1 = 0, i2 = 0; //current index
                int r = 0; // temp result

                while (true)
                {
                    bool c1 = Char.IsDigit(a, i1);
                    bool c2 = Char.IsDigit(b, i2);

                    if (!c1 && !c2)
                    {
                        bool letter1 = Char.IsLetter(a, i1);
                        bool letter2 = Char.IsLetter(b, i2);
                        if ((letter1 && letter2) || (!letter1 && !letter2))
                        {
                            if (letter1 && letter2)
                            {
                                r = Char.ToLower(a[i1]).CompareTo(Char.ToLower(b[i2]));
                            }
                            else
                            {
                                r = a[i1].CompareTo(b[i2]);
                            }
                            if (r != 0)
                                return r;
                        }
                        else if (!letter1 && letter2)
                            return -1;
                        else if (letter1 && !letter2)
                            return 1;
                    }
                    else if (c1 && c2)
                    {
                        r = CompareNum(a, ref i1, b, ref i2);
                        if (r != 0)
                            return r;
                    }
                    else if (c1)
                    {
                        return -1;
                    }
                    else if (c2)
                    {
                        return 1;
                    }

                    i1++;
                    i2++;

                    if ((i1 >= a.Length) && (i2 >= b.Length))
                    {
                        return 0;
                    }
                    else if (i1 >= a.Length)
                    {
                        return -1;
                    }
                    else if (i2 >= b.Length)
                    {
                        return -1;
                    }
                }
            }
            catch 
			{
                return 0;
			}
        }

        private static int CompareNum(string a, ref int ia, string b, ref int ib)
        {
            int nzStart1 = ia, nzStart2 = ib; // nz = non zero
            int end1 = ia, end2 = ib;

            ScanNumEnd(a, ia, ref end1, ref nzStart1);
            ScanNumEnd(b, ib, ref end2, ref nzStart2);
            
            int start1 = ia;
            
            ia = end1 - 1;
            
            int start2 = ib;
            
            ib = end2 - 1;

            int nzLength1 = end1 - nzStart1;
            int nzLength2 = end2 - nzStart2;

            if (nzLength1 < nzLength2)
                return -1;
            else if (nzLength1 > nzLength2)
                return 1;

            for (int j1 = nzStart1,j2 = nzStart2; j1 <= ia; j1++, j2++)
            {
                int r = a[j1].CompareTo(b[j2]);
                
                if (r != 0)
                    return r;
            }
            // the nz parts are equal
            int length1 = end1 - start1;
            int length2 = end2 - start2;
            
            if (length1 == length2)
                return 0;
            
            if (length1 > length2)
                return -1;
            
            return 1;
        }

        //lookahead
        private static void ScanNumEnd(string s, int start, ref int end, ref int nzStart)
        {
            bool countZeros = true;

            nzStart = start;
            end = start;

            while (Char.IsDigit(s, end))
            {
                if (countZeros && s[end].Equals('0'))
                    nzStart++;
                else
                    countZeros = false;
                
                end++;
                
                if (end >= s.Length)
                    break;
            }
        }

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than,
		///     equal to, or greater than the other.
		///
		/// Exceptions:
		///   T:System.ArgumentException:
		///     Neither x nor y implements the System.IComparable interface. -or- x and y are
		///     of different types and neither one can handle comparisons with the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A signed integer that indicates the relative values of x and y:
		///     - If less than 0, x is less than y.
		///     - If 0, x equals y.
		///     - If greater than 0, x is greater than y.</returns>
		int IComparer.Compare(object? x, object? y) => Compare(x, y);

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than,
		///     equal to, or greater than the other.
		/// Value – Meaning
		///     Less than zero –x is less than y.
		///     Zero –x equals y.
		///     Greater than zero –x is greater than y.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A signed integer that indicates the relative values of x and y, as shown in the
		///     following table.</returns>
		int IComparer<string>.Compare(string? x, string? y) => Compare(x, y);
	}
}
