using System;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class EntityTypePermissionService : PermissionServiceAbstract
	{
		#region PermissionServiceAbstract Members
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new EntityTypePermissionRepository());
		public override IPermissionRepository Repository { get { return repository.Value; } }

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.EntityTypePermissions,
					AddNewItemActionCode = ActionCode.AddNewEntityTypePermission,
					EntityTypeCode = EntityTypeCode.EntityTypePermission,
					IsPropagateable = false,
					PermissionEntityTypeCode = EntityTypeCode.EntityTypePermission
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.EntityTypePermissionProperties,
					EntityTypeCode = EntityTypeCode.EntityTypePermission,
					IsPropagateable = false
				};
			}
		} 
		#endregion		
	}
}
