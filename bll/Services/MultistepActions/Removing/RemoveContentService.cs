using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	public sealed class RemoveContentService : RemovingServiceAbstract
	{
		ClearContentCommand clearCommand;
		RemoveContentCommand removeCommand;

		public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
		{
			Content content = ContentRepository.GetById(contentId);
			if (content == null)
				throw new ApplicationException(String.Format(ContentStrings.ContentNotFound, contentId));

			if (!content.IsAccessible(ActionTypeCode.Remove))
				throw new ApplicationException(ArticleStrings.CannotRemoveBecauseOfSecurity);

			List<string> violationMessages = new List<string>();
			content.ValidateForRemove(violationMessages);
			if (violationMessages.Count > 0)
				throw new ApplicationException(String.Join(Environment.NewLine, violationMessages));

			DataRow row = ClearContentRepository.GetContentItemsInfo(contentId);
			string contentName = "";
			int itemCount = 0;
			if (row != null)
			{
				itemCount = row.Field<int>("ITEMS_COUNT");
				contentName = row.Field<string>("CONTENT_NAME");
			}

			clearCommand = new ClearContentCommand(siteId, contentId, contentName, itemCount);
			removeCommand = new RemoveContentCommand(siteId, contentId, contentName);
			return base.Setup(siteId, contentId, boundToExternal);
		}
		
		protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
		{
			return new MultistepActionSettings 
			{ 
				Stages = new[] 
				{ 
					clearCommand.GetStageSettings(),
					removeCommand.GetStageSettings()
				} 
			};
		}

		protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionStageCommandState clearCommandState = clearCommand.GetState();
			return new MultistepActionServiceContext 
			{ 
				CommandStates = new[] 
				{ 
					clearCommand.GetState(),
 					removeCommand.GetState()
				} 
			};
		}
	}
}
