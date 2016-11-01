using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL.Helpers;

namespace QP8.WebMvc.NUnit.Tests.BLL.Services
{
    [TestClass]
    public class ApplicationInfoServiceTest
    {
        [TestMethod]
        [DeploymentItem("TestData\\fix_dbo.testsample.sql")]
        public void GetCurrentFixDboVersionTest()
        {
            const string expected = "7.9.1.0";
            var actual = new ApplicationInfoHelper().GetCurrentFixDboVersion("fix_dbo.testsample.sql");
            Assert.AreEqual(expected, actual, true, CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void VersionsEqualTest_VersionsAreEqual_ReturnTrue()
        {
            var actual = new ApplicationInfoHelper().VersionsEqual("7.9.1.0", "7.9.1.0");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void VersionsEqualTest_VersionsAreNotEqual_ReturnFalse()
        {
            var actual = new ApplicationInfoHelper().VersionsEqual("7.9.1.1", "7.9.1.0");
            Assert.IsFalse(actual);
        }
    }
}
