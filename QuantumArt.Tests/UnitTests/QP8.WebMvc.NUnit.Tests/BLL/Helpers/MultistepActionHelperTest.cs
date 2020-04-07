using NUnit.Framework;
using Quantumart.QP8.BLL.Helpers;

namespace QP8.WebMvc.NUnit.Tests.BLL.Helpers
{
    [TestFixture]
    public class MultistepActionHelperTest
    {
        [Test]
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
