using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Providers;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class SchedulerNotificationConfiguration : UnityContainerExtension
    {
        public const string NotificationService = "qp8.notification";

        protected override void Initialize()
        {
            Container.RegisterService(NotificationService, "QP8 Notification", "Отправка уведомлений");
            Container.RegisterProcessor<NotificationProcessor>(NotificationService, "NotificationSchedule");
            Container.RegisterProcessor<CleanupProcessor>(NotificationService, "CleanupNotificationQueueSchedule");

            var assemblyType = typeof(NotificationProcessor);
            Container.RegisterType<IProcessor, NotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new NotificationProcessor(
                    LogProvider.LogFactory.GetLogger(assemblyType),
                    c.Resolve<ISchedulerCustomers>(),
                    c.Resolve<IExternalNotificationService>(),
                    c.Resolve<INotificationProvider>()
               )
           ));

            var descriptor = new ProcessorDescriptor(assemblyType.Name, NotificationService, "UserSynchronizationSchedule");
            Container.RegisterInstance(descriptor.Processor, descriptor);

            Container.RegisterType<IExternalNotificationService, ExternalNotificationService>();
            Container.RegisterType<INotificationProvider, NotificationProvider>();
        }
    }
}
