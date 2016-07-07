using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace QP8.FunctionalTests.Configuration
{
    public static partial class Config
    {
        public static class Environment
        {
            public static bool IsSmokeTests { get { return BoolValue("IsSmokeTests"); } }

            public static string GridHubHost { get { return StringValue("GridHubHost", "localhost"); } }
            public static string GridHubPort { get { return StringValue("GridHubPort", "4444"); } }

            public static TimeSpan PageLoadTimeout { get { return TimeSpan.FromMilliseconds(IntValue("PageLoadTimeout", 20000)); } }
            public static TimeSpan ImplicitlyTimeout { get { return TimeSpan.FromMilliseconds(IntValue("ImplicitlyTimeout", 2000)); } }
            public static TimeSpan JavaScriptTimeout { get { return TimeSpan.FromMilliseconds(IntValue("JavaScriptTimeout", 5000)); } }

            public static string AllureResultsPath
            {
                get
                {
                    var path = StringValue("AllureResultsPath", "qp_beeline_main");

                    if (path.Any(character => Path.GetInvalidPathChars().Contains(character)))
                        throw new ConfigurationErrorsException("Value of key 'AllureResultsPath' contains invalid path characters");

                    if (path.Contains("${BuildPath}"))
                        path = path.Replace("${BuildPath}", AppDomain.CurrentDomain.BaseDirectory);

                    if (!path.EndsWith("\\"))
                        path += "\\";

                    return path;
                }
            }
        }
    }
}
