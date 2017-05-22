using QP8.Infrastructure.Logging.Factories;
using Quantumart.QP8.CdcDataImport.Common;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs;
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
                factory.Service<CdcServiceHost>(service =>
                {
                    service.ConstructUsing(settings => new CdcServiceHost());
                    service.WhenStarted(host => host.Start());
                    service.WhenStopped(host => host.Stop());
                    service.UsingQuartzJobFactory(() => new QuartzJobFactory());
                    service.ScheduleQuartzJob(cfg => JobConfigurator.GetConfigurationForJob<CdcDataImportJob>(cfg, Settings.Default.CdcRecurrentTimeout));
                    service.ScheduleQuartzJob(cfg => JobConfigurator.GetConfigurationForJob<CheckNotificationQueueJob>(cfg, Settings.Default.CheckNotificationsQueueRecurrentTimeout));
                });

                factory.RunAsLocalSystem().StartAutomatically().EnableServiceRecovery(sr =>
                {
                    sr.OnCrashOnly();
                    sr.RestartService(0);
                    sr.SetResetPeriod(1);
                }).EnableShutdown();

                factory.SetServiceName("qp8.cdc.tarantool");
                factory.SetDisplayName("QP8 Tarantool CDC Data Import");
                factory.SetDescription("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
                factory.SetHelpTextPrefix("Import data from CDC data tables and push them to notification service queue (using Tarantool formatting)");
            });
        }
    }
}
