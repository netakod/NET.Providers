using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace NET.Tools.Providers
{
	public static class StringExtensions
	{
		public static IEnumerable<string> RemoveFirstLines(this string text, Func<string, bool> stopCriteria, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
		{
			int skipNumber = 0;
			var lines = text.ToLines(options);

			foreach (string line in lines)
			{
				if (stopCriteria(line))
					break;

				skipNumber++;
			}

			var newLines = lines.Skip(skipNumber);

			return newLines;
		}
	}
}
