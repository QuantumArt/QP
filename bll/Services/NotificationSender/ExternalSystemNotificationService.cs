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
        public List<SystemNotificationModel> GetPendingNotifications(string providerName)
        {
            var notifications = QPContext.EFContext.SystemNotificationSet
                .Where(entity => !entity.Sent && entity.CdcLastExecutedLsn.ProviderName == providerName)
                .ToList();

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

        public bool ExistsUnsentNotifications(string providerUrl)
        {
            return QPContext.EFContext.SystemNotificationSet.Any(entity => entity.CdcLastExecutedLsn.ProviderUrl == providerUrl && !entity.Sent);
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

        public void InsertNotification(IEnumerable<SystemNotificationModel> notifications)
        {
            var rawXml = XmlSerializerHelpers.Serialize(new SystemNotificationBulkXmlSerilizableModel
            {
                Notifications = notifications.ToList()
            });

            CommonSystemNotificationsDal.InsertNotifications(QPConnectionScope.Current.DbConnection, rawXml);
        }
    }
}
