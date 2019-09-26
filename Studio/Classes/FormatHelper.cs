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
			Match m;
			while ((m = Regex.Match(output, "(SELECT )(.*?)( FROM[ \t\n])")).Success)
			{
				var selectParts = m.Groups[2].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				var newOutput = output.Substring(0, m.Groups[2].Index).Trim();
				for (int i = 0; i < selectParts.Length; i++)
				{
					newOutput += "\r\n\t" + selectParts[i].Trim() + (i == selectParts.Length - 1 ? "" : ",");
				}
				newOutput += "\r\n" + output.Substring(m.Groups[3].Index).Trim();
				output = newOutput;
			}

			string joinParts = $"LEFT JOIN |INNER JOIN |JOIN ";
			var patterns = new string[]
			{
				$"(FROM |AND )(.*?)( {joinParts} | WHERE | AND )",
				$"({joinParts}|FROM )(.*?)( WHERE )",
				$"(ON |WHERE |AND )(.*?)( AND )",
				$"({joinParts})(.*?)({joinParts})"
			};

			foreach (var pattern in patterns)
			{
				while ((m = Regex.Match(output, pattern)).Success)
				{
					var newOutput = output.Substring(0, m.Groups[3].Index).Trim();
					newOutput += "\r\n" + output.Substring(m.Groups[3].Index).Trim();
					output = newOutput;
				}
			}

			patterns = new string[]
			{
				$"( LIMIT .*?)"
			};

			foreach (var pattern in patterns)
			{
				while ((m = Regex.Match(output, pattern)).Success)
				{
					var newOutput = output.Substring(0, m.Groups[1].Index).Trim();
					newOutput += "\r\n" + output.Substring(m.Groups[1].Index).Trim();
					output = newOutput;
				}
			}

			return output;
		}
	}
}
