using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Quantumart.QP8.WebMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                .Build();

            var builder = WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .UseNLog()
                .UseStartup<Startup>();
            return builder.Build();
        }
    }
}
