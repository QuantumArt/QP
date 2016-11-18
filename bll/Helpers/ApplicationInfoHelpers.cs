using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class ApplicationInfoHelpers
    {
        public static string GetCurrentFixDboVersion(string fixDboPath)
        {
            var version = string.Empty;
            var regex = new Regex(@"^print\s+'(?<ver>(\d+\.){3}\d+){1}\s+completed'$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            if (File.Exists(fixDboPath))
            {
                using (var reader = new StreamReader(fixDboPath, Encoding.GetEncoding(1251)))
                {
                    while (reader.Peek() >= 0)
                    {
                        var currentLine = reader.ReadLine();
                        if (currentLine != null)
                        {
                            var mathes = regex.Matches(currentLine);
                            if (mathes.Count > 0)
                            {
                                version = mathes[0].Groups["ver"].Value;
                            }
                        }
                    }
                }
            }

            return version;
        }

        public static string GetCurrentBackendVersion()
        {
            return GetCurrentFixDboVersion(Path.Combine(HttpContext.Current.Server.MapPath("~"), "fix_dbo.sql"));
        }

        internal static bool VersionsEqual(string v1, string v2)
        {
            return string.Equals(v1, v2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
