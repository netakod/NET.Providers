using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public static class StringReplacement
	{
		public static readonly Dictionary<char, char> CroatianSpecificLettersEnglishReplacement = new()
		{
			{ 'Č', 'C' },
			{ 'č', 'c' },
			{ 'Ć', 'C' },
			{ 'ć', 'c' },
			{ 'Đ', 'd' },
			{ 'đ', 'd' },
			{ 'Š', 'S' },
			{ 'š', 's' },
			{ 'Ž', 'Z' },
			{ 'ž', 'z' },
		};
	}
}
