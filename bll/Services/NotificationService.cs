using System;
using System.Collections.Generic;
using System.Linq;
using Assembling;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Exceptions;

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

		NotificationInitListResult InitList(int parentId);

		MessageResult MultipleRemove(int[] IDs);

		MessageResult AssembleNotification(int id);

		MessageResult AssembleNotificationPreAction(int id);

		MessageResult MultipleAssembleNotificationPreAction(int[] IDs);

		MessageResult MultipleAssembleNotification(int[] IDs);

		bool IsSiteDotNetByObjectFormatId(int objectFormattId);
	}

	public class NotificationService: INotificationService
	{
		public MessageResult AssembleNotificationPreAction(int id)
		{
			var site = NotificationRepository.GetPropertiesById(id).Content.Site;
			string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
			return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
		}

		public MessageResult AssembleNotification(int id)
		{
			var notification = NotificationRepository.GetPropertiesById(id);

			var cnt = new AssembleFormatController(notification.FormatId.Value, AssembleMode.Notification, QPContext.CurrentCustomerCode);
			cnt.Assemble();
			return null;
		}

		public MessageResult MultipleAssembleNotification(int[] IDs)
		{
			List<Notification> notifications = IDs.Select(NotificationRepository.GetPropertiesById).ToList();

		    foreach (var cnt in notifications.Select(notification => new AssembleFormatController(notification.FormatId.Value, AssembleMode.Notification, QPContext.CurrentCustomerCode)))
		    {
		        cnt.Assemble();
		    }

		    return null;
		}

		public MessageResult MultipleAssembleNotificationPreAction(int[] IDs)
		{
			if (IDs == null)
				throw new ArgumentNullException("IDs");
			var site = NotificationRepository.GetPropertiesById(IDs[0]).Content.Site;
			string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
			return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
		}

		public MessageResult MultipleRemove(int[] IDs)
		{
			if (IDs == null)
				throw new ArgumentNullException("IDs");
						
			NotificationRepository.MultipleDelete(IDs);
			
            return null;
		}

		public NotificationInitListResult InitList(int contentId)
		{
			return new NotificationInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewNotification) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update)
			};
		}

		public Notification NewNotificationPropertiesForUpdate(int parentId)
		{
			return NewNotificationProperties(parentId);
		}

		public Notification ReadNotificationPropertiesForUpdate(int id)
		{
			return ReadNotificationProperties(id);
		}

		public NotificationObjectFormat ReadNotificationTemplateFormatForUpdate(int id)
		{
			return ReadNotificationTemplateFormat(id);
		}

		public NotificationObjectFormat UpdateNotificationTemplateFormat(NotificationObjectFormat item)
		{
		    if (item == null)
		        throw new ArgumentNullException("item");
		    return ObjectFormatRepository.UpdateNotificationTemplateFormat(item);
		}

		public NotificationObjectFormat ReadNotificationTemplateFormat(int id)
		{
			return ObjectFormatRepository.ReadNotificationTemplateFormat(id);
		}

		public MessageResult Remove(int id)
		{
			Notification notification = NotificationRepository.GetPropertiesById(id);
			if (notification == null)
				throw new ApplicationException(String.Format(NotificationStrings.NotificationNotFound, id));
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

        public IEnumerable<ListItem> GetObjectFormatsAsListItemsByContentId(int contentId)
        {
            return ObjectFormatRepository.GetObjectFormats(0, contentId, new int[]{ });
        }

        public IEnumerable<ListItem> GetStatusesAsListItemsBySiteId(int siteId)
        {
            return StatusTypeRepository.GetStatusList(siteId).Select(status => new ListItem { Text = status.Name, Value = status.Id.ToString()})
                    .ToArray();
        }

        public Notification UpdateNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl)
        {
			if (createDefaultFormat  && !notification.FormatId.HasValue && !notification.IsExternal)
			{
				notification.FormatId = CreateDefaultFormat(notification.ContentId, backendUrl, QPContext.CurrentCustomerCode);				
			}
            return NotificationRepository.UpdateProperties(notification);
        }

        public Notification SaveNotificationProperties(Notification notification,  bool createDefaultFormat, string backendUrl)
        {
			if (ContentRepository.IsAnyAggregatedFields(notification.ParentEntityId))
				throw ActionNotAllowedException.CreateNotAllowedForAggregatedContentException();

			if (createDefaultFormat && !notification.IsExternal)
			{
				notification.FormatId = CreateDefaultFormat(notification.ContentId, backendUrl, QPContext.CurrentCustomerCode);				
			}
            Notification result = NotificationRepository.SaveProperties(notification);
			return result;
        }

        public Notification ReadNotificationProperties(int id)
        {
            Notification notification = NotificationRepository.GetPropertiesById(id);
            if (notification == null)
                throw new ApplicationException(String.Format(NotificationStrings.NotificationNotFound, id));
            return notification;
        }

        public Notification NewNotificationProperties(int contentId)
        {
            return Notification.Create(contentId);
        }

        public ListResult<NotificationListItem> GetNotificationsByContentId(ListCommand cmd, int contentId)
        {
            int totalRecords;
            IEnumerable<NotificationListItem> list = NotificationRepository.List(cmd, contentId, out totalRecords);
            return new ListResult<NotificationListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public void SendNotification(string connectionString, int id, string code)
        {
			Site site = null;
			using (new QPConnectionScope(connectionString))
			{
				Article article = ArticleRepository.GetById(id);
				if(article == null)
					throw new ArgumentException(String.Format(ArticleStrings.ArticleNotFound, id));
				site = article.Content.Site;
			}
			
			NotificationRepository repository = new NotificationRepository();
			string[] codes = code.Split(';');
			foreach (string currentCode in codes)
				repository.SendNotification(connectionString, site.Id, currentCode, id, site.IsLive || site.AssembleFormatsInLive);
		}

		public MessageResult UnbindNotification(int notificationId)
		{
			var notification = ReadNotificationProperties(notificationId);
			notification.WorkFlowId = null;
			NotificationRepository.UpdateProperties(notification);
			return MessageResult.Info(NotificationStrings.UnbindedMessage);			
		}

		public bool IsSiteDotNetByObjectFormatId(int objectFormatId)
		{
			return ObjectFormatRepository.IsSiteDotNeByObjectFormatId(objectFormatId);
		}

		private int CreateDefaultFormat(int contentId, string backendUrl, string currentCustomerCode)
		{
			return ObjectFormatRepository.CreateDefaultFormat(contentId, backendUrl, currentCustomerCode);
		}


		public PageTemplate ReadPageTemplateByObjectFormatId(int id)
		{
			var frmt = ReadNotificationTemplateFormat(id);
			var obj = ObjectRepository.GetObjectPropertiesById(frmt.ObjectId);
			return PageTemplateRepository.GetPageTemplatePropertiesById(obj.PageTemplateId);
		}
	}
}
