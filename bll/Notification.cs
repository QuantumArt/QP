using System.Text.RegularExpressions;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

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

        [LocalizedDisplayName("Format", NameResourceType = typeof(NotificationStrings))]
        public int? FormatId { get; set; }

        [LocalizedDisplayName("User", NameResourceType = typeof(NotificationStrings))]
        public int? UserId { get; set; }

        [LocalizedDisplayName("UserGroup", NameResourceType = typeof(NotificationStrings))]
        public int? GroupId { get; set; }

        [LocalizedDisplayName("QP8User", NameResourceType = typeof(NotificationStrings))]
        public int? FromBackenduserId { get; set; }

        public User FromUser { get; set; }

        [LocalizedDisplayName("Field", NameResourceType = typeof(NotificationStrings))]
        public int? EmailFieldId { get; set; }

        public int? WorkFlowId { get; set; }

        public Workflow Workflow { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof(NotificationStrings))]
        public int? NotifyOnStatusTypeId { get; set; }

        [LocalizedDisplayName("ForCreate", NameResourceType = typeof(NotificationStrings))]
        public bool ForCreate { get; set; }

        [LocalizedDisplayName("ForModify", NameResourceType = typeof(NotificationStrings))]
        public bool ForModify { get; set; }

        [LocalizedDisplayName("ForRemove", NameResourceType = typeof(NotificationStrings))]
        public bool ForRemove { get; set; }

        [LocalizedDisplayName("ForStatusChanged", NameResourceType = typeof(NotificationStrings))]
        public bool ForStatusChanged { get; set; }

        [LocalizedDisplayName("ForStatusPartiallyChanged", NameResourceType = typeof(NotificationStrings))]
        public bool ForStatusPartiallyChanged { get; set; }

        [LocalizedDisplayName("ForFrontend", NameResourceType = typeof(NotificationStrings))]
        public bool ForFrontend { get; set; }

        [LocalizedDisplayName("ForDelayedPublication", NameResourceType = typeof(NotificationStrings))]
        public bool ForDelayedPublication { get; set; }

        [LocalizedDisplayName("SenderName", NameResourceType = typeof(NotificationStrings))]
        public string FromUserName { get; set; }

        [LocalizedDisplayName("UseDefaultSenderName", NameResourceType = typeof(NotificationStrings))]
        public bool FromDefaultName { get; set; }

        [LocalizedDisplayName("DefaultSenderName", NameResourceType = typeof(NotificationStrings))]
        public string DefaultSenderName { get; set; }

        [LocalizedDisplayName("UseQaMail", NameResourceType = typeof(NotificationStrings))]
        public bool UseQaMail { get; set; }

        [LocalizedDisplayName("Email", NameResourceType = typeof(NotificationStrings))]
        public string FromUserEmail { get; set; }

        [LocalizedDisplayName("SendFiles", NameResourceType = typeof(NotificationStrings))]
        public bool SendFiles { get; set; }

        [LocalizedDisplayName("UseQP8UserEmail", NameResourceType = typeof(NotificationStrings))]
        public bool FromBackenduser { get; set; }

        public User ToUser { get; set; }

        public UserGroup ToUserGroup { get; set; }

        public bool NoEmail { get; set; }

        [LocalizedDisplayName("External", NameResourceType = typeof(NotificationStrings))]
        public bool IsExternal { get; set; }

        [LocalizedDisplayName("ExternalUrl", NameResourceType = typeof(NotificationStrings))]
        public string ExternalUrl { get; set; }

        [LocalizedDisplayName("UseService", NameResourceType = typeof(NotificationStrings))]
        public bool UseService { get; set; }

        [LocalizedDisplayName("ReceiverType", NameResourceType = typeof(NotificationStrings))]
        public int SelectedReceiverType { get; set; }

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

        public void DoCustomBinding(bool createDefaultFormat)
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

            if (createDefaultFormat)
            {
                FormatId = null;
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
