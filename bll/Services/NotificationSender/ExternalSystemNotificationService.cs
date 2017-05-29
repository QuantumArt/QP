using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class ExternalSystemNotificationService : IExternalSystemNotificationService
    {
        public List<SystemNotificationModel> GetPendingNotifications()
        {
            var notifications = QPContext.EFContext.SystemNotificationSet.Where(entity => !entity.Sent).ToList();
            return MapperFacade.SystemNotificationMapper.GetBizList(notifications);
        }

        public bool ExistsSentNotifications()
        {
            return QPContext.EFContext.SystemNotificationSet.Any(entity => entity.Sent);
        }

        public bool ExistsUnsentNotifications()
        {
            return QPContext.EFContext.SystemNotificationSet.Any(entity => !entity.Sent);
        }

        public void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            CommonSystemNotificationsDal.UpdateSentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
        }

        public void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            CommonSystemNotificationsDal.UpdateUnsentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
        }

        public void DeleteSentNotifications()
        {
            CommonSystemNotificationsDal.DeleteSentNotifications(QPConnectionScope.Current.DbConnection);
        }

        public void InsertNotification(List<SystemNotificationModel> notifications)
        {
            var rawXml = XmlSerializerHelpers.Serialize(new SystemNotificationBulkXmlSerilizableModel
            {
                Notifications = notifications
            });

            CommonSystemNotificationsDal.InsertNotifications(QPConnectionScope.Current.DbConnection, rawXml);
        }
    }
}
