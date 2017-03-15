using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL.Helpers;

namespace QP8.WebMvc.NUnit.Tests.BLL.Helpers
{
    /// <summary>
    ///This is a test class for MultistepActionHelperTest and is intended
    ///to contain all MultistepActionHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class MultistepActionHelperTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///A test for GetStepCount
        ///</summary>
        [TestMethod]
        public void GetStepCountTest()
        {
            Assert.AreEqual(0, MultistepActionHelper.GetStepCount(0, 20));
            Assert.AreEqual(1, MultistepActionHelper.GetStepCount(1, 20));
            Assert.AreEqual(1, MultistepActionHelper.GetStepCount(19, 20));
            Assert.AreEqual(1, MultistepActionHelper.GetStepCount(20, 20));
            Assert.AreEqual(2, MultistepActionHelper.GetStepCount(21, 20));
            Assert.AreEqual(5, MultistepActionHelper.GetStepCount(100, 20));
            Assert.AreEqual(6, MultistepActionHelper.GetStepCount(101, 20));
        }
    }
}
