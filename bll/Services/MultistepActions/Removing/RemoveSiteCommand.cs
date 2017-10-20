using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

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

		public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
		{
		    Type = RemovingStageCommandTypes.RemoveSite,
		    ParentId = 0,
		    Id = SiteId
		};

	    public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
	    {
	        ItemCount = 1,
	        StepCount = 1,
	        Name = string.Format(SiteStrings.RemoveSiteStageName, (SiteName ?? ""))
	    };

	    #region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			var site = SiteRepository.GetById(SiteId);
			if (site == null)
			{
			    throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, SiteId));
			}
		    if (site.LockedByAnyoneElse)
		    {
		        throw new ApplicationException(string.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));
		    }

		    SiteRepository.Delete(SiteId);
			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}
		#endregion

		
	}
}
