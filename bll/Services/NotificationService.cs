using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public interface INotificationService
    {
        void SendNotification(string connectionString, int id, string code);

        ListResult<NotificationListItem> GetNotificationsByContentId(ListCommand cmd, int parentId);

        Notification NewNotificationProperties(int contentId);

        Notification ReadNotificationProperties(int id);

        Notification SaveNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl);

        Notification UpdateNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl);

        IEnumerable<ListItem> GetStatusesAsListItemsBySiteId(int siteId);

        IEnumerable<ListItem> GetObjectFormatsAsListItemsByContentId(int contentId);

        IEnumerable<ListItem> GetStringFieldsAsListItemsByContentId(int contentId);

        MessageResult UnbindNotification(int notificationId);

        MessageResult Remove(int id);

        NotificationObjectFormat ReadNotificationTemplateFormat(int id);

        PageTemplate ReadPageTemplateByObjectFormatId(int id);

        NotificationObjectFormat UpdateNotificationTemplateFormat(NotificationObjectFormat item);

        NotificationObjectFormat ReadNotificationTemplateFormatForUpdate(int id);

        Notification ReadNotificationPropertiesForUpdate(int id);

        Notification NewNotificationPropertiesForUpdate(int parentId);

        InitListResult InitList(int parentId);

        MessageResult MultipleRemove(int[] ids);

        MessageResult AssembleNotification(int id);

        MessageResult AssembleNotificationPreAction(int id);

        MessageResult MultipleAssembleNotificationPreAction(int[] ids);

        MessageResult MultipleAssembleNotification(int[] ids);

        bool IsSiteDotNetByObjectFormatId(int objectFormattId);

        IEnumerable<ListItem> GetTemplates();
    }

    public class NotificationService : INotificationService
    {
        public MessageResult AssembleNotificationPreAction(int id)
        {
            var site = NotificationRepository.GetPropertiesById(id).Content.Site;
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public MessageResult AssembleNotification(int id)
        {
            var notification = NotificationRepository.GetPropertiesById(id);

            if (notification.FormatId != null)
            {
                var cnt = new AssembleFormatController(notification.FormatId.Value, AssembleMode.Notification, QPContext.CurrentDbConnectionString);
                cnt.Assemble();
            }
            return null;
        }

        public MessageResult MultipleAssembleNotification(int[] ids)
        {
            var notifications = ids.Select(NotificationRepository.GetPropertiesById).ToList();

            foreach (var cnt in notifications.Select(notification =>
            {
                if (notification.FormatId != null)
                {
                    return new AssembleFormatController(notification.FormatId.Value, AssembleMode.Notification,
                        QPContext.CurrentDbConnectionString);
                }

                return null;
            }))
            {
                cnt.Assemble();
            }

            return null;
        }

        public MessageResult MultipleAssembleNotificationPreAction(int[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var site = NotificationRepository.GetPropertiesById(ids[0]).Content.Site;
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public MessageResult MultipleRemove(int[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            NotificationRepository.MultipleDelete(ids);

            return null;
        }

        public InitListResult InitList(int contentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewNotification) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update)
        };

        public Notification NewNotificationPropertiesForUpdate(int parentId) => NewNotificationProperties(parentId);

        public Notification ReadNotificationPropertiesForUpdate(int id) => ReadNotificationProperties(id);

        public NotificationObjectFormat ReadNotificationTemplateFormatForUpdate(int id) => ReadNotificationTemplateFormat(id);

        public NotificationObjectFormat UpdateNotificationTemplateFormat(NotificationObjectFormat item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return ObjectFormatRepository.UpdateNotificationTemplateFormat(item);
        }

        public NotificationObjectFormat ReadNotificationTemplateFormat(int id) => ObjectFormatRepository.ReadNotificationTemplateFormat(id);

        public MessageResult Remove(int id)
        {
            var notification = NotificationRepository.GetPropertiesById(id);
            if (notification == null)
            {
                throw new ApplicationException(string.Format(NotificationStrings.NotificationNotFound, id));
            }

            NotificationRepository.Delete(id);
            return null;
        }

        public IEnumerable<ListItem> GetStringFieldsAsListItemsByContentId(int contentId)
        {
            return FieldRepository.GetFullList(contentId).Where(f => f.TypeId == 1).Select(field => new ListItem
            {
                Text = field.Name,
                Value = field.Id.ToString()
            }).ToArray();
        }

        public IEnumerable<ListItem> GetObjectFormatsAsListItemsByContentId(int contentId) => ObjectFormatRepository.GetObjectFormats(0, contentId, new int[] { });

        public IEnumerable<ListItem> GetStatusesAsListItemsBySiteId(int siteId)
        {
            return StatusTypeRepository.GetStatusList(siteId).Select(status => new ListItem { Text = status.Name, Value = status.Id.ToString() })
                .ToArray();
        }

        public IEnumerable<ListItem> GetTemplates()
        {
            AppSettingsDAL setting = QPContext.EFContext.AppSettingsSet
               .FirstOrDefault(x => x.Key == "TEMPLATE_CONTENT");

            if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
            {
                throw new InvalidOperationException("Unable to find setting TemplatesContent in QP settings.");
            }

            if (!int.TryParse(setting.Value, out int contentId))
            {
                throw new InvalidOperationException($"Unable to parse template content id {setting.Value} as int");
            }

            IEnumerable<Article> articles = ArticleRepository.GetList(null, true, true, contentId);

            return articles.Select(x => new ListItem(x.Id.ToString(), x.Name));
        }

        public Notification UpdateNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl)
        {
            if (createDefaultFormat && !notification.FormatId.HasValue && !notification.IsExternal)
            {
                notification.FormatId = CreateDefaultFormat(notification.ContentId, backendUrl, QPContext.CurrentCustomerCode);
            }
            return NotificationRepository.UpdateProperties(notification);
        }

        public Notification SaveNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl)
        {
            if (ContentRepository.IsAnyAggregatedFields(notification.ParentEntityId))
            {
                throw new ActionNotAllowedException(ContentStrings.OperationIsNotAllowedForAggregated);
            }

            if (createDefaultFormat && !notification.IsExternal)
            {
                notification.FormatId = CreateDefaultFormat(notification.ContentId, backendUrl, QPContext.CurrentCustomerCode);
            }

            return NotificationRepository.SaveProperties(notification);
        }

        public Notification ReadNotificationProperties(int id)
        {
            var notification = NotificationRepository.GetPropertiesById(id);
            if (notification == null)
            {
                throw new ApplicationException(string.Format(NotificationStrings.NotificationNotFound, id));
            }

            return notification;
        }

        public Notification NewNotificationProperties(int contentId) => Notification.Create(contentId);

        public ListResult<NotificationListItem> GetNotificationsByContentId(ListCommand cmd, int contentId)
        {
            var list = NotificationRepository.List(cmd, contentId, out var totalRecords);
            return new ListResult<NotificationListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public void SendNotification(string connectionString, int id, string code)
        {
            Site site;
            using (new QPConnectionScope())
            {
                var article = ArticleRepository.GetById(id);
                if (article == null)
                {
                    throw new ArgumentException(string.Format(ArticleStrings.ArticleNotFound, id));
                }

                site = article.Content.Site;
            }

            var repository = new NotificationRepository();
            var codes = code.Split(';');
            foreach (var currentCode in codes)
            {
                repository.SendNotification(connectionString, site.Id, currentCode, id, site.IsLive || site.AssembleFormatsInLive);
            }
        }

        public MessageResult UnbindNotification(int notificationId)
        {
            var notification = ReadNotificationProperties(notificationId);
            notification.WorkFlowId = null;
            NotificationRepository.UpdateProperties(notification);
            return MessageResult.Info(NotificationStrings.UnbindedMessage);
        }

        public bool IsSiteDotNetByObjectFormatId(int objectFormatId) => ObjectFormatRepository.IsSiteDotNeByObjectFormatId(objectFormatId);

        private static int CreateDefaultFormat(int contentId, string backendUrl, string currentCustomerCode) => ObjectFormatRepository.CreateDefaultFormat(contentId, backendUrl, currentCustomerCode);

        public PageTemplate ReadPageTemplateByObjectFormatId(int id)
        {
            var frmt = ReadNotificationTemplateFormat(id);
            var obj = ObjectRepository.GetObjectPropertiesById(frmt.ObjectId);
            return PageTemplateRepository.GetPageTemplatePropertiesById(obj.PageTemplateId);
        }
    }
}
