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
    [Parallelizable(ParallelScope.Fixtures)]
    [AllureFeatures(Feature.Authentication)]
    public class AuthenticationTests3 : BaseTest
    {
        public const string Title = "Authentication3 form tests";

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
