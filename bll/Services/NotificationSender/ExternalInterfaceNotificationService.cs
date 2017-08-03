using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class ExternalInterfaceNotificationService : IExternalInterfaceNotificationService
    {
        public List<ExternalNotificationModel> GetPendingNotifications()
        {
            var notifications = QPContext.EFContext.ExternalNotificationSet.Where(n => !n.Sent).ToList();
            return MapperFacade.ExternalNotificationMapper.GetBizList(notifications);
        }

        public bool ExistsSentNotifications()
        {
            return QPContext.EFContext.ExternalNotificationSet.Any(n => n.Sent);
        }

        public void UpdateSentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotificationsDal.UpdateSentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        public void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotificationsDal.UpdateUnsentNotifications(QPConnectionScope.Current.DbConnection, notificationIds);
            }
        }

        public void DeleteSentNotifications()
        {
            using (new QPConnectionScope())
            {
                CommonExternalNotificationsDal.DeleteSentNotifications(QPConnectionScope.Current.DbConnection);
            }
        }

        public void Insert(IEnumerable<ExternalNotificationModel> notifications)
        {
            using (new QPConnectionScope())
            {
                var xml = GetNotificationsXml(notifications);
                CommonExternalNotificationsDal.InsertNotifications(QPConnectionScope.Current.DbConnection, xml.ToString(SaveOptions.None));
            }
        }

        private XDocument GetNotificationsXml(IEnumerable<ExternalNotificationModel> notifications)
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
    }
}
