using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public interface IObjectService
    {
        ObjectInitListResult InitObjectList(int parentId, bool isTemplateObject);

        ListResult<ObjectListItem> GetTemplateObjectsByTemplateId(ListCommand listCommand, int parentId);

        IEnumerable<StatusType> GetActiveStatusesByObjectId(int objectId);

        ListResult<ObjectListItem> GetPageObjectsByPageId(ListCommand listCommand, int parentId);

        BllObject NewObjectProperties(int parentId, bool pageOrTemplate);

        BllObject NewObjectPropertiesForUpdate(int parentId, bool pageOrTemplate);

        BllObject SaveObjectProperties(BllObject bllObject, IEnumerable<int> activeStatuses, bool isReplayAction);

        BllObject UpdateObjectProperties(BllObject bllObject, IEnumerable<int> activeStatuses);

        BllObject ReadObjectProperties(int id, bool withAutoLock = true);

        BllObject ReadObjectPropertiesForUpdate(int id);

        MessageResult PromotePageObject(int id);

        MessageResult RemoveObject(int id);

        Page ReadPageProperties(int id, bool withAutoLock = true);

        MessageResult MultipleRemovePageObject(int[] IDs);

        MessageResult MultipleRemoveTemplateObject(int[] IDs);

        void CancelObject(int id);

        MessageResult MultipleAssembleObjectPreAction(int[] Ids);

        MessageResult AssembleObjectPreAction(int id);

        MessageResult AssembleObject(int id);

        MessageResult MultipleAssembleObject(int[] ids);

        IEnumerable<ListItem> GetTypes();

        int GetPublishedStatusIdBySiteId(int p);

        IEnumerable<ListItem> GetPermissionLevels();

        Content GetContentById(int contentId);

        IEnumerable<ListItem> GetNetLanguagesAsListItems();

        IEnumerable<BllObject> GetFreeTemplateObjectsByPageId(int pageId);

        void CaptureLockPageObject(int id);

        void CaptureLockTemplateObject(int id);
    }
}
