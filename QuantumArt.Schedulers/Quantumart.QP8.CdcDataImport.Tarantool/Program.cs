using QP8.Infrastructure.Logging.Factories;
using Quantumart.QP8.CdcDataImport.Common.Tarantool.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Topshelf;
using Topshelf.Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool
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
                factory.ScheduleQuartzJobAsService(cfg => JobConfigurator.GetConfigurationForJob<DataImportJob>(cfg, Settings.Default.RecurrentTimeout));
                factory.RunAsLocalSystem().StartAutomatically().EnableServiceRecovery(sr =>
                {
                    sr.OnCrashOnly();
                    sr.RestartService(0);
                    sr.SetResetPeriod(1);
                });

                factory.SetServiceName("qp8.cdc.tarantool");
                factory.SetDisplayName("QP8 Tarantool CDC Data Import");
                factory.SetDescription("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
                factory.SetHelpTextPrefix("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
            });
        }
    }
}
