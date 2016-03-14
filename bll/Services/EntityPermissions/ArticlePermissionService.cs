using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public class ArticlePermissionService : PermissionServiceAbstract
	{
		private Lazy<IPermissionRepository> repository = new Lazy<IPermissionRepository>(() => new ArticlePermissionRepository());
		public override IPermissionRepository Repository { get { return repository.Value; } }

		public override IPermissionListViewModelSettings ListViewModelSettings
		{
			get 
			{
				return new GenericPermissionListViewModelSettings
				{
					ActionCode = ActionCode.ArticlePermissions,
					AddNewItemActionCode = ActionCode.AddNewArticlePermission,
					EntityTypeCode = EntityTypeCode.ArticlePermission,
					IsPropagateable = false,
					CanHide = false,
					PermissionEntityTypeCode = EntityTypeCode.ArticlePermission
				};
			}
		}

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get 
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.ArticlePermissionProperties,
					EntityTypeCode = EntityTypeCode.ArticlePermission,
					IsPropagateable = false,
					CanHide = false
				};
			}
		}

		public override PermissionInitListResult InitList(int parentId)
		{
			PermissionInitListResult result = base.InitList(parentId);
			Article artcile = ArticleRepository.GetById(parentId);
			result.IsEnableArticlesPermissionsAccessable = SecurityRepository.IsActionAccessible(ActionCode.EnableArticlesPermissions) &&
				SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, artcile.ContentId, ActionTypeCode.Update);
			return result;
		}

		public override EntityPermission Save(EntityPermission permission)
		{
			if (ArticleRepository.IsAnyAggregatedFields(permission.ParentEntityId))
				throw ActionNotAllowedException.CreateNotAllowedForAggregatedArticleException();

			return base.Save(permission);
		}

		public override EntityPermission Update(EntityPermission permission)
		{
			if (ArticleRepository.IsAnyAggregatedFields(permission.ParentEntityId))
				throw ActionNotAllowedException.CreateNotAllowedForAggregatedArticleException();

			return base.Update(permission);
		}

		public override MessageResult Remove(int parentId, int id)
		{
			if (ArticleRepository.IsAnyAggregatedFields(parentId))
				return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);	

			return base.Remove(parentId, id);
		}

		public override MessageResult MultipleRemove(int parentId, IEnumerable<int> IDs)
		{
			if (ArticleRepository.IsAnyAggregatedFields(parentId))
				return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);	
			return base.MultipleRemove(parentId, IDs);
		}
	}
}
