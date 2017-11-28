using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
    public interface IActionPermissionChangeService
    {
        EntityPermission ReadOrDefault(int parentId, int? userId, int? groupId);
        EntityPermission ReadOrDefaultForChange(int parentId, int? userId, int? groupId);
        IPermissionViewModelSettings ViewModelSettings { get; }
        EntityPermission Change(EntityPermission entityPermission);
        MessageResult Remove(int parentId, int? userId, int? groupId);
    }
}
