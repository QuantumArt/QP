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
        private const string CleanupNlogPath = "NLog.Notifications.Cleanup.config";
        private const string NotificationsNlogPath = "NLog.Notifications.config";

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
            Container.RegisterType<IPrtgNLogFactory>(CleanupNlogPath, new InjectionFactory(container => GetLoggerFactory(CleanupNlogPath)));
            Container.RegisterType<CleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new CleanupProcessor(
                    Container.Resolve<IPrtgNLogFactory>(CleanupNlogPath).GetLogger(assemblyType),
                    new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(CleanupNlogPath)),
                    c.Resolve<ISchedulerCustomers>(),
                    c.Resolve<IExternalNotificationService>()
               )
           ));
        }

        private void RegisterNotificationsProcessor()
        {
            var assemblyType = typeof(NotificationProcessor);
            Container.RegisterType<IPrtgNLogFactory>(NotificationsNlogPath, new InjectionFactory(container => GetLoggerFactory(NotificationsNlogPath)));
            Container.RegisterType<NotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new NotificationProcessor(
                    Container.Resolve<IPrtgNLogFactory>(NotificationsNlogPath).GetLogger(assemblyType),
                    new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(NotificationsNlogPath)),
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
    }
}
