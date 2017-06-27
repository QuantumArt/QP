using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public interface IExternalSystemNotificationService
    {
        List<SystemNotificationModel> GetPendingNotifications();

        void UpdateSentNotifications(IEnumerable<int> notificationIds);

        void UpdateUnsentNotifications(IEnumerable<int> notificationIds);

        void DeleteSentNotifications();

        bool ExistsSentNotifications();

        bool ExistsUnsentNotifications();

        bool ExistsUnsentNotifications(string providerUrl);

        void InsertNotification(IEnumerable<SystemNotificationModel> notifications);
    }
}
