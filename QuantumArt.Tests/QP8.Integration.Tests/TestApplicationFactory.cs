using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Quantumart.QP8.WebMvc;

namespace QP8.Integration.Tests
{
    public class TestApplicationFactory : WebApplicationFactory<Startup>
    {
        public TestApplicationFactory()
        {
            CreateDefaultClient();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }
    }
}
