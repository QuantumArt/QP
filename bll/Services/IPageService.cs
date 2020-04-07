using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public interface IPageService
    {
        PageInitListResult InitPageList(int parentId);

        PageInitListResult InitPageListForSite(int siteId);

        ListResult<PageListItem> GetPagesByTemplateId(ListCommand listCommand, int parentId);

        IEnumerable<ListItem> GetLocalesAsListItems();

        IEnumerable<ListItem> GetCharsetsAsListItems();

        Page ReadPagePropertiesForUpdate(int id);

        Page UpdatePageProperties(Page page);

        Page NewPageProperties(int parentId);

        Page NewPagePropertiesForUpdate(int parentId);

        Page SavePageProperties(Page page);

        MessageResult RemovePage(int id);

        void CancelPage(int id);

        MessageResult MultipleRemovePage(int[] ids);

        PageTemplate ReadPageTemplateProperties(int id, bool withAutoLock = true);

        Page ReadPageProperties(int id, bool withAutoLock = true);

        ListResult<PageListItem> ListPagesForSite(ListCommand listCommand, int parentId, int id);

        MessageResult AssemblePagePreAction(int id);

        MessageResult MultipleAssemblePagePreAction(int[] ids);

        MessageResult MultipleAssemblePage(int[] ids);

        MessageResult AssemblePage(int id);

        void CaptureLockPage(int id);

        CopyResult Copy(int id);
    }
}
