using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
	public class ToolbarService
	{
		/// <summary>
		/// Возвращает список кнопок панели инструментов по коду действия
		/// </summary>
		/// <param name="actionCode">код действия</param>
		/// <returns>список кнопок панели инструментов</returns>
		public static IEnumerable<ToolbarButton> GetButtonListByActionCode(string actionCode, int entityId, int parentEntityId, bool? boundToExternal)
		{
			int id = (entityId == 0 ? parentEntityId : entityId);
			BackendAction action = BackendActionRepository.GetByCode(actionCode);					

			IEnumerable<ToolbarButton> allButtons = ToolbarRepository.GetButtonListByActionCode(action.Code, entityId);

			// если неопределен id сущности, то нужно работать с родительским entity type
			string etypeCode = entityId != 0 ? action.EntityType.Code : action.EntityType.ParentCode;
			IEnumerable<string> legalActionCodes = CustomActionResolver.CanExecuteFilter(etypeCode, id, parentEntityId, allButtons.Select(b => b.ActionCode)).ToArray();

			// Только те кнопки, для которых разрешены Action
			IEnumerable<ToolbarButton> result = allButtons.Where(b => legalActionCodes.Contains(b.ActionCode)).ToArray();
			if (actionCode.Equals(ActionCode.Articles) || actionCode.Equals(ActionCode.EditArticle))
			{
				Content content = ContentRepository.GetById(parentEntityId);

				if (actionCode.Equals(ActionCode.Articles) && (!content.WorkflowBinding.IsAssigned || !content.WorkflowBinding.CurrentUserCanRemoveArticles))
					result = result.Where(b => b.ActionCode != ActionCode.MultiplePublishArticles);


				if (!content.IsArticleChangingActionsAllowed(boundToExternal))
					result = result.Where(b => ActionCode.ArticleNonChangingActionCodes.Contains(b.ActionCode, StringComparer.InvariantCultureIgnoreCase)).ToArray();
			}
			else if (actionCode.Equals(ActionCode.ArticleVersions) || actionCode.Equals(ActionCode.PreviewArticleVersion))
			{
				Article article = ArticleRepository.GetById(parentEntityId);
				if (!article.IsArticleChangingActionsAllowed(boundToExternal))
					result = result.Where(b => ActionCode.ArticleVersionsNonChangingActionCodes.Contains(b.ActionCode, StringComparer.InvariantCultureIgnoreCase)).ToArray();
			}

			return result;
		}
	}
}
