using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
	public class NotificationTemplateFormatViewModel : EntityViewModel
	{						
		private int _contentId;

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

		#region creation

		public static NotificationTemplateFormatViewModel Create(NotificationObjectFormat format, string tabId, int parentId, int templateId, int siteId)
		{
			var model = EntityViewModel.Create<NotificationTemplateFormatViewModel>(format, tabId, parentId);
			model._contentId = parentId;
			model.TemplateId = templateId;
			model.SiteId = siteId;
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.TemplateObjectFormat;
			}
		}

		public override string ActionCode
		{
			get
			{				
				return C.ActionCode.NotificationObjectFormatProperties;
			}
		}

		public int TemplateId { get; set; }

		public int SiteId { get; set; }
	}
}