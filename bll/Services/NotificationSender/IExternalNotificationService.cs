using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public interface IExternalNotificationService
    {
        IEnumerable<ExternalNotification> GetPendingNotifications();

        void UpdateSentNotifications(IEnumerable<int> notificationIds);

        void UpdateUnsentNotifications(IEnumerable<int> notificationIds);

        void DeleteSentNotifications();

        bool ExistsSentNotifications();
    }
}
