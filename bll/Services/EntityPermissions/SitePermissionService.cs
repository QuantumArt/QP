using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class SitePermissionService : PermissionServiceAbstract
	{
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new SitePermissionRepository());
		public override IPermissionRepository Repository => repository.Value;

	    public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
	    {
	        ActionCode = ActionCode.SitePermissions,
	        AddNewItemActionCode = ActionCode.AddNewSitePermission,
	        EntityTypeCode = EntityTypeCode.SitePermission,
	        IsPropagateable = true,
	        PermissionEntityTypeCode = EntityTypeCode.SitePermission
	    };

	    public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
		{
		    ActionCode = ActionCode.SitePermissionProperties,
		    EntityTypeCode = EntityTypeCode.SitePermission,
		    IsPropagateable = true
		};
	}
}
