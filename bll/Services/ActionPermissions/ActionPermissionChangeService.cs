using System;
using Quantumart.QP8.BLL.Repository.ActionPermissions;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
	public class ActionPermissionChangeService : ActionPermissionChangeServiceAbstract
	{
		private Lazy<IPermissionRepository> permissionRepository = new Lazy<IPermissionRepository>(() => new ActionPermissionRepository());
		public override IPermissionRepository PermissionRepository => permissionRepository.Value;

	    private Lazy<IActionPermissionChangeRepository> changeRepository = new Lazy<IActionPermissionChangeRepository>(() => new ActionPermissionChangeRepository());
		public override IActionPermissionChangeRepository ChangeRepository => changeRepository.Value;

	    public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
		{
		    ActionCode = ActionCode.ChangeActionPermission,
		    EntityTypeCode = EntityTypeCode.EntityTypePermission,
		    IsPropagateable = false
		};
	}
}
