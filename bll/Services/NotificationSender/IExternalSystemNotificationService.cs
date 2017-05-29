using Quantumart.QP8.BLL.Models.NotificationSender;
using System.Collections.Generic;

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

        void InsertNotification(List<SystemNotificationModel> notifications);
    }
}
