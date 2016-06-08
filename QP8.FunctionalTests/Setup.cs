using System;
using System.IO;
using AllureCSharpCommons;
using NLog;
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
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _log.Trace("Global 'OneTimeSetUp' started");
            Console.WriteLine("Global 'OneTimeSetUp' started");

            var resultsPath = Config.AllureResultsPath;
            var environmentXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environment.xml");

            _log.Trace("Allure results directory path: {0}", resultsPath);
            _log.Trace("Environment.xml file: {0}", environmentXmlPath);

            Console.WriteLine("Allure results directory path: {0}", resultsPath);
            Console.WriteLine("Environment.xml file: {0}", environmentXmlPath);

            if (!Directory.Exists(resultsPath))
            {
                _log.Trace("Creating Allure results directory");

                Console.WriteLine("Creating Allure results directory");
                Directory.CreateDirectory(resultsPath);
            }

            AllureConfig.ResultsPath = resultsPath;
            AllureConfig.AllowEmptySuites = true;

            if (File.Exists(environmentXmlPath))
            {
                _log.Trace("Copying environment.xml file to Allure results directory");
                Console.WriteLine("Copying environment.xml file to Allure results directory");

                File.Copy(environmentXmlPath, Path.Combine(resultsPath, "environment.xml"), true);
            }

            _log.Trace("Connection with Selenium GridHub{2}host: '{0}' port: '{1}'", 
                Config.GridHubHost, Config.GridHubPort, Environment.NewLine);
            Console.WriteLine("Connection with Selenium GridHub{2}host: '{0}' port: '{1}'",
                Config.GridHubHost, Config.GridHubPort, Environment.NewLine);

            GridHub = ExtensionCore.GetGridHubManager().ConnectToHub(Config.GridHubHost, Config.GridHubPort);

            _log.Trace("GridHub connected");
            Console.WriteLine("GridHub connected");

            _log.Trace("Global 'OneTimeSetUp' ended");
            Console.WriteLine("Global 'OneTimeSetUp' ended");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _log.Trace("GridHub disposing");
            Console.WriteLine("GridHub disposing");

            try
            {
                GridHub.Dispose();
            }
            catch (Exception e)
            {
                _log.Trace("GridHub disposing");
                Console.WriteLine(e.Message);
            }
        }
    }
}
