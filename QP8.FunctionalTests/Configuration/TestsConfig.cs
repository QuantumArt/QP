using System.Collections.Generic;

namespace QP8.FunctionalTests.Configuration
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

            public static string BackendUrl { get { return StringValue("BackendUrl", "http://mscdev02:90/Backend/"); } }
            public static string BackendLogin { get { return StringValue("BackendLogin", "AutotestQuantumart"); } }
            public static string BackendPassword { get { return StringValue("BackendPassword", "1q2w-p=[Password"); } }
            public static string BackendCustomerCode { get { return StringValue("BackendCustomerCode", "qp_beeline_main"); } }

            public static string BackendCustomerCodeFieldType
            {
                get
                {
                    return StringValue("BackendCustomerCodeFieldType", "input");
                }
            }

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
