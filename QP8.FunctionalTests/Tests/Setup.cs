using System;
using System.IO;
using AllureCSharpCommons;
using NUnit.Framework;
using QP8.FunctionalTests.Configuration;
using SeleniumExtension;
using SeleniumExtension.Grid;

namespace QP8.FunctionalTests.Tests
{
    [SetUpFixture]
    public class Setup
    {
        public static IGridHub GridHub;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var resultsPath = Config.Environment.AllureResultsPath;
            var environmentXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environment.xml");

            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
            }

            AllureConfig.ResultsPath = resultsPath;
            AllureConfig.AllowEmptySuites = true;

            if (File.Exists(environmentXmlPath))
            {
                File.Copy(environmentXmlPath, Path.Combine(resultsPath, "environment.xml"), true);
            }

            GridHub = ExtensionCore.GetGridHubManager().ConnectToHub(Config.Environment.GridHubHost, Config.Environment.GridHubPort);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            GridHub.Dispose();
        }
    }
}
