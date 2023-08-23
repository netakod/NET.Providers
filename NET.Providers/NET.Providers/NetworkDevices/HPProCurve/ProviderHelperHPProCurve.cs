using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NET.Tools.Terminal;

namespace NET.Tools.Providers
{
    internal class ProviderHelperHPProCurve
    {
        public static string[][] GetTable(string textTable)
        {
            return GetTable(textTable, String.Empty, true);
        }

        public static string[][] GetTable(string tableText, string beginHeaderText)
        {
            return GetTable(tableText, beginHeaderText, true);
        }

        public static string[][] GetTable(string tableText, string beginHeaderText, bool skipLineAfterHeader)
        {
            return GetTable(tableText, beginHeaderText, skipLineAfterHeader, String.Empty);
        }

        public static string[][] GetTable(string tableText, string beginHeaderText, bool skipLineAfterHeader, string endText)
        {
            bool isThisTargetSection = false;
            bool isThisFirstLineAfterTargetSection = true;

            List<string[]> result = new List<string[]>();
            endText = (endText == null) ? String.Empty : endText.Trim();
            beginHeaderText = (beginHeaderText == null) ? String.Empty : beginHeaderText;

            string[] inputTextArray = tableText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string line in inputTextArray)
            {
                if (line.Trim().Length == 0)
                    continue;

                if (!isThisTargetSection)
                {
                    if (line.Trim().StartsWith(beginHeaderText))
                        isThisTargetSection = true;

                    continue;
                }

                if (skipLineAfterHeader && isThisFirstLineAfterTargetSection)
                {
                    isThisFirstLineAfterTargetSection = false;
                    
                    continue;
                }

                if (endText != "" && line.Trim().StartsWith(endText))
                    break;

                string[] lineArray = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

				if (lineArray.Length > 1)
                    result.Add(lineArray);
            }

            return result.ToArray();
        }

        public static string[] GetTextTableHeader(string textTable)
        {
            string[] result = new string[] { };
            string[] inputTextArray = textTable.Split(new string[] { "\r\n" }, StringSplitOptions.None);

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

        
        public static async ValueTask<IEnumerable<string>> GetInterfaceTrunkGroupNames(TerminalClient terminal)
        {
            const string strTrunk = "trunk";
            List<string> result = new List<string>();

                              await terminal.ExitConfigModeAsync();
            string response = await terminal.SendAsync("show trunks");

            string[][] trunkTable = ProviderHelperHPProCurve.GetTable(response, "----", skipLineAfterHeader: false);

            foreach (string[] lineArray in trunkTable)
            {
                string trunkPort = lineArray.First();
                string trunkType = lineArray.Last();
                string trunkGroupName = lineArray.ElementAt(lineArray.Length - 2);

                if (trunkType.ToLower() == strTrunk && !result.Contains(trunkGroupName))
                    result.Add(trunkGroupName);
            }

            return result;
        }
    }
}
