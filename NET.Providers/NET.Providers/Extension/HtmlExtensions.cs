using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace NET.Tools.Providers
{
	public static class HtmlExtensions
	{
		public static HtmlDocument ToHtmlDocument(this string html)
		{
			var result = new HtmlDocument();
			result.LoadHtml(html);

			return result;
		}
	}
}
