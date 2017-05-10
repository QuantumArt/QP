using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace Quantumart.QP8.BLL.Repository.NotificationSender
{
    internal class SystemNotificationRepository
    {
        internal static IEnumerable<SystemNotification> GetPendingNotifications()
        {
            var notifications = QPContext.EFContext.SystemNotificationSet.Where(n => !n.Sent).ToList();
            return MapperFacade.SystemNotificationMapper.GetBizList(notifications);
        }

        internal static void DeleteSentNotifications()
        {
            using (new QPConnectionScope())
            {
                CommonSystemNotificationsDal.DeleteSentNotifications(QPConnectionScope.Current.DbConnection);
            }
        }

        internal static void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonSystemNotificationsDal.UpdateSentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        internal static void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonSystemNotificationsDal.UpdateUnsentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        internal static void Update(IEnumerable<SystemNotification> notifications)
        {
            var entities = QPContext.EFContext;
            foreach (var notification in notifications)
            {
                var entity = MapperFacade.SystemNotificationMapper.GetDalObject(notification);
                entities.SystemNotificationSet.Attach(entity);
                entities.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
            }

            entities.SaveChanges();
        }

        internal static void Insert(IEnumerable<SystemNotification> notifications)
        {
            using (new QPConnectionScope())
            {
                var xml = GetNotificationsXml(notifications);
                CommonSystemNotificationsDal.InsertNotifications(QPConnectionScope.Current.DbConnection, xml.ToString(SaveOptions.None));
            }
        }

        private static XDocument GetNotificationsXml(IEnumerable<SystemNotification> notifications)
        {
            var doc = new XDocument();
            var root = new XElement("Notifications");
            doc.Add(root);

            foreach (var notification in notifications)
            {
                var elem = new XElement("Notification");
                root.Add(elem);

                elem.Add(new XElement("EVENT", notification.Event));
                elem.Add(new XElement("TYPE", notification.Type));
                elem.Add(new XElement("TRANSACTION_DATE", notification.TransactionDate));
                elem.Add(new XElement("JSON", notification.Json));
                elem.Add(new XElement("Url", notification.Url));
            }

            return doc;
        }

        public static bool ExistsSentNotifications()
        {
            using (new QPConnectionScope())
            {
                return CommonSystemNotificationsDal.ExistsSentNotifications(QPConnectionScope.Current.DbConnection);
            }
        }
    }
}
