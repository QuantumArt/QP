using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services;

public interface INotificationService
{
    ListResult<NotificationListItem> GetNotificationsByContentId(ListCommand cmd, int parentId);

    Notification NewNotificationProperties(int contentId);

    Notification ReadNotificationProperties(int id);

    Notification SaveNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl);

    Notification UpdateNotificationProperties(Notification notification, bool createDefaultFormat, string backendUrl);

    IEnumerable<ListItem> GetStatusesAsListItemsBySiteId(int siteId);

    IEnumerable<ListItem> GetObjectFormatsAsListItemsByContentId(int contentId);

    IEnumerable<ListItem> GetStringFieldsAsListItemsByContentId(int contentId);

    IEnumerable<ListItem> GetRelationFieldsAsListItemsByContentId(int contentId);

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
