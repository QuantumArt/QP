using System;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    public sealed class RemoveSiteService : RemovingServiceAbstract
    {
        private RemoveSiteArticlesCommand removeSiteArticlesCommand;
        private RemoveSiteContentsCommand removeSiteContentsCommand;
        private RemoveSiteCommand removeSiteCommand;

        public override MultistepActionSettings Setup(int dbId, int siteId, bool? boundToExternal)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }
            if (site.LockedByAnyoneElse)
            {
                throw new ApplicationException(string.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));
            }

            var sharedUnionBaseContentsName = ContentRepository.GetSharedUnionBaseContentNames(siteId);
            if (sharedUnionBaseContentsName.Any())
            {
                var message = string.Format(ContentStrings.ContentsAreSharedUnionBase,
                    Environment.NewLine + string.Join(Environment.NewLine, sharedUnionBaseContentsName.Distinct(StringComparer.InvariantCultureIgnoreCase)));
                throw new ApplicationException(message);
            }

            var sharedRelatedContentsName = ContentRepository.GetSharedRelatedContentNames(siteId);
            if (sharedRelatedContentsName.Any())
            {
                var message = string.Format(ContentStrings.ContentsAreSharedRelated,
                    Environment.NewLine + string.Join(Environment.NewLine, sharedRelatedContentsName.Distinct(StringComparer.InvariantCultureIgnoreCase)));
                throw new ApplicationException(message);
            }

            var articleCount = SiteRepository.GetSiteArticleCount(siteId);
            var contentCount = SiteRepository.GetSiteContentCount(siteId);

            removeSiteArticlesCommand = new RemoveSiteArticlesCommand(siteId, site.Name, articleCount);
            removeSiteContentsCommand = new RemoveSiteContentsCommand(siteId, site.Name, contentCount);
            removeSiteCommand = new RemoveSiteCommand(siteId, site.Name);

            return base.Setup(dbId, siteId, boundToExternal);
        }

        protected override MultistepActionSettings CreateActionSettings(int dbId, int siteId) => new MultistepActionSettings
        {
            Stages = new[]
            {
                removeSiteArticlesCommand.GetStageSettings(),
                removeSiteContentsCommand.GetStageSettings(),
                removeSiteCommand.GetStageSettings()
            }
        };

        protected override MultistepActionServiceContext CreateContext(int dbId, int siteId, bool? boundToExternal) => new MultistepActionServiceContext
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
