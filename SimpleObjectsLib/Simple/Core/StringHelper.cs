using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public static class StringHelper
    {
        public static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        public static Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        public static string MergeText(string delimiter, params string[] textList)
        {
            string result = String.Empty;

            foreach (string text in textList)
            {
                if (result.Length > 0 && !text.IsNullOrEmpty())
                    result += delimiter;

                if (!text.IsNullOrEmpty())
                    result += text;
            }

            return result;
        }
    }
}
