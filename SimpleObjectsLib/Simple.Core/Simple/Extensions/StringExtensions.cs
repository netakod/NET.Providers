using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
	public static class StringExtensions
	{
		public static bool IsNullOrEmpty(this string? value)
		{
			return String.IsNullOrEmpty(value);
		}

		//public static bool IsNotNullOrEmpty(this string? value)
		//{
		//	return !String.IsNullOrEmpty(value);
		//}

		public static bool IsNullOrWhiteSpace(this string? value)
		{
			return String.IsNullOrWhiteSpace(value);
		}

		public static double CheckSum(this string expression)
		{
			if (expression.Length == 0)
				return 0;

			double a, result = 1;

			for (int i = 0; i <= expression.Length - 1; i++)
			{
				a = (double)expression[i];
				result = (result + a * (i + 1)) - ((result - a) / 32);
			}

			result += (int)result * expression.Length;
			
			return result;
		}

		public static string InsertSpaceOnUpperChange(this string value) => InsertOnUpperChange(value, " ");

        public static string InsertOnUpperChange(this string value, string textToInsert)
        {
            string newText = "";
            bool isLastLower = false;

            for (int i = 0; i < value.Length; i++)
            {
                if (isLastLower && char.IsUpper(value, i))
                    newText += textToInsert;

                newText += value[i];
                isLastLower = char.IsLower(value, i) || char.IsNumber(value, i);
            }

            return newText;
        }

		//public static bool Contains(this string str, string text, bool ignoreCase)
		//      {
		//          return str.ToLower().Contains(text.ToLower());
		//      }

		//      public static bool Contains(this string str, IEnumerable<string> strings)
		//      {
		//          return Enumerable.Contains<string>(strings, str);
		//      }

		//      public static bool Contains(this string str, IEnumerable<string> strings, bool ignoreCase)
		//      {
		//          foreach (string text in strings)
		//          {
		//              if (ignoreCase)
		//              {
		//                  if (str.ToLower().Contains(text.ToLower()))
		//                      return true;
		//              }
		//              else
		//              {
		//                  if (str.Contains(text))
		//                      return true;
		//              }
		//          }

		//          return false;
		//      }

		//public static bool Contains(this string str, string value, StringComparison comparisonType) => str?.IndexOf(value, comparisonType) >= 0;

		public static bool Contains(this string str, string value, bool ignoreCase)
		{
			if (ignoreCase)
				return str.Contains(value, StringComparison.OrdinalIgnoreCase);
			else
				return str.Contains(value);
		}

		public static bool ContainsAll(this string str, params string[] strings)
		{
			foreach (string text in strings)
				if (!str.Contains(text))
					return false;

			return true;
		}

		public static bool ContainsAll(this string str, StringComparison comparisonType, params string[] strings)
		{
			foreach (string text in strings)
				if (!str.Contains(text, comparisonType))
					return false;

			return true;
		}

		public static bool ContainsAll(this string str, bool ignoreCase, params string[] strings)
		{
			foreach (string text in strings)
				if (!str.Contains(text, ignoreCase))
					return false;

			return true;
		}

		public static bool ContainsAny(this string str, params string[] strings)
		{
			foreach (string text in strings)
				if (str.Contains(text))
					return true;

			return false;
		}

		public static bool ContainsAny(this string str, StringComparison comparisonType, params string[] strings)
		{
			foreach (string text in strings)
				if (str.Contains(text, comparisonType))
					return true;

			return false;
		}

		public static bool ContainsAny(this string str, IEnumerable<string> strings, bool ignoreCase)
		{
			foreach (string text in strings)
				if (str.Contains(text, ignoreCase))
					return true;

			return false;
		}


		//public static bool ContainsAny(this string str, IEnumerable<string> strings)
  //      {
  //          return ContainsAny(str, strings, true);
  //      }

  //      public static bool ContainsAny(this string str, IEnumerable<string> strings, bool ignoreCase)
  //      {
  //          return CompareWithAny(str, strings, true, ignoreCase, (x, y) => x.Contains(y));
  //      }

        public static bool EndsWithAny(this string str, IEnumerable<string> endStrings)
        {
            return EndsWithAny(str, endStrings, true, true);
        }

        public static bool EndsWithAny(this string str, IEnumerable<string> endStrings, bool trim, bool ignoreCase)
        {
            return CompareWithAny(str, endStrings, trim, ignoreCase, (x, y) => x.EndsWith(y));
            
            //bool endsWithAny = false;

            //string val = trim ? str.Trim() : str;
            //val = ignoreCase ? val.ToLower() : val;

            //if (val.Length == 0)
            //{
            //    return false;
            //}

            //foreach (string compareString in endStrings)
            //{
            //    string compare = trim ? compareString.Trim() : compareString;
            //    compare = ignoreCase ? compare.ToLower() : compare;

            //    if (val.EndsWith(compare))
            //    {
            //        endsWithAny = true;
            //        break;
            //    }
            //}

            //return endsWithAny;
        }

		public static bool CompareWithAny(this string str, IEnumerable<string> strings, bool trim, bool ignoreCase, Func<string, string, bool> comparer)
        {
            bool result = false;
            string value = trim ? str.Trim() : str;
            
			value = ignoreCase ? value.ToLower() : value;

            if (value.Length == 0)
                return false;

            foreach (string compareString in strings)
            {
                string valueToCompare = trim ? compareString.Trim() : compareString;
                
				valueToCompare = ignoreCase ? valueToCompare.ToLower() : valueToCompare;

                if (comparer(value, valueToCompare))
                {
                    result = true;
                    
					break;
                }
            }

            return result;
        }

		public static string CapitalizeFirstLetter(this string str)
		{
			if (String.IsNullOrEmpty(str))
				return str;

			if (str.Length == 1)
				return str.ToUpper();

			return str.Remove(1).ToUpper() + str.Substring(1);
		}


		public static string UnCapitalizeFirstLetter(this string str)
		{
			if (String.IsNullOrEmpty(str))
				return str;

			if (str.Length == 1)
				return str.ToLower();

			return str.Remove(1).ToLower() + str.Substring(1);
		}

		public static string[] ToLines(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
		{
			return text.Split(new string[] { "\r\n", "\n\r", "\r", "\n", "\b" }, options);
			//return Regex.Split(text, "\r\n|\n\r|\n|\b", options);

		}

		public static string RemoveFirstLine(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
		{
			var lines = text.ToLines(options).Skip(1);
			string result = String.Join(Environment.NewLine, lines);

			return result;
		}

		public static string RemoveLastLine(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
		{
			var lines = text.ToLines(options);
			string result = String.Join(Environment.NewLine, lines.Take(lines.Length - 1));

			return result;
		}

		public static string Join(this IEnumerable<string> lines, string separator = "\r\n")
		{
			string result = String.Join(separator, lines);

			return result;
		}

		/// <summary>
		/// Provides a more natural way to call String.Format() on a string.
		/// </summary>
		/// <param name="args">An object array that contains zero or more objects to format</param>
		public static string FormatWith(this string s, params object[] args)
		{
			if (s == null) 
				return null;
			
			return String.Format(s, args);
		}

		/// <summary>
		/// Provides a more natural way to call String.Format() on a string.
		/// </summary>
		/// <param name="provider">An object that supplies the culture specific formatting</param>
		/// <param name="args">An object array that contains zero or more objects to format</param>
		public static string FormatWith(this string s, IFormatProvider provider, params object[] args)
		{
			if (s == null) 
				return null;
			
			return String.Format(provider, s, args);
		}

		public static void WriteLine(this string s, string lineText)
		{
			if (!lineText.IsNullOrEmpty())
			{
				if (!s.IsNullOrEmpty())
					s += Environment.NewLine; // "\r\n";

				s += lineText;
			}
		}
	}
}
