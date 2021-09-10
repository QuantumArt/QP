using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.VisualEditor;

namespace Quantumart.QP8.BLL.Services
{
    public interface IQpPluginService
    {
        InitListResult InitList(int parentId);

        ListResult<QpPluginListItem> List(ListCommand cmd, int parentId);

        QpPlugin Read(int id);

        QpPlugin ReadForUpdate(int id);

        QpPlugin Update(QpPlugin plugin);

        MessageResult Remove(int id);

        QpPlugin New(int parentId);

        QpPlugin NewForSave(int parentId);

        QpPlugin Save(QpPlugin visualEditorPlugin);
    }
}
