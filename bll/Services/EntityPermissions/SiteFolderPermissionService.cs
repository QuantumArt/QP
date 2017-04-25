using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class SiteFolderPermissionService : PermissionServiceAbstract
	{
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new SiteFolderPermissionRepository());
		public override IPermissionRepository Repository { get { return repository.Value; } }

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get 
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.SiteFolderPermissions,
					AddNewItemActionCode = ActionCode.AddNewSiteFolderPermission,
					EntityTypeCode = EntityTypeCode.SiteFolderPermission,
					IsPropagateable = false,
					PermissionEntityTypeCode = EntityTypeCode.SiteFolderPermission
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get 
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.SiteFolderPermissionProperties,
					EntityTypeCode = EntityTypeCode.SiteFolderPermission,
					IsPropagateable = false
				};
			}
		}
	}
}
