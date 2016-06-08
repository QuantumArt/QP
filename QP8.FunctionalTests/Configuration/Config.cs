using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace QP8.FunctionalTests.Configuration
{
    public class Config
    {
        public static string QP8BackendUrl { get { return StringValue("QP8BackendUrl", "http://mscdev02:90/Backend/"); } }
        public static string QP8BackendLogin { get { return StringValue("QP8BackendLogin", "AutotestQuantumart"); } }
        public static string QP8BackendPassword { get { return StringValue("QP8BackendPassword", "1q2w-p=[Password"); } }
        public static string QP8BackendCustomerCode { get { return StringValue("QP8BackendCustomerCode", "qp_beeline_main"); } }

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

        public static string GridHubHost { get { return StringValue("GridHubHost", "localhost"); } }
        public static string GridHubPort { get { return StringValue("GridHubPort", "4444"); } }

        public static TimeSpan PageLoadTimeout { get { return TimeSpan.FromMilliseconds(IntValue("PageLoadTimeout", 20000)); } }
        public static TimeSpan ImplicitlyTimeout { get { return TimeSpan.FromMilliseconds(IntValue("ImplicitlyTimeout", 2000)); } }
        public static TimeSpan JavaScriptTimeout { get { return TimeSpan.FromMilliseconds(IntValue("JavaScriptTimeout", 5000)); } }

        private static string StringValue(string propertyName, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[propertyName];
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        private static int IntValue(string propertyName, int defaultValue)
        {
            int value;
            return int.TryParse(ConfigurationManager.AppSettings[propertyName], out value) ? value : defaultValue;
        }

        private static bool BoolValue(string propertyName)
        {
            bool value;
            bool.TryParse(ConfigurationManager.AppSettings[propertyName], out value);
            return value;
        }
    }
}
