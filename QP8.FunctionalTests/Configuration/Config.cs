using System.Configuration;

namespace QP8.FunctionalTests.Configuration
{
    public static partial class Config
    {
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
