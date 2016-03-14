using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
    public class NotificationListViewModel : ListViewModel
    {
        public IEnumerable<NotificationListItem> Data { get; set; }

        public string GettingDataActionName
        {
            get
            {
                return "_Index";
            }
        }

        public static NotificationListViewModel Create(NotificationInitListResult result, string tabId, int parentId)
        {
            var model = ViewModel.Create<NotificationListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
        }

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Notification; }
        }

        public override string ActionCode
        {
            get { return C.ActionCode.Notifications; }
        }

        public override string AddNewItemActionCode
        {
            get
            {
                return C.ActionCode.AddNewNotification;
            }
        }

        public override string AddNewItemText
        {
            get
            {
                return NotificationStrings.AddNewNotification;
            }
        }
    }
}