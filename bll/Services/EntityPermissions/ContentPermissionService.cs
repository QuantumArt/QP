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
        private readonly Lazy<IContentPermissionRepository> _repository = new Lazy<IContentPermissionRepository>(() => new ContentPermissionRepository());

        public override IPermissionRepository Repository => _repository.Value;

        private IContentPermissionRepository ContentPermissionRepository => _repository.Value;

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.ContentPermissions,
            AddNewItemActionCode = ActionCode.AddNewContentPermission,
            EntityTypeCode = EntityTypeCode.ContentPermission,
            IsPropagateable = true,
            CanHide = true,
            PermissionEntityTypeCode = EntityTypeCode.ContentPermission
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            ActionCode = ActionCode.ContentPermissionProperties,
            EntityTypeCode = EntityTypeCode.ContentPermission,
            IsPropagateable = true,
            CanHide = true
        };

        public override EntityPermission Save(EntityPermission permission)
        {
            if (permission.ExplicitPermissionToRelatedContents)
            {
                ExplicitPermissionToRelatedContents(permission);
            }

            return base.Save(permission);
        }

        public override EntityPermission Update(EntityPermission permission)
        {
            if (permission.ExplicitPermissionToRelatedContents)
            {
                ExplicitPermissionToRelatedContents(permission);
            }

            return base.Update(permission);
        }

        private void ExplicitPermissionToRelatedContents(EntityPermissionBase permission)
        {
            Debug.Assert(permission != null);

            var content = ContentPermissionRepository.GetContentById(permission.ParentEntityId);
            if (content == null)
            {
                throw new ApplicationException("Content has not been found. ID: " + permission.ParentEntityId);
            }

            IEnumerable<int> relatedContentId = content.Fields
                .Select(f => f.RelateToContentId)
                .Where(id => id.HasValue && id.Value != permission.ParentEntityId)
                .Select(id => id.Value)
                .Distinct()
                .ToArray();

            var noPermissionRelatedContentId = ContentPermissionRepository.FilterNoPermissionContent(relatedContentId, permission.UserId, permission.GroupId);
            ContentPermissionRepository.MultipleSetPermission(noPermissionRelatedContentId, permission.UserId, permission.GroupId, PermissionLevel.Read);
        }
    }
}
