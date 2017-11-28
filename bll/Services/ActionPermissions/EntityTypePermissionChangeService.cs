using System;
using Quantumart.QP8.BLL.Repository.ActionPermissions;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
    public class EntityTypePermissionChangeService : ActionPermissionChangeServiceAbstract
    {
        private readonly Lazy<IPermissionRepository> permissionRepository = new Lazy<IPermissionRepository>(() => new EntityTypePermissionRepository());
        public override IPermissionRepository PermissionRepository => permissionRepository.Value;

        private readonly Lazy<IActionPermissionChangeRepository> changeRepository = new Lazy<IActionPermissionChangeRepository>(() => new EntityTypePermissionChangeRepository());
        public override IActionPermissionChangeRepository ChangeRepository => changeRepository.Value;

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            ActionCode = ActionCode.ChangeEntityTypePermission,
            EntityTypeCode = EntityTypeCode.EntityTypePermission,
            IsPropagateable = false
        };
    }
}
