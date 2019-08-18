using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Classes
{
	public class FormatHelper
	{
		public static string GetFormattedSQL(DataSource dataSource, string input)
		{
			string output = input;
			var reservedKeywords = dataSource.GetReservedKeywords();
			foreach (var kw in reservedKeywords)
			{
				output = Regex.Replace(output, $"\\b{kw}\\b", kw.ToUpper(), RegexOptions.IgnoreCase);
			}
			return output;
		}
	}
}
