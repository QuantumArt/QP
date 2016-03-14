using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
	internal class CreateFoldersCommand : IMultistepActionStageCommand
	{
		public int SiteId { get; private set; }
		public string SiteName { get; private set; }

		public CreateFoldersCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

		public CreateFoldersCommand(int siteId, string sitetName)
		{
			SiteId = siteId;
			SiteName = sitetName;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = BuildSiteStageCommandTypes.CreateFolders,
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
				Name = String.Format(SiteStrings.CreateSiteFoldersStageName, (SiteName ?? ""))
			};
		}
		
		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			Site site = SiteRepository.GetById(SiteId);
			if (site == null)
				throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, SiteId));
			if (!site.IsDotNet)
				throw new ApplicationException(String.Format(SiteStrings.ShouldBeDotNet));
			if (site.IsLive)
				site.CreateLiveSiteFolders();
			else
				site.CreateStageSiteFolders();

			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}

		#endregion
	}
}
