using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.ArticleScheduler.WinService.Properties;

namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler.WinService.Properties
{
    /// <summary>
    ///This is a test class for SettingsTest and is intended
    ///to contain all SettingsTest Unit Tests
    ///</summary>
    [TestClass]
    public class SettingsTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SplitExceptCustomerCodesTest_ArgumentIsEmptyString_ReturnEmptyResult()
        {
            var exceptCustomerCodes = string.Empty;
            var actual = Settings.SplitExceptCustomerCodes(exceptCustomerCodes);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count());
        }

        [TestMethod]
        public void SplitExceptCustomerCodesTest_ArgumentIsNull_ReturnEmptyResult()
        {
            var actual = Settings.SplitExceptCustomerCodes(null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count());
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void SplitExceptCustomerCodesTest_VariousArguments_ReturnCorrectResults()
        {
            var exceptCustomerCodes = string.Empty;
            IEnumerable<string> expect = new[] { "c1", "c2", "c3" };
            IEnumerable<string> actual;

            Action testResult = () =>
            {
                var message = "Argument is: \"" + exceptCustomerCodes + "\"";
                actual = Settings.SplitExceptCustomerCodes(exceptCustomerCodes);
                Assert.IsNotNull(actual, message);
                Assert.AreEqual(expect.Count(), actual.Count(), message);
                Assert.IsFalse(expect.Except(actual).Any(), message);
            };

            exceptCustomerCodes = "c1;c2;c3";
            testResult();

            exceptCustomerCodes = "c1;c2;c3;";
            testResult();

            exceptCustomerCodes = ";c1;c2;c3;";
            testResult();

            exceptCustomerCodes = "  ;;c1;c2;c3;;  ";
            testResult();

            exceptCustomerCodes = " c1; c2;c3; ";
            testResult();

            exceptCustomerCodes = " c1;c2; c3 ";
            testResult();

            exceptCustomerCodes = " ; ;; c1;; c2;;;    c3 ; ;;";
            testResult();

            expect = new[] { "c1" };

            exceptCustomerCodes = "c1;";
            testResult();

            exceptCustomerCodes = " c1;  ";
            testResult();

            exceptCustomerCodes = " c1  ";
            testResult();
        }
    }
}
