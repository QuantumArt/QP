using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;
using Quantumart.QP8.Configuration;
using Quantumart.QPublishing.FileSystem;

namespace Quantumart.QP8.BLL.Repository
{
    internal class NotificationPushRepository
    {

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private int ContentId { get; set; }

        private int SiteId { get; set; }

        private int[] ArticleIds { get; set; }

        private Article[] Articles { get; set; }

        private Notification[] Notifications { get; set; }

        private IEnumerable<Notification> ServiceNotifications => Notifications.Where(n => n.UseService).ToArray();

        private IEnumerable<Notification> NonServiceNotifications => Notifications.Where(n => !n.UseService).ToArray();

        public string ConnectionString { get; set; } = QPContext.CurrentDbConnectionString;

        private string[] Codes { get; set; }

        public bool IgnoreInternal { get; set; }

        private readonly S3Options _options;

        internal NotificationPushRepository() : this(new S3Options())
        {
        }

        internal NotificationPushRepository(S3Options options)
        {
            Articles = Array.Empty<Article>();
            Notifications = Array.Empty<Notification>();
            _options = options;
        }

        internal void PrepareNotifications(Article article, string[] codes, bool disableNotifications = false)
        {
            Notifications = Array.Empty<Notification>();
            ArticleIds = Array.Empty<int>();
            Articles = Array.Empty<Article>();
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
            Notifications = Array.Empty<Notification>();
            ArticleIds = Array.Empty<int>();
            Articles = Array.Empty<Article>();
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

        internal void SendBatchNotification()
        {
            try
            {
                SendServiceNotifications();
                SendInternalNotificationsBatch();
            }
            catch (Exception e)
            {
                HandleException(e);

                throw;
            }
        }

        internal void SendNonServiceNotifications(bool waitFor)
        {
            if (!NonServiceNotifications.Any())
            {
                return;
            }

            foreach (var item in ArticleIds)
            {
                var task = Task.Run(() => Notify(ConnectionString, item, string.Join(";", Codes), IgnoreInternal));
                if (waitFor)
                {
                   task.Wait();
                }
            }
        }

        internal void SendServiceNotifications()
        {
            if (!ServiceNotifications.Any())
            {
                return;
            }

            try
            {
                IEnumerable<ExternalNotificationModel> notificationQueue;
                if (Codes.Contains(NotificationCode.Delete))
                {
                    notificationQueue = from notification in ServiceNotifications
                        join article in Articles on notification.ContentId equals article.ContentId
                        select new ExternalNotificationModel
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
                        select new ExternalNotificationModel
                        {
                            EventName = NotificationCode.Create,
                            ArticleId = article.Id,
                            ContentId = ContentId,
                            SiteId = SiteId,
                            Url = notification.ExternalUrl,
                            OldXml = null,
                            NewXml = GetXDocument(article).ToString()
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
                        select new ExternalNotificationModel
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

                new ExternalInterfaceNotificationService().Insert(notificationQueue);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void SendInternalNotificationsBatch()
        {
            if (IgnoreInternal)
            {
                return;
            }

            DBConnector dbConnector = new(ConnectionString, (QP.ConfigurationService.Models.DatabaseType)QPContext.DatabaseType)
            {
                CacheData = false,
                DisableServiceNotifications = true,
                DisableInternalNotifications = false,
                ExternalExceptionHandler = HandleException,
                ThrowNotificationExceptions = true,
                WithTransaction = false,
                ExternalConnection = QPContext.CurrentConnectionScope.DbConnection
            };

            if (_options != null && !string.IsNullOrWhiteSpace(_options.Endpoint))
            {
                dbConnector.FileSystem = new S3FileSystem(_options.Endpoint, _options.AccessKey, _options.SecretKey, _options.Bucket);
            }

            QPConfiguration.SetAppSettings(dbConnector.DbConnectorSettings);

            try
            {
                foreach (string simpleCode in Codes)
                {
                    dbConnector.SendInternalNotificationBatch(simpleCode, ArticleIds);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);

                throw;
            }
        }

        private void Notify(string connectionString, int id, string code, bool disableInternalNotifications)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var cnn = new DBConnector(connectionString, (QP.ConfigurationService.Models.DatabaseType)QPContext.DatabaseType)
                {
                    CacheData = false,
                    DisableServiceNotifications = true,
                    DisableInternalNotifications = disableInternalNotifications,
                    ExternalExceptionHandler = HandleException,
                    ThrowNotificationExceptions = false
                };

                if (_options != null && !string.IsNullOrWhiteSpace(_options.Endpoint))
                {
                    cnn.FileSystem = new S3FileSystem(_options.Endpoint, _options.AccessKey, _options.SecretKey, _options.Bucket);
                }

                QPConfiguration.SetAppSettings(cnn.DbConnectorSettings);

                try
                {
                    foreach (var simpleCode in code.Split(';'))
                    {
                        cnn.SendNotification(id, simpleCode);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        private static XDocument GetXDocument(Article article)
        {
            var doc = new XDocument(new XElement("article", new XAttribute("id", article.Id)));
            if (doc.Root != null)
            {
                doc.Root.Add(new XElement("created", article.Created.ToString(CultureInfo.InvariantCulture)));
                doc.Root.Add(new XElement("modified", article.Modified.ToString(CultureInfo.InvariantCulture)));
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
            return ContentId != 0 ? articles.Where(a => a.ContentId == ContentId).ToArray() : articles.ToArray();
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
                throw new ArgumentException($@"codes {NotificationCode.Create} and {NotificationCode.Delete} can't be used with othes codes", nameof(codes));
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
                throw new ArgumentException($@"codes {string.Join(", ", wrongCodes)} is not valid", nameof(codes));
            }
        }

        private static void HandleException(Exception ex)
        {
            Logger.Error().Exception(ex).Message("Notification service error").Write();
        }
    }
}
