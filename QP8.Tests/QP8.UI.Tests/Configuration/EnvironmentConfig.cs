using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace QP8.UI.Tests.Configuration
{
    public static partial class Config
    {
        public static class Environment
        {
            public static bool IsSmokeTests => Config.BoolValue("IsSmokeTests");

            public static string GridHubHost => Config.StringValue("GridHubHost", "localhost");
            public static string GridHubPort => Config.StringValue("GridHubPort", "4444");

            public static TimeSpan PageLoadTimeout => TimeSpan.FromMilliseconds(Config.IntValue("PageLoadTimeout", 20000));

            public static TimeSpan ImplicitlyTimeout => TimeSpan.FromMilliseconds(Config.IntValue("ImplicitlyTimeout", 2000));

            public static TimeSpan JavaScriptTimeout => TimeSpan.FromMilliseconds(Config.IntValue("JavaScriptTimeout", 5000));

            public static string AllureResultsPath
            {
                get
                {
                    var path = Config.StringValue("AllureResultsPath", "qp_beeline_main");

                    if (path.Any(character => Path.GetInvalidPathChars().Contains(character)))
                    {
                        throw new ConfigurationErrorsException("Value of key 'AllureResultsPath' contains invalid path characters");
                    }

                    if (path.Contains("${BuildPath}"))
                    {
                        path = path.Replace("${BuildPath}", AppDomain.CurrentDomain.BaseDirectory);
                    }

                    if (!path.EndsWith("\\"))
                    {
                        path += "\\";
                    }

                    return path;
                }
            }
        }
    }
}
