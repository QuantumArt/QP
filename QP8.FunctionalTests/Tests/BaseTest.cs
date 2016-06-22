using Nunit3AllureAdapter;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using QP8.FunctionalTests.Configuration;
using SeleniumExtension;
using SeleniumExtension.Support.UI;

namespace QP8.FunctionalTests.Tests
{
    [AllureTestFixture]
    public class BaseTest : AllureStepDefinition
    {
        protected RemoteWebDriver Driver;
        protected IWait Wait;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Driver = Setup.GridHub.GetBrowserInstance(DesiredCapabilities.Chrome());
            Driver.Manage().Timeouts().ImplicitlyWait(Config.ImplicitlyTimeout);
            Driver.Manage().Timeouts().SetPageLoadTimeout(Config.PageLoadTimeout);
            Driver.Manage().Timeouts().SetScriptTimeout(Config.JavaScriptTimeout);
        }

        [SetUp]
        public void SetUp()
        {
            Driver.Manage().Cookies.DeleteAllCookies();

            Wait = ExtensionCore.GetWaiter();
            Wait.Timeout = Config.ImplicitlyTimeout;
            Wait.IgnoreExceptionTypes(typeof(NotFoundException), typeof(StaleElementReferenceException));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Driver.Dispose();
        }
    }
}
