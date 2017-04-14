using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ExternalNotificationRepository
    {
        internal static IEnumerable<ExternalNotification> GetPendingNotifications()
        {
            var notifications = QPContext.EFContext.ExternalNotificationSet.Where(n => !n.Sent).ToList();
            return MapperFacade.ExternalNotificationMapper.GetBizList(notifications);
        }

        internal static void DeleteSentNotifications()
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotifications.DeleteSentNotifications(QPConnectionScope.Current.DbConnection);
            }
        }

        internal static void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotifications.UpdateSentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        internal static void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotifications.UpdateUnsentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        internal static void Update(IEnumerable<ExternalNotification> notifications)
        {
            var entities = QPContext.EFContext;
            foreach (var notification in notifications)
            {
                var entity = MapperFacade.ExternalNotificationMapper.GetDalObject(notification);
                entities.ExternalNotificationSet.Attach(entity);
                entities.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
            }

            entities.SaveChanges();
        }

        internal static void Insert(IEnumerable<ExternalNotification> notifications)
        {
            using (new QPConnectionScope())
            {
                var xml = GetNotificationsXml(notifications);
                CommonExternalNotifications.InsertNotifications(QPConnectionScope.Current.DbConnection, xml.ToString(SaveOptions.None));
            }
        }

        private static XDocument GetNotificationsXml(IEnumerable<ExternalNotification> notifications)
        {
            var doc = new XDocument();
            var root = new XElement("Notifications");
            doc.Add(root);

            foreach (var notification in notifications)
            {
                var elem = new XElement("Notification");
                root.Add(elem);

                elem.Add(new XElement("EventName", notification.EventName));
                elem.Add(new XElement("ArticleId", notification.ArticleId));
                elem.Add(new XElement("ContentId", notification.ContentId));
                elem.Add(new XElement("SiteId", notification.SiteId));
                elem.Add(new XElement("Url", notification.Url));
                if (notification.NewXml != null)
                {
                    elem.Add(new XElement("NewXml", notification.NewXml));
                }
                if (notification.OldXml != null)
                {
                    elem.Add(new XElement("OldXml", notification.OldXml));
                }
            }

            return doc;
        }

        public static bool ExistsSentNotifications()
        {
            using (new QPConnectionScope())
            {
                return CommonExternalNotifications.ExistsSentNotifications(QPConnectionScope.Current.DbConnection);
            }
        }
    }
}
