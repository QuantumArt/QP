using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using Nunit3AllureAdapter;
using OpenQA.Selenium.Remote;
using QP8.UI.Tests.Configuration;
using OpenQA.Selenium.Chrome;

namespace QP8.UI.Tests.Tests
{
    [AllureTestFixture]
    public class BaseTest : AllureStepDefinition
    {
        protected RemoteWebDriver Driver;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Driver = Setup.GridHub.GetBrowserInstance((DesiredCapabilities)new ChromeOptions().ToCapabilities());

            var timeouts = Driver.Manage().Timeouts();
            timeouts.ImplicitWait = Config.Environment.ImplicitlyTimeout;
            timeouts.PageLoad = Config.Environment.PageLoadTimeout;
            timeouts.AsynchronousJavaScript = Config.Environment.JavaScriptTimeout;

            var failedActions = new Action[]
            {
                () =>
                {
                    var screenshot = Driver.GetScreenshot();
                    MakeAttachment(Image.FromStream(new MemoryStream(screenshot.AsByteArray)),"Exception screenshot", ImageType.imageJpeg);
                }
            };

            NUnit3AllureAdapterConfig.ActionsIfStepFailed = failedActions;
        }

        [SetUp]
        public void SetUp()
        {
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Driver.Dispose();
        }
    }
}
