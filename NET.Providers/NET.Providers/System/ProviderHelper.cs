using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Providers
{
    public class ProviderHelper
    {
        public static string[][] GetTable(string tableText)
        {
            return GetTable(tableText, String.Empty);
        }

        public static string[][] GetTable(string tableText, string beginHeaderText)
        {
            return GetTable(tableText, beginHeaderText, String.Empty);
        }

        // Example 1:
        //Interface                      Status         Protocol Description
        //Fa0                            up             up       
        //Fa1                            up             up       
        //Fa2                            up             down     
        //
        // Example 2:
        //Interface                      Status         Protocol Description
        //--------------------------------------------------------------------
        //Fa0                            up             up       
        //Fa1                            up             up       
        //Fa2                            up             down     
        //
        // Example 3:
        //Interface                      Status         Protocol Description
        //--------------------------------------------------------------------
        //Fa0                            up             up       
        //Fa1                            up             up       
        //Fa2                            up             down     
        //Fa3                            up             up       
        //--------------------------------------------------------------------
        //Fa4                            initializing   down     
        //Vl1                            up             up       Kucna mreza
        //NV0                            up             up       
        //Di1                            up             up       
        //
        // Example 4:
        //Interface                      Status         Protocol Description
        //--------------------------------------------------------------------
        //Fa0                            up             up       
        //Fa1                            up             up       
        //Fa2                            up             down     
        //Fa3                            up             up       
        //
        //Interface                      Status         Protocol Description
        //--------------------------------------------------------------------
        //Fa4                            initializing   down     
        //Vl1                            up             up       Kucna mreza
        //NV0                            up             up       
        //Di1                            up             up       
        public static string[][] GetTable(string tableText, string beginHeaderText, string endText)
        {
            List<string[]> result = new List<string[]>();
            endText = (endText == null) ? String.Empty : endText.Trim();
            beginHeaderText = (beginHeaderText == null) ? String.Empty : beginHeaderText;
            int headerLenght = 0;

            string[] tableTextArray = tableText.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in tableTextArray)
            {
                if (line.Trim().Length == 0)
                    continue;

                if (beginHeaderText.Length > 0 && line.Trim().StartsWith(beginHeaderText))
                {
                    if (result.Count > 0)
                    {
                        if (headerLenght == 0)
                            headerLenght = result.Count;

                        for (int i = 0; i < headerLenght; i++) // Remove header above
                        {
                            result.RemoveAt(result.Count - 1);
                        }
                    }
                    
                    continue;
                }

                if (endText != String.Empty && line.Trim().StartsWith(endText))
                    break;

                string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                
                if (lineArray.Length > 0)
                    result.Add(lineArray);
            }

            return result.ToArray();
        }

        public static string[] GetTableHeader(string tableText)
        {
            string[] result = new string[] { };
            string[] inputTextArray = tableText.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.None);

            foreach (string line in inputTextArray)
            {
                if (line.Trim().Length > 0)
                {
                    result = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    break;
                }
            }

            return result;
        }

        public static string GetStandardizedInterfaceName(string interfaceName)
        {
            if (interfaceName == null)
                return String.Empty;

            string standardizedInterfaceName = interfaceName.Replace("\0", "").Trim();

            if (standardizedInterfaceName.ToLower().StartsWith("vlan"))
                standardizedInterfaceName = "Vlan" + standardizedInterfaceName.Substring(4).Trim();

            return standardizedInterfaceName;
        }
    }
}
