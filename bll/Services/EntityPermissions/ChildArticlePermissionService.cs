using System.Collections.Generic;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public class ChildArticlePermissionService : ChildEntityPermissionServiceAbstract
    {
        protected override IChildEntityPermissionRepository Repository => new ChildArticlePermissionRepository();

        #region IChildEntityPermissionService Members

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.ChildArticlePermissions,
            EntityTypeCode = EntityTypeCode.ArticlePermission,
            PermissionEntityTypeCode = EntityTypeCode.ArticlePermission,
            ContextMenuCode = "child_article_permission",
            ActionCodeForLink = ActionCode.ArticlePermissionsForChild,
            ParentPermissionsListAction = ActionCode.ContentPermissions,
            IsPropagateable = false,
            CanHide = false
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            EntityTypeCode = EntityTypeCode.ArticlePermission,
            IsPropagateable = false,
            CanHide = false
        };

        public override void MultipleChange(int parentId, List<int> entityIDs, ChildEntityPermission permissionSettings)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                throw new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            base.MultipleChange(parentId, entityIDs, permissionSettings);
        }

        public override void Change(int parentId, int entityId, ChildEntityPermission permissionSettings)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                throw new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            base.Change(parentId, entityId, permissionSettings);
        }

        public override void ChangeAll(int parentId, ChildEntityPermission permissionSettings)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                throw new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            base.ChangeAll(parentId, permissionSettings);
        }

        public override MessageResult MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            return base.MultipleRemove(parentId, entityIDs, userId, groupId);
        }

        public override MessageResult Remove(int parentId, int entityId, int? userId, int? groupId)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            return base.Remove(parentId, entityId, userId, groupId);
        }

        public override MessageResult RemoveAll(int parentId, int? userId, int? groupId)
        {
            if (ContentRepository.IsAnyAggregatedFields(parentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            return base.RemoveAll(parentId, userId, groupId);
        }

        #endregion
    }
}
