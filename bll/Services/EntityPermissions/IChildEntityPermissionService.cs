using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    /// <summary>
    /// Интерфейс сервиса для правил доступа к дочерним элементам
    /// </summary>
    public interface IChildEntityPermissionService
    {
        /// <summary>
        /// Стратегии, определяющая поведение View-моделей
        /// </summary>
        IPermissionListViewModelSettings ListViewModelSettings { get; }

        IPermissionViewModelSettings ViewModelSettings { get; }

        ChildPermissionInitListResult InitList(int parentId);

        ListResult<ChildEntityPermissionListItem> List(int parentId, int? groupId, int? userId, ListCommand cmd);

        ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId);

        void MultipleChange(int parentId, List<int> entityIds, ChildEntityPermission permissionSettings);

        void Change(int parentId, int entityId, ChildEntityPermission childEntityPermission);

        void ChangeAll(int parentId, ChildEntityPermission childEntityPermission);

        MessageResult MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId);

        MessageResult Remove(int parentId, int entityId, int? userId, int? groupId);

        MessageResult RemoveAll(int parentId, int? userId, int? groupId);

        IEnumerable<EntityPermissionLevel> GetPermissionLevels();
    }
}
