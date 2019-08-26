using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Quantumart.QP8.WebMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();

                    // TODO: review logging
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                    }
                    else
                    {
                        logging.SetMinimumLevel(LogLevel.Information);
                    }
                })
                .UseNLog();
    }
}
