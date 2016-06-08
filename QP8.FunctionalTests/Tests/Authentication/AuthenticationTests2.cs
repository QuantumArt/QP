using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using Nunit3AllureAdapter;
using NUnit.Framework;
using QP8.FunctionalTests.Configuration;
using QP8.FunctionalTests.TestsData;
using QP8.FunctionalTests.TestsData.Authentication;
using QP8.PageObjects.Pages.Authentication;

namespace QP8.FunctionalTests.Tests.Authentication
{
    [TestFixture]
    [AllureTitle(Title)]
    [AllureFeatures(Feature.Authentication)]
    public class AuthenticationTests2 : BaseTest
    {
        public const string Title = "Authentication2 form tests";
        
        #region invalid password

        [AllureTest]
        [AllureTitle("Authentication with invalid password")]
        [AllureDescription("Invalid password", descriptiontype.html)]
        [AllureStories(Story.Negative)]
        [AllureSeverity(severitylevel.normal)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidPassword")]
        public void AuthenticationWithInvalidPassword(string password)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.QP8BackendLogin, password, Config.QP8BackendCustomerCode);
            CheckValidationSteps(page, page.Password, "Password", "You entered wrong password!");
            CheckJavaScriptErrors();
        }

        #endregion

        #region invalid customer code

        [AllureTest]
        [AllureTitle("Authentication with invalid customer code")]
        [AllureDescription("Invalid customer code", descriptiontype.html)]
        [AllureStories(Story.Negative)]
        [AllureSeverity(severitylevel.normal)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidCustomerCode")]
        public void AuthenticationWithInvalidCustomerCode(string customerCode)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.QP8BackendLogin, Config.QP8BackendPassword, customerCode);
            CheckValidationSteps(page, page.CustomerCode, "Customer code", "Customer code does not exist!");
            CheckJavaScriptErrors();
        }

        #endregion
    }
}
