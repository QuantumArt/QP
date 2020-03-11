using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                .Build();

            var location = Assembly.GetEntryAssembly()?.Location;
            var contentRoot = location != null ? Path.GetDirectoryName(location) : Directory.GetCurrentDirectory();

            var builder = WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseContentRoot(contentRoot)
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
            return builder;
        }

        public static IWebHost BuildWebHost(string[] args) => CreateWebHostBuilder(args).Build();
    }
}
