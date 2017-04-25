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
            var childContainer = Container.CreateChildContainer();
            childContainer.RegisterType<IPrtgNLogFactory>(new InjectionFactory(container => GetLoggerFactory("NLog.Notifications.Cleanup.config")));

            var assemblyType = typeof(CleanupProcessor);
            Container.RegisterType<CleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new CleanupProcessor(
                    childContainer.Resolve<IPrtgNLogFactory>().GetLogger(assemblyType),
                    new PrtgErrorsHandler(childContainer.Resolve<IPrtgNLogFactory>()),
                    c.Resolve<ISchedulerCustomers>(),
                    c.Resolve<IExternalNotificationService>()
               )
           ));
        }

        private void RegisterNotificationsProcessor()
        {
            var childContainer = Container.CreateChildContainer();
            childContainer.RegisterType<IPrtgNLogFactory>(new InjectionFactory(container => GetLoggerFactory("NLog.Notifications.config")));

            var assemblyType = typeof(NotificationProcessor);
            Container.RegisterType<NotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new NotificationProcessor(
                    childContainer.Resolve<IPrtgNLogFactory>().GetLogger(assemblyType),
                    new PrtgErrorsHandler(childContainer.Resolve<IPrtgNLogFactory>()),
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
