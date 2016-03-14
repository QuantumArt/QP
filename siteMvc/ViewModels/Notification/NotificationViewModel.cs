using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;


namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
    public class NotificationViewModel: EntityViewModel
    {

		private INotificationService _service;
		private int _contentId;
        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Notification; }            
        }

        public override string ActionCode
        {
            get { return IsNew ? C.ActionCode.AddNewNotification : C.ActionCode.NotificationProperties; }
        }

        public new BLL.Notification Data
        {
            get
            {
                return (BLL.Notification)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

		private List<B.ListItem> _formats;
        public List<B.ListItem> Formats
		{
			get
			{
				if (_formats == null)
				{
					_formats = _service.GetObjectFormatsAsListItemsByContentId(_contentId).ToList();
					_formats.Insert(0, new ListItem { Text = NotificationStrings.ChooseFormat, Value = string.Empty });
				}
				return _formats;
			}
			set
			{
				_formats = value;
			}
		}

		private List<B.ListItem> _statuses;
        public List<B.ListItem> Statuses
		{
			get
			{
				if (_statuses == null)
				{
					_statuses = _service.GetStatusesAsListItemsBySiteId(Data.Content.SiteId).ToList();
					_statuses.Insert(0, new ListItem { Text = NotificationStrings.AnyStatus, Value = string.Empty });
				}
				return _statuses;
			}			
		}

		private List<B.ListItem> _fields;
        public List<B.ListItem> Fields 
		{
			get 
			{
				if (_fields == null)
				{
					_fields = _service.GetStringFieldsAsListItemsByContentId(_contentId).ToList();
					_fields.Insert(0, new ListItem { Text = NotificationStrings.ChooseField, Value = string.Empty });
				}
				return _fields;
			}			
		}		

		private bool _createDefaultFormat;		

		[LocalizedDisplayName("CreateDefaultFormat", NameResourceType = typeof(NotificationStrings))]
		public bool CreateDefaultFormat
		{
			get
			{
				return _createDefaultFormat;
			}
			set
			{
				_createDefaultFormat = value;
			}
		}

        public QPSelectListItem FromUserListItem
        {
            get
            {
				if (Data.FromBackenduserId == null)
					return null;
                return Data.FromUser != null ?
                    new QPSelectListItem { Value = Data.FromUser.Id.ToString(), Text = Data.FromUser.LogOn, Selected = true } :
					new QPSelectListItem { Value = SpecialIds.AdminUserId.ToString(), Text = "admin", Selected = true };
            }
        }

        public QPSelectListItem ToUserListItem
        {
            get
            {
                return Data.ToUser != null ?
                    new QPSelectListItem { Value = Data.ToUser.Id.ToString(), Text = Data.ToUser.LogOn, Selected = true } :
                    null;
            }
        }

        public QPSelectListItem ToUserGroupListItem
        {
            get
            {
                return Data.ToUserGroup != null ?
                    new QPSelectListItem { Value = Data.ToUserGroup.Id.ToString(), Text = Data.ToUserGroup.Name, Selected = true } :
                    null;
            }
        }

        public IEnumerable<B.ListItem> GetReceiverTypes()
        {
            return new[]
			{
				new B.ListItem(ReceiverType.User.ToString(), B.Content.GetReceiverTypeString(ReceiverType.User), "UserPanel"),
				new B.ListItem(ReceiverType.UserGroup.ToString(), B.Content.GetReceiverTypeString(ReceiverType.UserGroup), "UserGroupPanel"),
				new B.ListItem(ReceiverType.EveryoneInHistory.ToString(), B.Content.GetReceiverTypeString(ReceiverType.EveryoneInHistory), "EmptyPanel"),
                new B.ListItem(ReceiverType.EmailFromArticle.ToString(), B.Content.GetReceiverTypeString(ReceiverType.EmailFromArticle), "FieldPanel"),
                new B.ListItem(ReceiverType.None.ToString(), B.Content.GetReceiverTypeString(ReceiverType.None), "EmptyPanel")
			};
        }

		public SelectOptions SelectFormatOptions
		{
			get
			{
				SelectOptions options = new SelectOptions();
				options.EntityDataListArgs = new EntityDataListArgs();
				options.EntityDataListArgs.EntityTypeCode = Constants.EntityTypeCode.TemplateObjectFormat;
				options.EntityDataListArgs.ParentEntityId = Data.ContentId;
				options.EntityDataListArgs.EntityId = Data.Id;
				options.EntityDataListArgs.ListId = Data.ContentId;
				options.EntityDataListArgs.ReadActionCode = Constants.ActionCode.NotificationObjectFormatProperties;				
				return options;
			}
		}

        public static NotificationViewModel Create(BLL.Notification notification, string tabId, int parentId, INotificationService service)
        {
            var model = EntityViewModel.Create<NotificationViewModel>(notification, tabId, parentId);
			model.CreateDefaultFormat = model.IsNew;
			model._contentId = parentId;
			model._service = service;
			model.ShowUnbindButton = notification.CanBeUnbound;
            return model;
        }

        internal void DoCustomBinding()
        {
			if (Data.IsExternal)
				CreateDefaultFormat = false;			
			Data.DoCustomBinding(CreateDefaultFormat);			
        }

		public override void Validate(ModelStateDictionary modelState)
		{
			base.Validate(modelState);
			if (!CreateDefaultFormat && !Data.FormatId.HasValue && !Data.IsExternal)
				modelState.AddModelError("Data.FormatId", NotificationStrings.FormatIdNotEntered);
		}

		public bool ShowUnbindButton { get; private set; }
    }
}