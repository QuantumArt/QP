using QP8.Infrastructure.Logging.Factories;
using Topshelf;

namespace Quantumart.QP8.CdcDataImport
{
    internal static class Program
    {
        private static void Main()
        {
            LogProvider.LogFactory = new NLogFactory();
            HostFactory.Run(factory =>
            {
                factory.UseNLog();
                factory.EnableShutdown();
                factory.EnablePauseAndContinue();
                factory.Service<IServiceHost>(sc =>
                {
                    sc.ConstructUsing(s => new ServiceHost());
                    sc.WhenStarted((s, host) => s.Start(host));
                    sc.WhenStopped(s => s.Stop());
                    sc.WhenPaused(s => s.Pause());
                    sc.WhenContinued(s => s.Continue());

                    factory.RunAsLocalSystem()
                        .StartAutomatically()
                        .EnableServiceRecovery(sr =>
                        {
                            sr.OnCrashOnly();
                            sr.RestartService(0);
                            sr.SetResetPeriod(1);
                        });

                    factory.SetServiceName("QP8 Tarantool CDC Data Import");
                    factory.SetDisplayName("QP8 Tarantool CDC Data Import");
                    factory.SetDescription("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
                    factory.SetHelpTextPrefix("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
                });
            });
        }
    }
}
