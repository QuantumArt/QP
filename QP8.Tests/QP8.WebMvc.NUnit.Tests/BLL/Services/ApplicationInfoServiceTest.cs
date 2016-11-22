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
            var actual = ApplicationInfoHelpers.GetCurrentFixDboVersion("fix_dbo.testsample.sql");
            Assert.AreEqual(expected, actual, true, CultureInfo.InvariantCulture);
        }
    }
}
