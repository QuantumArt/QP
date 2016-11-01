using System.Collections.Generic;
using Config = QP8.UI.Tests.Configuration.Config;

namespace QP8.UI.Tests.TestsData.Authentication
{
    public static class AuthenticationTestsData
    {
        public static IEnumerable<string> InvalidLogin => CombineWithVariations(Config.Tests.BackendLogin);

        public static IEnumerable<string> InvalidPassword => CombineWithVariations(Config.Tests.BackendPassword);

        public static IEnumerable<string> InvalidCustomerCode => Config.Tests.BackendCustomerCodeFieldIsDropdown
            ? new List<string> { "ignore" }
            : CombineWithVariations(Config.Tests.BackendCustomerCode);

        private static IEnumerable<string> CombineWithVariations(string pattern)
        {
            var invalidLogins = new List<string>();
            var variations = Config.Environment.IsSmokeTests ? Config.Tests.SmokeVariations : Config.Tests.FullVariations;
            foreach (var variation in variations)
            {
                invalidLogins.Add($"{pattern}{variation}");
                invalidLogins.Add($"{variation}{pattern}");
            }

            return invalidLogins;
        }
    }
}
