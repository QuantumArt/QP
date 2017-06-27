using System;
using System.Collections.Specialized;
using Autofac;
using Autofac.Extras.Quartz;
using AutoMapper;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.IoC;
using Quantumart.QP8.CdcDataImport.Common;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Quantumart.QP8.Constants;
using Quartz;
using Topshelf;
using Topshelf.Autofac;
using Topshelf.Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool
{
    internal static class Program
    {
        private static readonly IContainer Container;

        static Program()
        {
            Container = BuildContainer();
        }

        private static void Main()
        {
            Init();
            HostFactory.Run(factory =>
            {
                factory.UseNLog();
                factory.UseAutofacContainer(Container);
                factory.Service<CdcServiceHost>(service =>
                {
                    service.ConstructUsingAutofacContainer();
                    service.WhenStarted((sc, hc) => sc.Start(hc));
                    service.WhenStopped((sc, hc) =>
                    {
                        Container.Dispose();
                        return sc.Stop(hc);
                    });

                    service.ScheduleQuartzJob(cfg => JobConfigurator.GetConfigurationForJob<CdcDataImportJob>(cfg, Settings.Default.CdcRecurrentTimeout));
                    service.ScheduleQuartzJob(cfg => JobConfigurator.GetConfigurationForJob<CheckNotificationQueueJob>(cfg, Settings.Default.CheckNotificationsQueueRecurrentTimeout));
                });

                factory.RunAsLocalSystem().StartAutomaticallyDelayed().EnableServiceRecovery(sr =>
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

        private static void Init()
        {
            LogProvider.LogFactory = Container.Resolve<INLogFactory>();
            Logger.Log.SetGlobalContext(LoggerData.AppNameCustomVariable, "QP.CdcDataImport.Tarantool");

            ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = Container.Resolve<IScheduler>;
            AppDomain.CurrentDomain.UnhandledException += (o, ea) =>
            {
                Logger.Log.Fatal("Unhandled application exception: ", ea.ExceptionObject as Exception);
                Logger.Log.Dispose();
            };

            Mapper.Initialize(cfg => { cfg.AddProfile<TarantoolMapperProfile>(); });
            Mapper.AssertConfigurationIsValid();
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c => new NameValueCollection
                {
                    { "quartz.threadPool.threadCount", "4" },
                    { "quartz.threadPool.threadNamePrefix", "CdcTarantoolSchedulerWorker" },
                    { "quartz.scheduler.threadName", "CdcTarantoolScheduler" }
                }
            });

            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(CdcDataImportJob).Assembly));
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(CheckNotificationQueueJob).Assembly));

            builder.RegisterModule<NLogAutofacModule>();
            builder.RegisterModule<IoCTarantoolModule>();
            return builder.Build();
        }
    }
}
