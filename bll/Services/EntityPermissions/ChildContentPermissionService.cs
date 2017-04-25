using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using System.Diagnostics;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class ChildContentPermissionService : ChildEntityPermissionServiceAbstract
	{
		private Lazy<IChildContentPermissionRepository> repository = new Lazy<IChildContentPermissionRepository>(() => new ChildContentPermissionRepository());

		protected override IChildEntityPermissionRepository Repository { get { return repository.Value; } }
		private IChildContentPermissionRepository ContentPermissionRepository { get { return repository.Value; } }

		#region IChildEntityPermissionService Members

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get 
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.ChildContentPermissions,
					EntityTypeCode = EntityTypeCode.ContentPermission,
					PermissionEntityTypeCode = EntityTypeCode.ContentPermission,
					ContextMenuCode = "child_content_permission",
					ActionCodeForLink = ActionCode.ContentPermissionsForChild,
					ParentPermissionsListAction = ActionCode.SitePermissions,
					IsPropagateable = true,
					CanHide = true,
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get 
			{
				return new GenericPermissionViewModelSettings
				{
					EntityTypeCode = EntityTypeCode.ContentPermission,
					IsPropagateable = true,
					CanHide = true
				};
			}
		}

		public override void MultipleChange(int parentId, IEnumerable<int> entityIDs, ChildEntityPermission permissionSettings)
		{
			if (permissionSettings.ExplicitPermissionToRelatedContents)
				ExplicitPermissionToRelatedContents(entityIDs, permissionSettings);

			base.MultipleChange(parentId, entityIDs, permissionSettings);
		}

		public override void Change(int parentId, int entityId, ChildEntityPermission permissionSettings)
		{
			if (permissionSettings.ExplicitPermissionToRelatedContents)
				ExplicitPermissionToRelatedContents(new[]{ entityId }, permissionSettings);
			
			base.Change(parentId, entityId, permissionSettings);
		}

		private void ExplicitPermissionToRelatedContents(IEnumerable<int> contentIDs, ChildEntityPermission permission)
		{
			Debug.Assert(contentIDs != null);
			Debug.Assert(permission != null);

			IEnumerable<Content> contents = ContentPermissionRepository.GetContentList(contentIDs);

			IEnumerable<int> relatedContentID = contents.SelectMany(c => c.Fields.Select(f => f.RelateToContentId))
				.Distinct()
				.Where(id => id.HasValue && !contentIDs.Contains(id.Value))
				.Select(id => id.Value)
				.ToArray();			


			IEnumerable<int> noPermissionRelatedContentID = ContentPermissionRepository.FilterNoPermissionContent(relatedContentID, permission.UserId, permission.GroupId);
			ContentPermissionRepository.MultipleSetPermission(noPermissionRelatedContentID, permission.UserId, permission.GroupId, Constants.PermissionLevel.Read);
		}
		
		#endregion
	}
}
