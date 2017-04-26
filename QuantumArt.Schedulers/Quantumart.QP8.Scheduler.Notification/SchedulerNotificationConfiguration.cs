using System;
using System.IO;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Providers;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class SchedulerNotificationConfiguration : UnityContainerExtension
    {
        public const string ServiceName = "qp8.notification";
        private const string CleanupNlogConfigName = "NLog.Notifications.Cleanup.config";
        private const string NotificationsNlogConfigName = "NLog.Notifications.config";

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Notification", "Отправка уведомлений");

            Container.RegisterType<INotificationProvider, NotificationProvider>();
            Container.RegisterType<IExternalNotificationService, ExternalNotificationService>();

            Container.RegisterProcessor<CleanupProcessor>(ServiceName, "CleanupNotificationQueueSchedule");
            Container.RegisterProcessor<NotificationProcessor>(ServiceName, "NotificationSchedule");

            RegisterCleanupProcessor();
            RegisterNotificationsProcessor();
        }

        private void RegisterCleanupProcessor()
        {
            var assemblyType = typeof(CleanupProcessor);
            var nlogInjectionFactory = new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(CleanupNlogConfigName)));
            Container.RegisterType<IPrtgNLogFactory>(CleanupNlogConfigName, nlogInjectionFactory);
            Container.RegisterType<CleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new CleanupProcessor(
                        Container.Resolve<IPrtgNLogFactory>(CleanupNlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(CleanupNlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalNotificationService>()
                    )
                ));
        }

        private void RegisterNotificationsProcessor()
        {
            var assemblyType = typeof(NotificationProcessor);
            var nlogInjectionFactory = new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(NotificationsNlogConfigName)));
            Container.RegisterType<IPrtgNLogFactory>(NotificationsNlogConfigName, nlogInjectionFactory);
            Container.RegisterType<NotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new NotificationProcessor(
                        Container.Resolve<IPrtgNLogFactory>(NotificationsNlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(NotificationsNlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalNotificationService>(),
                        c.Resolve<INotificationProvider>()
                    )
                ));
        }

        private static IPrtgNLogFactory GetLoggerFactory(string configPath)
        {
            return new PrtgNLogFactory(
                configPath,
                LoggerData.DefaultPrtgServiceStateVariableName,
                LoggerData.DefaultPrtgServiceQueueVariableName,
                LoggerData.DefaultPrtgServiceStatusVariableName
            );
        }

        private static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
