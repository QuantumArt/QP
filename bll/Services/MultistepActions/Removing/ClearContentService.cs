using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using System.Data;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	public sealed class ClearContentService : RemovingServiceAbstract
	{
		private ClearContentCommand command;

		public override MessageResult PreAction(int siteId, int contentId)
		{			
			if(ContentRepository.IsAnyAggregatedFields(contentId))
				return MessageResult.Info(ContentStrings.OperationIsNotAllowedForAggregated);
			if(ContentRepository.GetAggregatedContents(contentId).Any())
				return MessageResult.Info(ContentStrings.OperationIsNotAllowedForRoot);

			else
				return base.PreAction(siteId, contentId);
		}

		public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
		{							
			if (ContentRepository.IsAnyAggregatedFields(contentId))
				throw ActionNotAllowedException.CreateNotAllowedForAggregatedContentException();
			if (ContentRepository.GetAggregatedContents(contentId).Any())
				throw ActionNotAllowedException.CreateNotAllowedForRootContentException();

			Content content = ContentRepository.GetById(contentId);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, contentId));
			if (!content.IsContentChangingActionsAllowed)
				throw ActionNotAllowedException.CreateNotAllowedForContentChangingActionException();

			DataRow row = ClearContentRepository.GetContentItemsInfo(contentId);
			string contentName = "";
			int itemCount = 0;
			if (row != null)
			{
				itemCount = row.Field<int>("ITEMS_COUNT");
				contentName = row.Field<string>("CONTENT_NAME");
			}
			command = new ClearContentCommand(siteId, contentId, contentName, itemCount);
			return base.Setup(siteId, contentId, boundToExternal);
		}

		protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
		{	
			MultistepStageSettings stageSetting = command.GetStageSettings();
			return new MultistepActionSettings { Stages = new[] { stageSetting } };
		}

		protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionStageCommandState commandState = command.GetState();
			return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
		}		
	}
}
