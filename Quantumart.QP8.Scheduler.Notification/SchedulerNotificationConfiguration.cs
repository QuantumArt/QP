using Microsoft.Practices.Unity;
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
            Container.RegisterType<IExternalNotificationService, ExternalNotificationService>();
            Container.RegisterType<INotificationProvider, NotificationProvider>();
        }
    }
}
