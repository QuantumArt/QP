using System.Collections.Generic;
using QP8.FunctionalTests.Configuration;

namespace QP8.FunctionalTests.TestsData.Authentication
{
    public static class AuthenticationTestsData
    {
        private static readonly IEnumerable<string> _variations = new List<string>
        {
            "1", "123", "t", "test", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "{", "}", "'", @"""", "<", ">"
        };

        public static IEnumerable<string> InvalidLogin
        {
            get { return CombineWithVariations(Config.QP8BackendLogin); }
        }

        public static IEnumerable<string> InvalidPassword
        {
            get { return CombineWithVariations(Config.QP8BackendPassword); }
        }

        public static IEnumerable<string> InvalidCustomerCode
        {
            get { return CombineWithVariations(Config.QP8BackendCustomerCode); }
        }

        private static IEnumerable<string> CombineWithVariations(string pattern)
        {
            var invalidLogins = new List<string>();

            foreach (var variation in _variations)
            {
                invalidLogins.Add(string.Format("{0}{1}", pattern, variation));
                invalidLogins.Add(string.Format("{0}{1}", variation, pattern));
            }

            return invalidLogins;
        }
    }
}
