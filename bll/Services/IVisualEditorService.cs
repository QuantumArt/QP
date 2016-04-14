using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.VisualEditor;

namespace Quantumart.QP8.BLL.Services
{
    public interface IVisualEditorService
    {
        VisualEditorInitListResult InitList(int contentId);

        ListResult<VisualEditorPluginListItem> GetVisualEditorPlugins(ListCommand cmd, int contentId);

        VisualEditorPlugin ReadVisualEditorPluginProperties(int id);

        VisualEditorPlugin ReadVisualEditorPluginPropertiesForUpdate(int id);

        VisualEditorPlugin UpdateVisualEditorProperties(VisualEditorPlugin visualEditorPlugin);

        MessageResult Remove(int id);

        VisualEditorPlugin NewVisualEditorPluginProperties(int parentId);

        VisualEditorPlugin NewVisualEditorPluginPropertiesForUpdate(int parentId);

        VisualEditorPlugin SaveVisualEditorPluginProperties(VisualEditorPlugin visualEditorPlugin);

        VisualEditorStyleInitListResult InitVisualEditorStyleList(int parentId);

        ListResult<VisualEditorStyleListItem> GetVisualEditorStyles(ListCommand cmd, int contentId);

        VisualEditorStyle ReadVisualEditorStyleProperties(int id);

        VisualEditorStyle ReadVisualEditorStylePropertiesForUpdate(int id);

        VisualEditorStyle UpdateVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle);

        MessageResult RemoveVisualEditorStyle(int id);

        VisualEditorStyle NewVisualEditorStyleProperties(int parentId);

        VisualEditorStyle NewVisualEditorStylePropertiesForUpdate(int parentId);

        VisualEditorStyle SaveVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle);
    }
}
