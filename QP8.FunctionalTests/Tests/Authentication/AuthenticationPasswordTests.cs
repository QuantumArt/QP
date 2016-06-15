using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using Nunit3AllureAdapter;
using NUnit.Framework;
using QP8.FunctionalTests.Configuration;
using QP8.FunctionalTests.PageObjects.Pages.Authentication;
using QP8.FunctionalTests.TestsData.Authentication;

namespace QP8.FunctionalTests.Tests.Authentication
{
    [AllureStories(Story)]
    [Parallelizable(ParallelScope.Fixtures)]
    public class AuthenticationPasswordTests : AuthenticationTests
    {
        private const string Story = "Password";

        [AllureTest]
        [AllureSeverity(severitylevel.normal)]
        [AllureTitle("Authentication with invalid password")]
        [AllureDescription("Invalid password", descriptiontype.html)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidPassword")]
        public void InvalidPasswordTest(string password)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.QP8BackendLogin, password, Config.QP8BackendCustomerCode);
            CheckValidationSteps(page, page.Password, "Password", "You entered wrong password!");
            CheckJavaScriptErrors();
        }
    }
}
