using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class ActionPermissionService : PermissionServiceAbstract
	{
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new ActionPermissionRepository());
		public override IPermissionRepository Repository { get { return repository.Value; } }

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.ActionPermissions,
					AddNewItemActionCode = ActionCode.AddNewActionPermission,
					EntityTypeCode = EntityTypeCode.ActionPermission,
					IsPropagateable = false,
					CanHide = false,
					PermissionEntityTypeCode = EntityTypeCode.ActionPermission
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.ActionPermissionProperties,
					EntityTypeCode = EntityTypeCode.ActionPermission,
					IsPropagateable = false,
					CanHide = false
				};
			}
		}
	}
}
