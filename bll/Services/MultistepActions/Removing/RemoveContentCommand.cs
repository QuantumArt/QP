using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using System.Data;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	/// <summary>
	/// Команда этапа удаления контента
	/// </summary>
	internal class RemoveContentCommand : IMultistepActionStageCommand
	{
		public int ContentId { get; private set; }
		public int SiteId { get; private set; }
		public string ContentName { get; private set; }

		public RemoveContentCommand(MultistepActionStageCommandState state) : this(state.ParentId, state.Id, null) { }

		public RemoveContentCommand(int siteId, int contentId, string contentName)
		{
			SiteId = siteId;
			ContentId = contentId;
			ContentName = contentName;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = RemovingStageCommandTypes.RemoveContent,
				ParentId = SiteId,
				Id = ContentId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = 1,
				StepCount = 1,
				Name = String.Format(ContentStrings.RemoveContentStageName, (ContentName ?? ""))
			};
		}

		#region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			Content content = ContentRepository.GetById(ContentId);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, ContentId));
			
			content.DieWithoutValidation();

			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}
		#endregion

		

		
	}
}
