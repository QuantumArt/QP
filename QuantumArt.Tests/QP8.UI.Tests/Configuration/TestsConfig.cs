using System.Collections.Generic;

namespace QP8.UI.Tests.Configuration
{
    public static partial class Config
    {
        public static class Tests
        {
            public static IEnumerable<string> SmokeVariations
            {
                get
                {
                    var variations = StringValue("SmokeVariations", string.Empty);
                    return AvoidSpecialCharacters(variations).Split(' ');
                }
            }

            public static IEnumerable<string> FullVariations
            {
                get
                {
                    var variations = StringValue("FullVariations", string.Empty);
                    return AvoidSpecialCharacters(variations).Split(' ');
                }
            }

            public static string BackendUrl => StringValue("BackendUrl", "http://mscdev02:90/Backend/");

            public static string BackendLogin => StringValue("BackendLogin", "AutotestQuantumart");

            public static string BackendPassword => StringValue("BackendPassword", "1q2w-p=[Password");

            public static string BackendCustomerCode => StringValue("BackendCustomerCode", "qp_beeline_main");

            public static bool BackendCustomerCodeFieldIsDropdown => BoolValue("BackendCustomerCodeFieldIsDropdown");

            private static string AvoidSpecialCharacters(string inputString)
            {
                return inputString.Replace("&quot;", @"""")
                                  .Replace("&apos;", "'")
                                  .Replace("&lt;", "<")
                                  .Replace("&gt;", ">")
                                  .Replace("&amp;", "&");
            }
        }
    }
}
