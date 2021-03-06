﻿using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public interface IExternalInterfaceNotificationService
    {
        List<ExternalNotificationModel> GetPendingNotifications();

        void UpdateSentNotifications(IEnumerable<int> notificationIds);

        void UpdateUnsentNotifications(IEnumerable<int> notificationIds);

        void DeleteSentNotifications();

        bool ExistsSentNotifications();

        void Insert(IEnumerable<ExternalNotificationModel> notifications);
    }
}
