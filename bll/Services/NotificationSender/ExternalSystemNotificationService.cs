using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class ExternalSystemNotificationService : IExternalNotificationService
    {
        public IEnumerable<ExternalNotification> GetPendingNotifications()
        {
            return ExternalNotificationRepository.GetPendingNotifications();
        }

        public void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            ExternalNotificationRepository.UpdateSentNotifications(notificationIds);
        }

        public void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            ExternalNotificationRepository.UpdateUnsentNotifications(notificationIds);
        }

        public void DeleteSentNotifications()
        {
            ExternalNotificationRepository.DeleteSentNotifications();
        }

        public bool ExistsSentNotifications()
        {
            return ExternalNotificationRepository.ExistsSentNotifications();
        }
    }
}
