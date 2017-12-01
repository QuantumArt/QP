using System;
using System.ServiceProcess;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    internal static class Program
    {
        private static void Main()
        {
            var service = new ArticleSchedulerService();
            if (Environment.UserInteractive)
            {
                Console.WriteLine(@"Starting...");
                service.Processor.Run();

                Console.WriteLine(@"Running...");
                Console.ReadLine();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { service });
            }
        }
    }
}
