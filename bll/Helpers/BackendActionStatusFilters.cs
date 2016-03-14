using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class BackendActionStatusFilters
	{
		public static IEnumerable<BackendActionStatus> ResolveStatusesForArticle(this IEnumerable<BackendActionStatus> statuses, int entityId, bool? boundToExternal)
		{
			Article article = ArticleRepository.GetById(entityId);
			if (!article.Content.HasTreeField)
			{
				BackendActionStatus status = statuses.Where(s => s.Code == ActionCode.AddNewChildArticle).SingleOrDefault();
				if (status != null)
					status.Visible = false;
			}

			if (!article.IsArticleChangingActionsAllowed(boundToExternal))
			{
				var excluded = statuses.Where(s => ActionCode.ArticleNonChangingActionCodes.Contains(s.Code));
				foreach (var excludedItem in excluded)
					excludedItem.Visible = false;
			}
			return statuses;
		}

		public static IEnumerable<BackendActionStatus> ResolveStatusesForArticleVersion(this IEnumerable<BackendActionStatus> statuses, int entityId, bool? boundToExternal)
		{
			ArticleVersion version = ArticleVersionRepository.GetById(entityId);
			if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
			{
				var excluded = statuses.Where(s => ActionCode.ArticleNonChangingActionCodes.Contains(s.Code));
				foreach (var excludedItem in excluded)
					excludedItem.Visible = false;
			}
			return statuses;
		}

		/// <summary>
		/// Устанавливает соответствующий статус элементам меню связанным с Custom Action
		/// </summary>		
		public static IEnumerable<BackendActionStatus> ResolveStatusesForCustomActions(this IEnumerable<BackendActionStatus> statuses, string menuCode, int entityId, int parentEntityId)
		{
			ContextMenu menu = ContextMenuRepository.GetByCode(menuCode, false);
			if (menu == null)
				return Enumerable.Empty<BackendActionStatus>();
			else if (menu.EntityType == null)
				return statuses;
			else
				return CustomActionResolver.ResolveStatus(menu.EntityType.Code, entityId, parentEntityId, statuses.ToArray());
		}

		/// <summary>
		/// Делает невидимыми некоторые элементы контекстного меню для корневого Site Folder 
		/// </summary>		
		public static IEnumerable<BackendActionStatus> ResolveStatusesForSiteFolder(this IEnumerable<BackendActionStatus> statuses, int entityId)
		{
			Folder folder = SiteFolderService.GetById(entityId);
			if (!folder.ParentId.HasValue)
			{
				string[] codes = new string[] { ActionCode.SiteFolderProperties, ActionCode.RemoveSiteFolder };
				foreach (var status in statuses)
				{
					if (codes.Contains(status.Code))
						status.Visible = false;
				}
			}
			return statuses;
		}

		public static IEnumerable<BackendActionStatus> ResolveStatusesForContentGroup(this IEnumerable<BackendActionStatus> statuses, int entityId)
		{
			ContentGroup group = ContentRepository.GetGroupById(entityId);
			if (group.IsDefault)
			{
				BackendActionStatus status = statuses.Where(n => n.Code == ActionCode.ContentGroupProperties).SingleOrDefault();
				if (statuses != null)
					status.Visible = false;
			}
			return statuses;
		}

		/// <summary>
		/// Делает невидимыми некоторые элементы контекстного меню для корневого Content Folder 
		/// </summary>		
		public static IEnumerable<BackendActionStatus> ResolveStatusesForContentFolder(this IEnumerable<BackendActionStatus> statuses, int entityId)
		{
			Folder folder = ContentFolderService.GetById(entityId);
			if (!folder.ParentId.HasValue)
			{
				string[] codes = new string[] { ActionCode.ContentFolderProperties, ActionCode.RemoveContentFolder };
				foreach (var status in statuses)
				{
					if (codes.Contains(status.Code))
						status.Visible = false;
				}
			}
			return statuses;
		}

		public static IEnumerable<BackendActionStatus> ResolveStatusesForField(this IEnumerable<BackendActionStatus> statuses, int entityId)
		{
			Field field = FieldRepository.GetById(entityId);
			if (field != null)
			{
				if (field.TypeId != FieldTypeCodes.DynamicImage)
				{
					var dynStatus = statuses.Where(s => s.Code == ActionCode.RecreateDynamicImages).SingleOrDefault();
					if (dynStatus != null)
						dynStatus.Visible = false;
				}
				
				if (field.TypeId == FieldTypeCodes.DynamicImage || field.TypeId == FieldTypeCodes.M2ORelation)
				{
					var applyStatus = statuses.Where(s => s.Code == ActionCode.ApplyFieldDefaultValue).SingleOrDefault();
					if (applyStatus != null)
						applyStatus.Visible = false;
				}
			}
			return statuses;
		}
	}
}
