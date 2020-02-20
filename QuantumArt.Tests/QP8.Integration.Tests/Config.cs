using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using QP8.Integration.Tests.Infrastructure;
using Quantumart.QP8.WebMvc;

namespace QP8.Integration.Tests
{
    [SetUpFixture]
    public class Config
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Global.Factory = new WebApplicationFactory<Startup>();
            Global.Factory.CreateDefaultClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Global.Factory.Dispose();
        }
   }
}
