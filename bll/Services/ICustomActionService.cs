using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public interface ICustomActionService
    {
        CustomActionPrepareResult PrepareForExecuting(string tabId, int parentId, CustomActionQuery query);

        ListResult<CustomActionListItem> List(ListCommand listCommand);

        CustomAction Read(int id);

        CustomAction ReadForUpdate(int id);

        CustomAction Update(CustomAction customAction, int[] selectedActionsIds);

        CustomAction New();

        CustomAction NewForSave();

        CustomAction Save(CustomAction customAction, int[] selectedActionsIds);

        MessageResult Remove(int id);

        IEnumerable<ListItem> GetActionTypeList();

        IEnumerable<ListItem> GetEntityTypeList();

        IEnumerable<Site> GetSites(CustomAction action);

        IEnumerable<Content> GetContents(CustomAction action);

        CustomActionInitListResult InitList(int parentId);

        CopyResult Copy(int id, int[] selectedActionsIds);
    }
}
