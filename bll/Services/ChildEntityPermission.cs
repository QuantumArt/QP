using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    /// <summary>
    /// Правила доступа для дочерних сущьностей
    /// </summary>
    public class ChildEntityPermission : EntityPermissionBase
    {
        public static ChildEntityPermission Create(IChildEntityPermissionService service, int parentEntityId,
            int? userId = null, int? groupId = null,
            int? permissionLevelId = null, bool propagateToItems = false, bool hide = false) => new ChildEntityPermission
        {
            ParentEntityId = parentEntityId,
            UserId = userId,
            GroupId = groupId,
            PermissionLevelId = permissionLevelId ?? -1,
            PropagateToItems = propagateToItems,
            CopyParentPermission = false,
            Hide = hide
        };

        public static ChildEntityPermission CreateFrom(EntityPermission permission, bool createFromParent = true) => new ChildEntityPermission
        {
            UserId = permission.UserId,
            GroupId = permission.GroupId,
            PermissionLevelId = permission.PermissionLevelId,
            PropagateToItems = permission.PropagateToItems,
            CopyParentPermission = createFromParent,
            ParentEntityId = permission.ParentEntityId,
            Hide = permission.Hide
        };

        [Display(Name = "PropagateToItems", ResourceType = typeof(EntityPermissionStrings))]
        public bool PropagateToItems { get; set; }

        [Display(Name = "CopyParentPermission", ResourceType = typeof(EntityPermissionStrings))]
        public bool CopyParentPermission { get; set; }

        [Display(Name = "ExplicitPermissionToRelatedContents", ResourceType = typeof(EntityPermissionStrings))]
        public bool ExplicitPermissionToRelatedContents { get; set; }

        [Display(Name = "Hide", ResourceType = typeof(EntityPermissionStrings))]
        public bool Hide { get; set; }
    }
}
