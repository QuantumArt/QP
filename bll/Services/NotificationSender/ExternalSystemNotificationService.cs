using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Repository.NotificationSender;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class ExternalSystemNotificationService : IExternalSystemNotificationService
    {
        public IEnumerable<SystemNotification> GetPendingNotifications()
        {
            return SystemNotificationRepository.GetPendingNotifications();
        }

        public void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            SystemNotificationRepository.UpdateSentNotifications(notificationIds);
        }

        public void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            SystemNotificationRepository.UpdateUnsentNotifications(notificationIds);
        }

        public void DeleteSentNotifications()
        {
            SystemNotificationRepository.DeleteSentNotifications();
        }

        public bool ExistsSentNotifications()
        {
            return SystemNotificationRepository.ExistsSentNotifications();
        }
    }
}
