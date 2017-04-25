using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	public sealed class RemoveSiteService : RemovingServiceAbstract
	{
		RemoveSiteArticlesCommand removeSiteArticlesCommand;
		RemoveSiteContentsCommand removeSiteContentsCommand;
		RemoveSiteCommand removeSiteCommand;

		public override MultistepActionSettings Setup(int dbId, int siteId, bool? boundToExternal)
		{
			Site site = SiteRepository.GetById(siteId);
			if (site == null)
				throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, siteId));
			if (site.LockedByAnyoneElse)
				throw new ApplicationException(String.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));

			IEnumerable<string> sharedUnionBaseContentsName = ContentRepository.GetSharedUnionBaseContentNames(siteId);
			if (sharedUnionBaseContentsName.Any())
			{
				string message = String.Format(ContentStrings.ContentsAreSharedUnionBase,
					Environment.NewLine + String.Join(Environment.NewLine, sharedUnionBaseContentsName.Distinct(StringComparer.InvariantCultureIgnoreCase)));
				throw new ApplicationException(message);
			}

			IEnumerable<string> sharedRelatedContentsName = ContentRepository.GetSharedRelatedContentNames(siteId);
			if (sharedRelatedContentsName.Any())
			{
				string message = String.Format(ContentStrings.ContentsAreSharedRelated,
					Environment.NewLine + String.Join(Environment.NewLine, sharedRelatedContentsName.Distinct(StringComparer.InvariantCultureIgnoreCase)));
				throw new ApplicationException(message);
			}

			int articleCount = SiteRepository.GetSiteArticleCount(siteId);
			int contentCount = SiteRepository.GetSiteContentCount(siteId);

			removeSiteArticlesCommand = new RemoveSiteArticlesCommand(siteId, site.Name, articleCount);
			removeSiteContentsCommand = new RemoveSiteContentsCommand(siteId, site.Name, contentCount);
			removeSiteCommand = new RemoveSiteCommand(siteId, site.Name);
			
			return base.Setup(dbId, siteId, boundToExternal);
		}
		protected override MultistepActionSettings CreateActionSettings(int dbId, int siteId)
		{
			return new MultistepActionSettings
			{
				Stages = new[] 
				{ 
					removeSiteArticlesCommand.GetStageSettings(),
					removeSiteContentsCommand.GetStageSettings(),
					removeSiteCommand.GetStageSettings()
				} 
			};
		}

		protected override MultistepActionServiceContext CreateContext(int dbId, int siteId, bool? boundToExternal)
		{
			return new MultistepActionServiceContext
			{
				CommandStates = new[] 
				{ 
					removeSiteArticlesCommand.GetState(),
 					removeSiteContentsCommand.GetState(),
 					removeSiteCommand.GetState()
				} 
			};
		}
	}
}
