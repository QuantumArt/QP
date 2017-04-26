using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.EntityPermissions;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Правила доступа для дочерних сущьностей
	/// </summary>
	public class ChildEntityPermission : EntityPermissionBase
	{
        public static ChildEntityPermission Create(IChildEntityPermissionService service, int parentEntityId, 
            int? userId = null, int? groupId = null,
            int? permissionLevelId = null, bool propagateToItems = false, bool hide = false)
        {
            return new ChildEntityPermission
            {
                ParentEntityId = parentEntityId,
                UserId = userId,
                GroupId = groupId,
                PermissionLevelId = permissionLevelId ?? -1,
                PropagateToItems = propagateToItems,
                CopyParentPermission = false,
				Hide = hide,
            };
        }


        public static ChildEntityPermission CreateFrom(EntityPermission permission, bool createFromParent = true)
		{
			return new ChildEntityPermission
			{
				UserId = permission.UserId,
				GroupId = permission.GroupId,
				PermissionLevelId = permission.PermissionLevelId,
				PropagateToItems = permission.PropagateToItems,
                CopyParentPermission = createFromParent,
				ParentEntityId = permission.ParentEntityId,
				Hide = permission.Hide
			};
		}
		
		
		[LocalizedDisplayName("PropagateToItems", NameResourceType = typeof(EntityPermissionStrings))]
		public bool PropagateToItems { get; set; }

		[LocalizedDisplayName("CopyParentPermission", NameResourceType = typeof(EntityPermissionStrings))]
		public bool CopyParentPermission { get; set; }

		[LocalizedDisplayName("ExplicitPermissionToRelatedContents", NameResourceType = typeof(EntityPermissionStrings))]
		public bool ExplicitPermissionToRelatedContents { get; set; }

		[LocalizedDisplayName("Hide", NameResourceType = typeof(EntityPermissionStrings))]
		public bool Hide { get; set; }
	}
}
