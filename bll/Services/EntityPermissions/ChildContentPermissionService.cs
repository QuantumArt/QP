using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public class ChildContentPermissionService : ChildEntityPermissionServiceAbstract
    {
        private readonly Lazy<IChildContentPermissionRepository> _repository = new Lazy<IChildContentPermissionRepository>(() => new ChildContentPermissionRepository());

        protected override IChildEntityPermissionRepository Repository => _repository.Value;

        private IChildContentPermissionRepository ContentPermissionRepository => _repository.Value;

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.ChildContentPermissions,
            EntityTypeCode = EntityTypeCode.ContentPermission,
            PermissionEntityTypeCode = EntityTypeCode.ContentPermission,
            ContextMenuCode = "child_content_permission",
            ActionCodeForLink = ActionCode.ContentPermissionsForChild,
            ParentPermissionsListAction = ActionCode.SitePermissions,
            IsPropagateable = true,
            CanHide = true
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            EntityTypeCode = EntityTypeCode.ContentPermission,
            IsPropagateable = true,
            CanHide = true
        };

        public override void MultipleChange(int parentId, List<int> entityIDs, ChildEntityPermission permissionSettings)
        {
            if (permissionSettings.ExplicitPermissionToRelatedContents)
            {
                ExplicitPermissionToRelatedContents(entityIDs, permissionSettings);
            }

            base.MultipleChange(parentId, entityIDs, permissionSettings);
        }

        public override void Change(int parentId, int entityId, ChildEntityPermission permissionSettings)
        {
            if (permissionSettings.ExplicitPermissionToRelatedContents)
            {
                ExplicitPermissionToRelatedContents(new[] { entityId }, permissionSettings);
            }

            base.Change(parentId, entityId, permissionSettings);
        }

        private void ExplicitPermissionToRelatedContents(ICollection<int> contentIds, EntityPermissionBase permission)
        {
            var contents = ContentPermissionRepository.GetContentList(contentIds);
            IEnumerable<int> relatedContentId = contents.SelectMany(c => c.Fields.Select(f => f.RelateToContentId))
                .Distinct()
                .Where(id => id.HasValue && !contentIds.Contains(id.Value))
                .Select(id => id.Value)
                .ToArray();

            var noPermissionRelatedContentId = ContentPermissionRepository.FilterNoPermissionContent(relatedContentId, permission.UserId, permission.GroupId);
            ContentPermissionRepository.MultipleSetPermission(noPermissionRelatedContentId, permission.UserId, permission.GroupId, PermissionLevel.Read);
        }
    }
}
