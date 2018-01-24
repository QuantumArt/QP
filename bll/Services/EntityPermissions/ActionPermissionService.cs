using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public class ActionPermissionService : PermissionServiceAbstract
    {
        private readonly Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new ActionPermissionRepository());
        public override IPermissionRepository Repository => repository.Value;

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.ActionPermissions,
            AddNewItemActionCode = ActionCode.AddNewActionPermission,
            EntityTypeCode = EntityTypeCode.ActionPermission,
            IsPropagateable = false,
            CanHide = false,
            PermissionEntityTypeCode = EntityTypeCode.ActionPermission
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            ActionCode = ActionCode.ActionPermissionProperties,
            EntityTypeCode = EntityTypeCode.ActionPermission,
            IsPropagateable = false,
            CanHide = false
        };
    }
}
