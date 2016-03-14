using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using System.Data;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	/// <summary>
	/// Команда этапа очистки контента
	/// </summary>
	internal class ClearContentCommand : IMultistepActionStageCommand
	{
		private static readonly int ITEMS_PER_STEP = 100;

		public int ContentId { get; private set; }
		public int SiteId { get; private set; }
		public int ItemCount { get; private set; }
		public string ContentName { get; private set; }

		public ClearContentCommand(MultistepActionStageCommandState state) : this(state.ParentId, state.Id, null, 0) { }

		public ClearContentCommand(int siteId, int contentId, string contentName, int itemCount)
		{
			SiteId = siteId;
			ContentId = contentId;
			ContentName = contentName;
			ItemCount = itemCount;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = RemovingStageCommandTypes.ClearContent,
				ParentId = SiteId,
				Id = ContentId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = ItemCount,
				StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
				Name = String.Format(ContentStrings.ClearContentStageName, (ContentName ?? ""))
			};
		}

		public bool IsLastStep(int step)
		{
			return step == GetStageSettings().StepCount - 1;
		}

		#region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			Content content = ContentRepository.GetById(ContentId);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, ContentId));
			if (!content.IsContentChangingActionsAllowed)
				throw ActionNotAllowedException.CreateNotAllowedForContentChangingActionException();

			ClearContentRepository.RemoveContentItems(ContentId, ITEMS_PER_STEP);

			if (IsLastStep(step))
				ClearContentRepository.ClearO2MRelations(ContentId);

			return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
		}
		#endregion

		

		
	}
}
