#if !NET_STANDARD
using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    internal class CreateFoldersCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; }
        public string SiteName { get; }

        public CreateFoldersCommand(MultistepActionStageCommandState state)
            : this(state.Id, null)
        {
        }

        public CreateFoldersCommand(int siteId, string sitetName)
        {
            SiteId = siteId;
            SiteName = sitetName;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = BuildSiteStageCommandTypes.CreateFolders,
            ParentId = 0,
            Id = SiteId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = 1,
            StepCount = 1,
            Name = string.Format(SiteStrings.CreateSiteFoldersStageName, SiteName ?? "")
        };

        #region IMultistepActionStageCommand Members

        public MultistepActionStepResult Step(int step)
        {
            var site = SiteRepository.GetById(SiteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, SiteId));
            }
            if (!site.IsDotNet)
            {
                throw new ApplicationException(string.Format(SiteStrings.ShouldBeDotNet));
            }

            if (site.IsLive)
            {
                site.CreateLiveSiteFolders();
            }
            else
            {
                site.CreateStageSiteFolders();
            }

            return new MultistepActionStepResult { ProcessedItemsCount = 1 };
        }

        #endregion
    }
}
#endif
