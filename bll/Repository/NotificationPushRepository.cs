using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;

namespace Quantumart.QP8.BLL.Repository
{
    internal class NotificationPushRepository
    {
        #region Private fields and properties
        private int ContentId { get; set; }

        private int SiteId { get; set; }

        private int[] ArticleIds { get; set; }

        private Article[] Articles { get; set; }

        private Notification[] Notifications { get; set; }

        private Notification[] ServiceNotifications => Notifications.Where(n => n.UseService).ToArray();

        private Notification[] NonServiceNotifications => Notifications.Where(n => !n.UseService).ToArray();

        public string ConnectionString { get; set; } = QPContext.CurrentDbConnectionString;


        private string[] Codes { get; set; }

        #endregion

        #region public properties

        /// <summary>
        /// Do not send internal e-mail notifications
        /// </summary>
        public bool IgnoreInternal { get; set; }

        #endregion

        #region Constructor
        internal NotificationPushRepository()
        {
            Articles = new Article[0];
            Notifications = new Notification[0];
        }
        #endregion

        #region Internal methods
        internal void SendNotificationOneWay(string connectionString, int id, string code)
        {
            QueueAsync(() => Notify(connectionString, id, code, IgnoreInternal));
        }

        internal void SendNotification(string connectionString, int id, string code)
        {
            Queue(() => Notify(connectionString, id, code, IgnoreInternal));
        }

        internal void PrepareNotifications(Article article, string[] codes, bool disableNotifications = false)
        {
            Notifications = new Notification[0];
            ArticleIds = new int[0];
            Articles = new Article[0];
            ValidateCodes(codes);
            Codes = codes.Distinct().ToArray();
            if (!disableNotifications)
            {
                ContentId = article.ContentId;
                SiteId = article.Content.SiteId;
                Notifications = NotificationRepository.GetContentNotifications(ContentId, codes).ToArray();
                if (Notifications.Any())
                {
                    Articles = new[] { article };
                    ArticleIds = new[] { article.Id };
                }
            }
        }

        internal void PrepareNotifications(int contentId, int[] articleIds, string[] codes, bool disableNotifications = false)
        {
            Notifications = new Notification[0];
            ArticleIds = new int[0];
            Articles = new Article[0];
            ValidateCodes(codes);
            Codes = codes.Distinct().ToArray();
            if (!disableNotifications)
            {
                ContentId = contentId;
                SiteId = ContentRepository.GetSiteId(contentId);
                Notifications = NotificationRepository.GetContentNotifications(contentId, codes).ToArray();
                if (Notifications.Any())
                {
                    ArticleIds = articleIds;
                    if (!Codes.Contains(NotificationCode.Create) && ServiceNotifications.Any())
                    {
                        Articles = GetArticles();
                    }
                }
            }
        }

        internal void PrepareNotifications(int contentId, int[] ids, string code, bool disableNotifications = false)
        {
            PrepareNotifications(contentId, ids, code.Split(';'), disableNotifications);
        }

        internal void SendNotifications()
        {
            SendServiceNotifications();
            SendNonServiceNotifications(false);
        }

        internal void SendNonServiceNotifications(bool waitFor)
        {
            if (!NonServiceNotifications.Any()) return;
            foreach (var item in ArticleIds)
            {
                if (waitFor)
                    SendNotification(ConnectionString, item, string.Join(";", Codes));
                else
                    SendNotificationOneWay(ConnectionString, item, string.Join(";", Codes));
            }
        }


        internal void SendServiceNotifications()
        {
            if (!ServiceNotifications.Any()) return;
            try
            {
                IEnumerable<ExternalNotification> notificationQueue;
                if (Codes.Contains(NotificationCode.Delete))
                {
                    notificationQueue = from notification in ServiceNotifications
                                        join article in Articles on notification.ContentId equals article.ContentId
                                        select new ExternalNotification
                                        {
                                            EventName = NotificationCode.Delete,
                                            ArticleId = article.Id,
                                            ContentId = ContentId,
                                            SiteId = SiteId,
                                            Url = notification.ExternalUrl,
                                            OldXml = GetXDocument(article).ToString(),
                                            NewXml = null
                                        };

                }
                else if (Codes.Contains(NotificationCode.Create))
                {
                    notificationQueue = from notification in ServiceNotifications
                                        join article in GetArticles() on notification.ContentId equals article.ContentId
                                        select new ExternalNotification
                                        {
                                            EventName = NotificationCode.Create,
                                            ArticleId = article.Id,
                                            ContentId = ContentId,
                                            SiteId = SiteId,
                                            Url = notification.ExternalUrl,
                                            OldXml = null,
                                            NewXml = GetXDocument(article).ToString(),
                                        };
                }
                else
                {
                    var oldArticles = Articles;
                    var newArticles = GetArticles();

                    notificationQueue = from notification in ServiceNotifications
                                        from code in Codes
                                        join oldArticle in oldArticles on notification.ContentId equals oldArticle.ContentId
                                        join newArticle in newArticles on oldArticle.Id equals newArticle.Id
                                        where
                                            notification.ForModify && code == NotificationCode.Update ||
                                            notification.ForFrontend && code == NotificationCode.Custom ||
                                            notification.ForStatusChanged && code == NotificationCode.ChangeStatus ||
                                            notification.ForStatusPartiallyChanged && code == NotificationCode.PartialChangeStatus ||
                                            notification.ForDelayedPublication && code == NotificationCode.DelayedPublication
                                        select new ExternalNotification
                                        {
                                            EventName = code,
                                            ArticleId = oldArticle.Id,
                                            ContentId = ContentId,
                                            SiteId = SiteId,
                                            Url = notification.ExternalUrl,
                                            OldXml = GetXDocument(oldArticle).ToString(),
                                            NewXml = GetXDocument(newArticle).ToString()
                                        };
                }

                ExternalNotificationRepository.Insert(notificationQueue);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #region Private methods

        private static void QueueAsync(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => action());
        }

        private static void Queue(Action action)
        {
            var evt = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                action();
                evt.Set();
            });
            evt.WaitOne();
        }

        private void Notify(string connectionString, int id, string code, bool disableInternalNotifications)
        {
            var cnn = new DBConnector(connectionString)
            {
                CacheData = false,
                DisableServiceNotifications = true,
                DisableInternalNotifications = disableInternalNotifications,
                ExternalExceptionHandler = HandleException,
                ThrowNotificationExceptions = false
            };
            foreach (var simpleCode in code.Split(';'))
            {
                cnn.SendNotification(id, simpleCode);
            }
        }

        private static XDocument GetXDocument(Article article)
        {
            var doc = new XDocument(new XElement("article", new XAttribute("id", article.Id)));
            if (doc.Root != null)
            {
                doc.Root.Add(new XElement("created", (article.Created).ToString(CultureInfo.InvariantCulture)));
                doc.Root.Add(new XElement("modified", (article.Modified).ToString(CultureInfo.InvariantCulture)));
                doc.Root.Add(new XElement("contentId", article.ContentId));
                doc.Root.Add(new XElement("siteId", article.Content.SiteId));
                doc.Root.Add(new XElement("visible", article.Visible));
                doc.Root.Add(new XElement("archive", article.Archived));
                doc.Root.Add(new XElement("splitted", article.Splitted));
                doc.Root.Add(new XElement("statusName", article.Status.Name));
                doc.Root.Add(new XElement("lastModifiedBy", article.LastModifiedBy));
                doc.Root.Add(GetFieldValuesElement(article.FieldValues));
                var extRoot = new XElement("extensions");
                foreach (var item in article.AggregatedArticles)
                {
                    extRoot.Add(new XElement("extension", new XAttribute("typeId", item.ContentId), new XAttribute("id", item.Id), GetFieldValuesElement(item.FieldValues)));
                }
                doc.Root.Add(extRoot);
            }
            return doc;
        }

        private static XElement GetFieldValuesElement(IEnumerable<FieldValue> fieldValues)
        {
            var fields = new XElement("customFields");

            foreach (var fieldValue in fieldValues)
            {
                string value;

                if (fieldValue.RelatedItems != null && fieldValue.RelatedItems.Any())
                {
                    value = string.Join(",", fieldValue.RelatedItems);
                }
                else
                {
                    value = fieldValue.Value;
                }

                fields.Add(new XElement("field", new XAttribute("name", fieldValue.Field.Name), new XAttribute("id", fieldValue.Field.Id), value));
            }
            return fields;
        }

        private Article[] GetArticles()
        {
            var articles = ArticleRepository.GetList(ArticleIds, true);
            return ContentId == 0 ? articles.Where(a => a.ContentId == ContentId).ToArray() : articles.ToArray();
        }

        private void ValidateCodes(IEnumerable<string> codes)
        {
            if (codes == null)
            {
                throw new ArgumentNullException(nameof(codes));
            }

            var enumerable = codes as string[] ?? codes.ToArray();
            var count = enumerable.Length;

            if (!enumerable.Any())
            {
                throw new ArgumentException(@"no codes defined", nameof(codes));
            }
            if (enumerable.Any(c => c == NotificationCode.Create || c == NotificationCode.Delete) && count > 1)
            {
                throw new ArgumentException($"codes {NotificationCode.Create} and {NotificationCode.Delete} can't be used with othes codes", nameof(codes));
            }

            string[] availableCodes =
            {
                NotificationCode.Create,
                NotificationCode.Update,
                NotificationCode.Delete,
                NotificationCode.ChangeStatus,
                NotificationCode.PartialChangeStatus,
                NotificationCode.Custom,
                NotificationCode.DelayedPublication
            };

            var wrongCodes = enumerable.Select(c => c.ToLowerInvariant()).Except(availableCodes).ToArray();

            if (wrongCodes.Any())
            {
                throw new ArgumentException($"codes {string.Join(", ", wrongCodes)} is not valid", nameof(codes));
            }
        }

        private void HandleException(Exception ex)
        {
            EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(ex, "Policy");
        }
        #endregion
    }
}
