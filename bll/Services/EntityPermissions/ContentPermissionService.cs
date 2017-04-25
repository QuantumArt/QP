using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class ContentPermissionService : PermissionServiceAbstract
	{
		private Lazy<IContentPermissionRepository> repository = new Lazy<IContentPermissionRepository>(() => new ContentPermissionRepository());
		public override IPermissionRepository Repository { get { return repository.Value; } }
		private IContentPermissionRepository ContentPermissionRepository { get { return repository.Value; } }

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get 
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.ContentPermissions,
					AddNewItemActionCode = ActionCode.AddNewContentPermission,
					EntityTypeCode = EntityTypeCode.ContentPermission,
					IsPropagateable = true,
					CanHide = true,
					PermissionEntityTypeCode = EntityTypeCode.ContentPermission
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get 
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.ContentPermissionProperties,
					EntityTypeCode = EntityTypeCode.ContentPermission,
					IsPropagateable = true,
					CanHide = true
				};
			}
		}

		public override EntityPermission Save(EntityPermission permission)
		{
			if (permission.ExplicitPermissionToRelatedContents)
				ExplicitPermissionToRelatedContents(permission);			

			return base.Save(permission);
		}

		public override EntityPermission Update(EntityPermission permission)
		{
			if (permission.ExplicitPermissionToRelatedContents)
				ExplicitPermissionToRelatedContents(permission);

			return base.Update(permission);
		}

		private void ExplicitPermissionToRelatedContents(EntityPermission permission)
		{
			Debug.Assert(permission != null);

			Content content = ContentPermissionRepository.GetContentByID(permission.ParentEntityId);
			if (content == null)
				throw new ApplicationException("Content has not been found. ID: " + permission.ParentEntityId);						

			IEnumerable<int> relatedContentID = content.Fields
				.Select(f => f.RelateToContentId)
				.Where(id => id.HasValue && id.Value != permission.ParentEntityId)
				.Select(id => id.Value)
				.Distinct()
				.ToArray();
			IEnumerable<int> noPermissionRelatedContentID = ContentPermissionRepository.FilterNoPermissionContent(relatedContentID, permission.UserId, permission.GroupId);
			ContentPermissionRepository.MultipleSetPermission(noPermissionRelatedContentID, permission.UserId, permission.GroupId, Constants.PermissionLevel.Read);			
		}
	}
}
