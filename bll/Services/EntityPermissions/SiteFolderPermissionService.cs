using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public class SiteFolderPermissionService : PermissionServiceAbstract
    {
        private readonly Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new SiteFolderPermissionRepository());
        public override IPermissionRepository Repository => repository.Value;

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.SiteFolderPermissions,
            AddNewItemActionCode = ActionCode.AddNewSiteFolderPermission,
            EntityTypeCode = EntityTypeCode.SiteFolderPermission,
            IsPropagateable = false,
            PermissionEntityTypeCode = EntityTypeCode.SiteFolderPermission
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            ActionCode = ActionCode.SiteFolderPermissionProperties,
            EntityTypeCode = EntityTypeCode.SiteFolderPermission,
            IsPropagateable = false
        };
    }
}
