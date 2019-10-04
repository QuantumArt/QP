using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using QA.Core.ServiceBaseLifeTime;
using Quantumart.QP8.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) => { services.AddHostedService<ArticleService>(); })
                .ConfigureServices((hostContext, services) => { services.Configure<HostOptions>(option => { option.ShutdownTimeout = System.TimeSpan.FromSeconds(20); }); })
                .ConfigureServices((hostContext, services) => { services.Configure<ArticleSchedulerProperties>(hostContext.Configuration.GetSection("Properties")); })
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
                .UseConsoleLifetime();


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!(Debugger.IsAttached || ((IList)args).Contains("--console")))
                {
                    await builder.RunAsServiceAsync();
                }
                else
                {
                    await builder.RunConsoleAsync();
                }

            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
    }
}





