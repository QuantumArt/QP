using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	/// <summary>
	/// Команда этапа удаления контента
	/// </summary>
	internal class RemoveSiteContentsCommand : IMultistepActionStageCommand
	{
		private static readonly int ITEMS_PER_STEP = 20;

		public int SiteId { get; private set; }
		public string SiteName { get; private set; }
		public int ItemCount { get; private set; }

		public RemoveSiteContentsCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

		public RemoveSiteContentsCommand(int siteId, string sitetName, int itemCount)
		{
			SiteId = siteId;
			ItemCount = itemCount;
			SiteName = sitetName;
		}

		public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
		{
		    Type = RemovingStageCommandTypes.RemoveSiteContents,
		    ParentId = 0,
		    Id = SiteId
		};

	    public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
	    {
	        ItemCount = ItemCount,
	        StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
	        Name = string.Format(SiteStrings.RemoveSiteContentsStageName, (SiteName ?? ""))
	    };

	    #region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			var site = SiteRepository.GetById(SiteId);
			if (site == null)
			{
			    throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, SiteId));
			}

		    var deletedContentsIds = ContentRepository.BatchRemoveContents(SiteId, ITEMS_PER_STEP);				
			var deletedContentsStrIds = new HashSet<string>(deletedContentsIds.Select(id => id.ToString()), StringComparer.InvariantCultureIgnoreCase);

			var sitePath = site.BasePathInfo.GetSubPathInfo("contents").Path;
			if (Directory.Exists(sitePath))
			{
				foreach (var sd in new DirectoryInfo(sitePath).EnumerateDirectories().Where(i => deletedContentsStrIds.Contains(i.Name)))
				{
					sd.Delete(true);
				}
			}
			return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
		}
		#endregion

		
	}
}
