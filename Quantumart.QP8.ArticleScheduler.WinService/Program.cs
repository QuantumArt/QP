using System.ServiceProcess;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var ServicesToRun = new ServiceBase[]
            {
                new ArticleSchedulerService()
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
