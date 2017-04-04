using System.Reflection;
using Flurl.Http.Testing;
using Xunit.Sdk;

namespace QP8.Infrastructure.TestTools.Xunit.Attributes
{
    public class FlurlMockAttribute : BeforeAfterTestAttribute
    {
        private HttpTest _httpTest;

        public override void Before(MethodInfo methodUnderTest)
        {
            _httpTest = new HttpTest();
            base.After(methodUnderTest);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            _httpTest.Dispose();
            base.After(methodUnderTest);
        }
    }
}
