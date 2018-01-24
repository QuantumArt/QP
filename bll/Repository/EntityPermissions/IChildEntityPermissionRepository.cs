using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    public interface IChildEntityPermissionRepository
    {
        IEnumerable<ChildEntityPermissionListItem> List(int siteId, int? groupId, int? userId, ListCommand cmd, out int totalRecords);

        ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId);

        void MultipleChange(int parentId, List<int> entityIds, ChildEntityPermission permissionSettings);

        void ChangeAll(int parentId, ChildEntityPermission permissionSettings);

        void MultipleRemove(int parentId, IEnumerable<int> entityIds, int? userId, int? groupId);

        void RemoveAll(int parentId, int? userId, int? groupId);

        EntityPermission GetParentPermission(int parentId, int? userId, int? groupId);
    }
}
