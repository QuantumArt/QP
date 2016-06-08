using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NLog;
using Nunit3AllureAdapter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using QP8.FunctionalTests.Configuration;
using QP8.PageObjects.Elements;
using QP8.PageObjects.Pages.Authentication;
using SeleniumExtension;
using SeleniumExtension.Support.UI;

namespace QP8.FunctionalTests.Tests
{
    [AllureTestFixture]
    public class BaseTest : AllureStepDefinition
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected RemoteWebDriver Driver;
        protected IWait Wait;
        
        #region commont steps

        protected void AuthenticationSteps(AuthenticationPage page, string login, string password, string customerCode)
        {
            Step(string.Format("Opening {0}", Config.QP8BackendUrl), () =>
            {
                Driver.Url = Config.QP8BackendUrl;
            });

            Step("Authentication", () =>
            {
                MakeAttachment(string.Format("Login: {1}{0}Password: {2}{0}CustomerCode: {3}{0}",
                                             Environment.NewLine, login, password, customerCode),
                               "authentication data", TextType.textPlain);

                page.Authenticate(login, password, customerCode);
            });
        }

        protected void CheckValidationSteps(AuthenticationPage page, Input fieldToCheck,
                                            string fieldToCheckName, string validationErrorText)
        {
            Step("Check validation", () =>
            {
                Assert.That(fieldToCheck.Valid, Is.False,
                    string.Format("{0} input field is not highlighted with red", fieldToCheckName));

                Assert.That(fieldToCheck.ValidationText, Is.EqualTo(validationErrorText),
                    string.Format("{0} input field validation text is incorrect", fieldToCheckName));

                Assert.That(page.Password.Text, Is.Empty,
                    "Password input field is not empty");

                Assert.That(page.ValidationErrors.Count(), Is.EqualTo(1),
                    "Validation errors miscount");

                Assert.That(page.ValidationErrors[0].Text, Is.EqualTo(validationErrorText),
                    "Incorrect validation error text");
            });
        }

        protected void CheckJavaScriptErrors()
        {
            var errors = new List<string>
            {
                "SyntaxError",
                "EvalError",
                "ReferenceError",
                "RangeError",
                "TypeError",
                "URIError"
            };

            var logs = Driver.Manage().Logs.GetLog(LogType.Browser);
            var jsErrors = logs.Where(log => errors.Any(error => log.Message.Contains(error))).ToList();

            if (!jsErrors.Any())
                return;

            foreach (var error in jsErrors)
            {
                MakeAttachment(error.Message, "JavaScript error", TextType.textPlain);
            }

            Assert.That(jsErrors, Is.Empty, "Found JavaScript errors");
        }

        #endregion
        
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
