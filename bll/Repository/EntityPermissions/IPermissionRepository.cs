using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    public interface IPermissionRepository
    {
        IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords);

        EntityPermission GetById(int id, bool include = true);

        EntityPermission Save(EntityPermission permission);

        bool CheckUnique(EntityPermission permission);

        EntityPermission Update(EntityPermission permission);

        void MultipleRemove(IEnumerable<int> IDs);

        void Remove(int id);
    }
}
