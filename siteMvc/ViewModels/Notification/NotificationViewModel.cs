using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
    public class NotificationViewModel : EntityViewModel
    {
        private int _contentId;
        private INotificationService _service;

        public override string EntityTypeCode => Constants.EntityTypeCode.Notification;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewNotification : Constants.ActionCode.NotificationProperties;

        public BLL.Notification Data
        {
            get => (BLL.Notification)EntityData;
            set => EntityData = value;
        }

        private List<ListItem> _formats;

        public List<ListItem> Formats
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
            set => _formats = value;
        }

        private List<ListItem> _statuses;

        public List<ListItem> Statuses
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

        private List<ListItem> _templates;

        public List<ListItem> Templates
        {
            get
            {
                if (_templates == null)
                {
                    _templates = _service.GetTemplates().ToList();
                    _templates.Insert(0, new ListItem { Text = NotificationStrings.ChooseField, Value = string.Empty});
                }

                return _templates;
            }

        }

        private List<ListItem> _fields;

        public List<ListItem> Fields
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

        [Display(Name = "CreateDefaultFormat", ResourceType = typeof(NotificationStrings))]
        public bool CreateDefaultFormat { get; set; }

        public QPSelectListItem FromUserListItem
        {
            get
            {
                if (Data.FromBackenduserId == null)
                {
                    return null;
                }

                return Data.FromUser != null ? new QPSelectListItem { Value = Data.FromUser.Id.ToString(), Text = Data.FromUser.LogOn, Selected = true } : new QPSelectListItem { Value = SpecialIds.AdminUserId.ToString(), Text = @"admin", Selected = true };
            }
        }

        public QPSelectListItem ToUserListItem => Data.ToUser != null ? new QPSelectListItem { Value = Data.ToUser.Id.ToString(), Text = Data.ToUser.LogOn, Selected = true } : null;

        public QPSelectListItem ToUserGroupListItem => Data.ToUserGroup != null ? new QPSelectListItem { Value = Data.ToUserGroup.Id.ToString(), Text = Data.ToUserGroup.Name, Selected = true } : null;

        public IEnumerable<ListItem> GetReceiverTypes() => new[]
        {
            new ListItem(ReceiverType.User.ToString(), BLL.Content.GetReceiverTypeString(ReceiverType.User), "UserPanel"),
            new ListItem(ReceiverType.UserGroup.ToString(), BLL.Content.GetReceiverTypeString(ReceiverType.UserGroup), "UserGroupPanel"),
            new ListItem(ReceiverType.EveryoneInHistory.ToString(), BLL.Content.GetReceiverTypeString(ReceiverType.EveryoneInHistory), "EmptyPanel"),
            new ListItem(ReceiverType.EmailFromArticle.ToString(), BLL.Content.GetReceiverTypeString(ReceiverType.EmailFromArticle), "FieldPanel"),
            new ListItem(ReceiverType.None.ToString(), BLL.Content.GetReceiverTypeString(ReceiverType.None), "EmptyPanel")
        };

        public SelectOptions SelectFormatOptions
        {
            get
            {
                var options = new SelectOptions
                {
                    EntityDataListArgs = new EntityDataListArgs
                    {
                        EntityTypeCode = Constants.EntityTypeCode.TemplateObjectFormat,
                        ParentEntityId = Data.ContentId,
                        EntityId = Data.Id,
                        ListId = Data.ContentId,
                        ReadActionCode = Constants.ActionCode.NotificationObjectFormatProperties
                    }
                };

                return options;
            }
        }

        public static NotificationViewModel Create(BLL.Notification notification, string tabId, int parentId, INotificationService service)
        {
            var model = Create<NotificationViewModel>(notification, tabId, parentId);
            model.CreateDefaultFormat = false;
            model._contentId = parentId;
            model._service = service;
            model.ShowUnbindButton = notification.CanBeUnbound;
            return model;
        }

        public override void DoCustomBinding()
        {
            base.DoCustomBinding();

            CreateDefaultFormat = false;
            Data.FormatId = null;

            if (Data.IsExternal)
            {
                Data.TemplateId = null;
            }
        }

        public override IEnumerable<ValidationResult> ValidateViewModel()
        {
            if (!Data.IsExternal && !Data.TemplateId.HasValue)
            {
                yield return new ValidationResult(NotificationStrings.TemplateIdNotEntered, new[] { "Data.TemplateId" });
            }
        }

        public bool ShowUnbindButton { get; private set; }
    }
}
