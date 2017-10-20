using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public class ArticlePermissionService : PermissionServiceAbstract
    {
        private readonly Lazy<IPermissionRepository> _repository = new Lazy<IPermissionRepository>(() => new ArticlePermissionRepository());

        public override IPermissionRepository Repository => _repository.Value;

        public override IPermissionListViewModelSettings ListViewModelSettings => new GenericPermissionListViewModelSettings
        {
            ActionCode = ActionCode.ArticlePermissions,
            AddNewItemActionCode = ActionCode.AddNewArticlePermission,
            EntityTypeCode = EntityTypeCode.ArticlePermission,
            IsPropagateable = false,
            CanHide = false,
            PermissionEntityTypeCode = EntityTypeCode.ArticlePermission
        };

        public override IPermissionViewModelSettings ViewModelSettings => new GenericPermissionViewModelSettings
        {
            ActionCode = ActionCode.ArticlePermissionProperties,
            EntityTypeCode = EntityTypeCode.ArticlePermission,
            IsPropagateable = false,
            CanHide = false
        };

        public override PermissionInitListResult InitList(int parentId)
        {
            var result = base.InitList(parentId);
            var artcile = ArticleRepository.GetById(parentId);
            result.IsEnableArticlesPermissionsAccessable = SecurityRepository.IsActionAccessible(ActionCode.EnableArticlesPermissions) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, artcile.ContentId, ActionTypeCode.Update);
            return result;
        }

        public override EntityPermission Save(EntityPermission permission)
        {
            if (ArticleRepository.IsAnyAggregatedFields(permission.ParentEntityId))
            {
                throw new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            return base.Save(permission);
        }

        public override EntityPermission Update(EntityPermission permission)
        {
            if (ArticleRepository.IsAnyAggregatedFields(permission.ParentEntityId))
            {
                throw new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            return base.Update(permission);
        }

        public override MessageResult Remove(int parentId, int id) => ArticleRepository.IsAnyAggregatedFields(parentId)
            ? MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated)
            : base.Remove(parentId, id);

        public override MessageResult MultipleRemove(int parentId, IEnumerable<int> ids) => ArticleRepository.IsAnyAggregatedFields(parentId)
            ? MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated)
            : base.MultipleRemove(parentId, ids);
    }
}
