using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using NUnit.Framework;
using Nunit3AllureAdapter;
using QP8.UI.Tests.Configuration;
using QP8.UI.Tests.PageObjects.Pages.Authentication;
using QP8.UI.Tests.TestsData.Authentication;

namespace QP8.UI.Tests.Tests.Authentication
{
    [AllureStories(Story)]
    [Parallelizable(ParallelScope.Fixtures)]
    public class AuthenticationCustomerCodeTests : AuthenticationTests
    {
        private const string Story = "Customer code";

        [AllureTest]
        [AllureSeverity(severitylevel.normal)]
        [AllureTitle("Authentication with invalid customer code")]
        [AllureDescription("Invalid customer code", descriptiontype.html)]
        [TestCaseSource(typeof(AuthenticationTestsData), nameof(AuthenticationTestsData.InvalidCustomerCode))]
        public void InvalidCustomerCodeInInputValidationErrorsTest(string customerCode)
        {
            if (!Config.Tests.BackendCustomerCodeFieldIsDropdown)
            {
                var page = new AuthenticationPage(Driver);
                AuthenticationSteps(page, Config.Tests.BackendLogin, Config.Tests.BackendPassword, customerCode);
                CheckValidationSteps(page, page.CustomerCodeInput, "Customer code", "Customer code does not exist!");
                CheckJavaScriptErrors();
            }
        }
    }
}
