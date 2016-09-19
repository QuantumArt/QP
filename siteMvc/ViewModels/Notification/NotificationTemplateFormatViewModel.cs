using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
    public class NotificationTemplateFormatViewModel : EntityViewModel
    {
        public new NotificationObjectFormat Data
        {
            get
            {
                return (NotificationObjectFormat)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public static NotificationTemplateFormatViewModel Create(NotificationObjectFormat format, string tabId, int parentId, int templateId, int siteId)
        {
            var model = Create<NotificationTemplateFormatViewModel>(format, tabId, parentId);
            model.TemplateId = templateId;
            model.SiteId = siteId;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.TemplateObjectFormat;

        public override string ActionCode => Constants.ActionCode.NotificationObjectFormatProperties;

        public int TemplateId { get; set; }

        public int SiteId { get; set; }
    }
}
