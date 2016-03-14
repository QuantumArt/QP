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
using System.IO;
using Quantumart.QP8.BLL.Services.MultistepActions;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	/// <summary>
	/// Команда этапа удаления контента
	/// </summary>
	internal class RemoveSiteCommand : IMultistepActionStageCommand
	{
		public int SiteId { get; private set; }
		public string SiteName { get; private set; }

		public RemoveSiteCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

		public RemoveSiteCommand(int siteId, string sitetName)
		{
			SiteId = siteId;
			SiteName = sitetName;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = RemovingStageCommandTypes.RemoveSite,
				ParentId = 0,
				Id = SiteId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = 1,
				StepCount = 1,
				Name = String.Format(SiteStrings.RemoveSiteStageName, (SiteName ?? ""))
			};
		}

		#region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			Site site = SiteRepository.GetById(SiteId);
			if (site == null)
				throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, SiteId));
			if (site.LockedByAnyoneElse)
				throw new ApplicationException(String.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));


			SiteRepository.Delete(SiteId);
			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}
		#endregion

		
	}
}
