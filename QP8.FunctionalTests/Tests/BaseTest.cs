using System;
using System.Drawing;
using System.IO;
using NLog;
using Nunit3AllureAdapter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using QP8.FunctionalTests.Configuration;
using SeleniumExtension;
using SeleniumExtension.Support.UI;

namespace QP8.FunctionalTests.Tests
{
    [AllureTestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class BaseTest : AllureStepDefinition
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected RemoteWebDriver Driver;
        protected IWait Wait;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _log.Trace("Local 'OneTimeSetup' started");
            Console.WriteLine("Tests 'OneTimeSetup' started");

            _log.Trace("Starting Chromedriver");
            Console.WriteLine("Starting Chromedriver");

            Driver = Setup.GridHub.GetBrowserInstance(DesiredCapabilities.Chrome());

            _log.Trace("Chromedriver started");
            Console.WriteLine("Chromedriver started");

            Driver.Manage().Timeouts().ImplicitlyWait(Config.ImplicitlyTimeout);
            Driver.Manage().Timeouts().SetPageLoadTimeout(Config.PageLoadTimeout);
            Driver.Manage().Timeouts().SetScriptTimeout(Config.JavaScriptTimeout);

            _log.Trace("Local 'OneTimeSetup' ended");
            Console.WriteLine("Tests 'OneTimeSetup' ended");
        }

        [SetUp]
        public void SetUp()
        {
            _log.Trace("'Setup' started");
            Console.WriteLine("Test 'Setup' started");

            _log.Trace("Deleting cookies");
            Console.WriteLine("Deleting cookies");
            
            Driver.Manage().Cookies.DeleteAllCookies();

            Wait = ExtensionCore.GetWaiter();
            Wait.Timeout = Config.ImplicitlyTimeout;
            Wait.IgnoreExceptionTypes(typeof(NotFoundException), typeof(StaleElementReferenceException));

            _log.Trace("'Setup' ended");
            Console.WriteLine("Test 'Setup' ended");
        }

        [TearDown]
        public void TearDown()
        {
            if (!TestContext.CurrentContext.Result.Outcome.Status.Equals(TestStatus.Failed))
                return;

            using (var memoryStream = new MemoryStream(Driver.GetScreenshot().AsByteArray))
            {
                MakeAttachment(Image.FromStream(memoryStream), "Failed screenshot", ImageType.imagePng);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _log.Trace("Chromedriver disposing");
            Console.WriteLine("Chromedriver disposing");

            Driver.Dispose();
        }
    }
}
