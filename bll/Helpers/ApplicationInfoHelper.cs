using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Quantumart.QP8.BLL.Repository;
using System.Web;

namespace Quantumart.QP8.BLL.Helpers
{
	public class ApplicationInfoHelper
	{
		public string GetCurrentFixDboVersion(string fix_dbo_path)
		{
			string version = String.Empty;

			Regex regex = new Regex(@"^print\s+'(?<ver>(\d+\.){3}\d+){1}\s+completed'$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

			if (File.Exists(fix_dbo_path))
			{
				using (var reader = new StreamReader(fix_dbo_path, Encoding.GetEncoding(1251)))
				{
					while (reader.Peek() >= 0)
					{
						string currentLine = reader.ReadLine();
						MatchCollection mathes = regex.Matches(currentLine);
						if (mathes.Count > 0)
						{
							version = mathes[0].Groups["ver"].Value;
						}
					}
				}
			}

			return version;
		}

		public string GetCurrentDBVersion()
		{
			return ApplicationInfoRepository.GetCurrentDBVersion();
		}

		public string GetCurrentBackendVersion()
		{
			return GetCurrentFixDboVersion(Path.Combine(HttpContext.Current.Server.MapPath("~"), "fix_dbo.sql"));
		}

		public bool RecordActions()
		{
			return ApplicationInfoRepository.RecordActions();
		}

		internal bool VersionsEqual(string v1, string v2)
		{
			return String.Equals(v1, v2, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}

