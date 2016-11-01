using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using NUnit.Framework;
using Nunit3AllureAdapter;
using QP8.Tests.Infrastucture.PageObjects.Pages.Authentication;
using QP8.Tests.Infrastucture.PageObjects.Pages.Main;
using QP8.UI.Tests.TestsData.Authentication;
using Config = QP8.UI.Tests.Configuration.Config;

namespace QP8.UI.Tests.Tests.Authentication
{
    [AllureStories(Story)]
    [Parallelizable(ParallelScope.Fixtures)]
    public class AuthenticationLoginTests : AuthenticationTests
    {
        private const string Story = "Login";

        [AllureTest]
        [AllureSeverity(severitylevel.critical)]
        [AllureTitle("Authentication with valid login")]
        [AllureDescription("Valid login", descriptiontype.html)]
        public void AuthenticationWithValidData()
        {
            var login = Config.Tests.BackendLogin;
            var password = Config.Tests.BackendPassword;
            var customerCode = Config.Tests.BackendCustomerCode;

            AuthenticationSteps(new AuthenticationPage(Driver), login, password, customerCode);

            Step("Authentication check", () =>
            {
                var mainPage = new MainPage(Driver);
                Assert.That(mainPage.Header.UserName.Text, Is.EqualTo(login), "Incorrect user name");
                Assert.That(mainPage.Tree.Displayed, Is.True, "Menu tree is not displayed");
            });

            CheckJavaScriptErrors();
        }

        [AllureTest]
        [AllureSeverity(severitylevel.normal)]
        [AllureTitle("Authentication with invalid login")]
        [AllureDescription("Invalid login", descriptiontype.html)]
        [TestCaseSource(typeof(AuthenticationTestsData), nameof(AuthenticationTestsData.InvalidLogin))]
        public void AuthenticationWithInvalidLogin(string login)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, login, Config.Tests.BackendPassword, Config.Tests.BackendCustomerCode);
            CheckValidationSteps(page, page.Login, "Login", "Your account does not exist!");
            CheckJavaScriptErrors();
        }
    }
}
