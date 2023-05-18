using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class Notification : EntityObject
    {
        internal Notification()
        {
            FromBackenduserId = SpecialIds.AdminUserId;
            FromDefaultName = true;
            FromBackenduser = true;
            DefaultSenderName = QPConfiguration.ConfigVariable(Config.MailFromNameKey);
            SelectedReceiverType = ReceiverType.EveryoneInHistory;
        }

        internal static Notification Create(int contentId)
        {
            var notification = new Notification { ContentId = contentId };
            notification.UseQaMail = !notification.Content.Site.IsDotNet;
            notification.ExternalUrl = notification.Content.Site.ExternalUrl;
            return notification;
        }

        public int ContentId { get; set; }

        private Content _content;

        public Content Content
        {
            get => _content ?? (_content = ContentRepository.GetById(ContentId));
            set => _content = value;
        }

        [Display(Name = "Format", ResourceType = typeof(NotificationStrings))]
        public int? FormatId { get; set; }

        [Display(Name = "User", ResourceType = typeof(NotificationStrings))]
        public int? UserId { get; set; }

        [Display(Name = "UserGroup", ResourceType = typeof(NotificationStrings))]
        public int? GroupId { get; set; }

        [Display(Name = "QP8User", ResourceType = typeof(NotificationStrings))]
        public int? FromBackenduserId { get; set; }

        public User FromUser { get; set; }

        [Display(Name = "Field", ResourceType = typeof(NotificationStrings))]
        public int? EmailFieldId { get; set; }

        public int? WorkFlowId { get; set; }

        public Workflow Workflow { get; set; }

        [Display(Name = "Status", ResourceType = typeof(NotificationStrings))]
        public int? NotifyOnStatusTypeId { get; set; }

        [Display(Name = "ForCreate", ResourceType = typeof(NotificationStrings))]
        public bool ForCreate { get; set; }

        [Display(Name = "ForModify", ResourceType = typeof(NotificationStrings))]
        public bool ForModify { get; set; }

        [Display(Name = "ForRemove", ResourceType = typeof(NotificationStrings))]
        public bool ForRemove { get; set; }

        [Display(Name = "ForStatusChanged", ResourceType = typeof(NotificationStrings))]
        public bool ForStatusChanged { get; set; }

        [Display(Name = "ForStatusPartiallyChanged", ResourceType = typeof(NotificationStrings))]
        public bool ForStatusPartiallyChanged { get; set; }

        [Display(Name = "ForFrontend", ResourceType = typeof(NotificationStrings))]
        public bool ForFrontend { get; set; }

        [Display(Name = "ForDelayedPublication", ResourceType = typeof(NotificationStrings))]
        public bool ForDelayedPublication { get; set; }

        [Display(Name = "SenderName", ResourceType = typeof(NotificationStrings))]
        public string FromUserName { get; set; }

        [Display(Name = "UseDefaultSenderName", ResourceType = typeof(NotificationStrings))]
        public bool FromDefaultName { get; set; }

        [Display(Name = "DefaultSenderName", ResourceType = typeof(NotificationStrings))]
        public string DefaultSenderName { get; set; }

        [Display(Name = "UseQaMail", ResourceType = typeof(NotificationStrings))]
        public bool UseQaMail { get; set; }

        [Display(Name = "Email", ResourceType = typeof(NotificationStrings))]
        public string FromUserEmail { get; set; }

        [Display(Name = "SendFiles", ResourceType = typeof(NotificationStrings))]
        public bool SendFiles { get; set; }

        [Display(Name = "UseQP8UserEmail", ResourceType = typeof(NotificationStrings))]
        public bool FromBackenduser { get; set; }

        [Display(Name = "Template", ResourceType = typeof(NotificationStrings))]
        public int? TemplateId { get; set; }

        public User ToUser { get; set; }

        public UserGroup ToUserGroup { get; set; }

        public bool NoEmail { get; set; }

        [Display(Name = "External", ResourceType = typeof(NotificationStrings))]
        public bool IsExternal { get; set; }

        [Display(Name = "ExternalUrl", ResourceType = typeof(NotificationStrings))]
        public string ExternalUrl { get; set; }

        [Display(Name = "UseService", ResourceType = typeof(NotificationStrings))]
        public bool UseService { get; set; }

        [Display(Name = "ReceiverType", ResourceType = typeof(NotificationStrings))]
        public int SelectedReceiverType { get; set; }

        [Display(Name = "HideRecipients", ResourceType = typeof(NotificationStrings))]
        public bool HideRecipients { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.Notification;

        public override int ParentEntityId => ContentId;

        public bool IsLegacy => !IsExternal;

        public override void Validate()
        {
            var errors = new RulesException<Notification>();
            base.Validate(errors);

            if (Content.HasAggregatedFields)
            {
                errors.ErrorForModel(NotificationStrings.ContentContainsAggregatedFields);
            }

            if (IsExternal)
            {
                if (string.IsNullOrWhiteSpace(ExternalUrl))
                {
                    errors.ErrorFor(n => n.ExternalUrl, NotificationStrings.ExternalUrlNotEntered);
                }
                else if (!UrlHelpers.IsAbsoluteWebFolderUrl(ExternalUrl))
                {
                    errors.ErrorFor(n => n.ExternalUrl, NotificationStrings.ExternalUrlNotValid);
                }
            }

            if (!ForCreate && !ForModify && !ForRemove && !ForStatusChanged && !ForStatusPartiallyChanged && !ForFrontend && !ForDelayedPublication)
            {
                errors.ErrorForModel(NotificationStrings.EventNotSelected);
            }

            if (!FromDefaultName)
            {
                if (string.IsNullOrWhiteSpace(FromUserName))
                {
                    errors.ErrorFor(n => n.FromUserName, NotificationStrings.SenderNameNotEntered);
                }
                else if (FromUserName.Length > 255)
                {
                    errors.ErrorFor(n => n.FromUserName, string.Format(NotificationStrings.SenderNameMaxLengthExceeded, 255));
                }
            }

            if (!FromBackenduser)
            {
                if (string.IsNullOrWhiteSpace(FromUserEmail))
                {
                    errors.ErrorFor(n => n.FromUserEmail, NotificationStrings.SenderEmailNotEntered);
                }
                else if (FromUserEmail.Length > 255)
                {
                    errors.ErrorFor(n => n.FromUserEmail, string.Format(NotificationStrings.SenderEmailLengthExceeded, 255));
                }
                else if (!Regex.IsMatch(FromUserEmail, RegularExpressions.Email))
                {
                    errors.ErrorFor(n => n.FromUserEmail, NotificationStrings.SenderEmailNotValid);
                }
            }

            else
            {
                if (!FromBackenduserId.HasValue && !IsExternal)
                {
                    errors.ErrorFor(n => n.FromBackenduserId, NotificationStrings.UserNotSelected);
                }
            }

            switch (SelectedReceiverType)
            {
                case ReceiverType.User:
                    if (!UserId.HasValue)
                    {
                        errors.ErrorFor(n => n.UserId, NotificationStrings.UserNotSelected);
                    }
                    break;
                case ReceiverType.UserGroup:
                    if (!GroupId.HasValue)
                    {
                        errors.ErrorFor(n => n.GroupId, NotificationStrings.UserGroupNotSelected);
                    }
                    break;
                case ReceiverType.EmailFromArticle:
                    if (!EmailFieldId.HasValue)
                    {
                        errors.ErrorFor(n => n.EmailFieldId, NotificationStrings.FieldNotSelected);
                    }
                    break;
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        public override void DoCustomBinding()
        {
            if (IsExternal)
            {
                FromDefaultName = true;
                FromBackenduser = true;
                SelectedReceiverType = ReceiverType.EveryoneInHistory;
                SendFiles = false;
                UseQaMail = false;
                FromUserName = null;
                FromUserEmail = null;
                FormatId = null;
            }
            else
            {
                ExternalUrl = null;
                UseService = false;
            }

            if (!ForStatusChanged && !ForStatusPartiallyChanged)
            {
                NotifyOnStatusTypeId = null;
            }

            if (FromDefaultName)
            {
                FromUserName = null;
            }

            if (FromBackenduser)
            {
                FromUserEmail = null;
            }

            switch (SelectedReceiverType)
            {
                case ReceiverType.User:
                    GroupId = null;
                    EmailFieldId = null;
                    NoEmail = false;
                    break;
                case ReceiverType.UserGroup:
                    UserId = null;
                    EmailFieldId = null;
                    NoEmail = false;
                    break;
                case ReceiverType.EmailFromArticle:
                    UserId = null;
                    GroupId = null;
                    NoEmail = false;
                    break;
                case ReceiverType.EveryoneInHistory:
                    UserId = null;
                    GroupId = null;
                    EmailFieldId = null;
                    NoEmail = false;
                    break;
                case ReceiverType.None:
                    UserId = null;
                    GroupId = null;
                    EmailFieldId = null;
                    NoEmail = true;
                    break;
            }
        }

        public int ComputeReceiverType()
        {
            if (UserId.HasValue)
            {
                return ReceiverType.User;
            }

            if (GroupId.HasValue)
            {
                return ReceiverType.UserGroup;
            }

            if (EmailFieldId.HasValue)
            {
                return ReceiverType.EmailFromArticle;
            }

            return NoEmail ? ReceiverType.None : ReceiverType.EveryoneInHistory;
        }

        public bool CanBeUnbound => !IsNew && SecurityRepository.IsActionAccessible(ActionCode.UnbindNotification);
    }
}
