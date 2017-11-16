using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Notification
{
    public class NotificationListViewModel : ListViewModel
    {
        public IEnumerable<NotificationListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static NotificationListViewModel Create(NotificationInitListResult result, string tabId, int parentId)
        {
            var model = Create<NotificationListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Notification;

        public override string ActionCode => Constants.ActionCode.Notifications;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewNotification;

        public override string AddNewItemText => NotificationStrings.AddNewNotification;
    }
}
