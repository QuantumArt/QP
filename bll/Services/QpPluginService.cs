using System;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class QpPluginService : IQpPluginService
    {
        public InitListResult InitList(int parentId) =>
            new InitListResult
            {
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewQpPlugin) &&
                    SecurityRepository.IsEntityAccessible(EntityTypeCode.QpPlugin, parentId, ActionTypeCode.Update)
            };

        public ListResult<QpPluginListItem> List(ListCommand cmd, int parentId)
        {
            var list = QpPluginRepository.List(cmd, parentId, out var totalRecords);
            return new ListResult<QpPluginListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public QpPlugin Read(int id)
        {
            var plugin = QpPluginRepository.GetById(id);
            if (plugin == null)
            {
                throw new ApplicationException(string.Format(QpPluginStrings.PluginNotFound, id));
            }
            return plugin;
        }

        public QpPlugin ReadForUpdate(int id) => Read(id);

        public QpPlugin Update(QpPlugin plugin)
        {
            if (plugin.OldContract != plugin.Contract)
            {
                QpPluginRepository.CreateVersion(plugin);
            }
            return QpPluginRepository.UpdateProperties(plugin);
        }

        public MessageResult Remove(int id)
        {
            Read(id);
            QpPluginRepository.Delete(id);
            return null;
        }

        public QpPlugin New(int parentId) => QpPlugin.Create();

        public QpPlugin NewForSave(int parentId) => New(parentId);

        public QpPlugin Save(QpPlugin plugin) => QpPluginRepository.SaveProperties(plugin);
    }
}
