using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class WorkflowPermissionService : PermissionServiceAbstract
	{
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new WorkflowPermissionRepository());
		public override IPermissionRepository Repository => repository.Value;

	    public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
	    {
	        ActionCode = ActionCode.WorkflowPermissions,
	        AddNewItemActionCode = ActionCode.AddNewWorkflowPermission,
	        EntityTypeCode = EntityTypeCode.WorkflowPermission,
	        IsPropagateable = false,
	        PermissionEntityTypeCode = EntityTypeCode.WorkflowPermission
	    };

	    public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
		{
		    ActionCode = ActionCode.WorkflowPermissionProperties,
		    EntityTypeCode = EntityTypeCode.WorkflowPermission,
		    IsPropagateable = false
		};
	}
}
