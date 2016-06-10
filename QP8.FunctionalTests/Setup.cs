using AllureCSharpCommons;
using NUnit.Framework;
using QP8.FunctionalTests.Configuration;
using SeleniumExtension;
using SeleniumExtension.Grid;

namespace QP8.FunctionalTests
{
    [SetUpFixture]
    public class Setup
    {
        public static IGridHub GridHub;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            AllureConfig.AllowEmptySuites = true;
            AllureConfig.ResultsPath = Config.AllureResultsPath;

            GridHub = ExtensionCore.GetGridHubManager().ConnectToHub(Config.GridHubHost, Config.GridHubPort);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            GridHub.Dispose();
        }
    }
}
