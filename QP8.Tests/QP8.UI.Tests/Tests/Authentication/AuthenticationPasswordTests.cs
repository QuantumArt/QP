using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using NUnit.Framework;
using Nunit3AllureAdapter;
using QP8.Tests.Infrastucture.PageObjects.Pages.Authentication;
using QP8.UI.Tests.TestsData.Authentication;
using Config = QP8.UI.Tests.Configuration.Config;

namespace QP8.UI.Tests.Tests.Authentication
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
        [TestCaseSource(typeof(AuthenticationTestsData), nameof(AuthenticationTestsData.InvalidPassword))]
        public void InvalidPasswordTest(string password)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.Tests.BackendLogin, password, Config.Tests.BackendCustomerCode);
            CheckValidationSteps(page, page.Password, "Password", "You entered wrong password!");
            CheckJavaScriptErrors();
        }
    }
}
