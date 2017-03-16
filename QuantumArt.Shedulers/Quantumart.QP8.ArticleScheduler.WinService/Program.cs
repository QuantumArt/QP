using System;
using System.ServiceProcess;
using Quantumart.QP8.ArticleScheduler.WinService.Properties;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    internal static class Program
    {
        private static void Main()
        {
            var processor = new QpSchedulerProcessor(Settings.Default.RecurrentTimeout, Settings.SplitExceptCustomerCodes(Settings.Default.ExceptCustomerCodes));
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Starting...");
                processor.Run();

                Console.WriteLine("Running...");
                Console.ReadLine();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[]
                {
                    new ArticleSchedulerService(processor)
                });
            }
        }
    }
}