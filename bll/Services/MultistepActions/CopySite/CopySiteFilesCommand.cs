using System;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteFilesCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; set; }

        public int? NewSiteId { get; set; }

        public string SiteName { get; set; }

        public long AllFilesSize { get; set; }

        public int AllFileCount { get; set; }

        public int StepsCount { get; set; }

        public CopySiteFilesCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, false)
        {
        }

        public CopySiteFilesCommand(int siteId, string siteName, bool doNotCopyFiles)
        {
            SiteId = siteId;
            SiteName = siteName;

            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            NewSiteId = prms.DestinationSiteId;

            var sourceSite = SiteRepository.GetById(SiteId);
            if (doNotCopyFiles)
            {
                StepsCount = 0;
                return;
            }

            AllFilesSize = CopySiteFilesHelper.GetAllFileSize(sourceSite);
            AllFileCount = CopySiteFilesHelper.FilesCount(sourceSite);

            var steps = (double)AllFilesSize / CopySiteFilesHelper.FilesSizeLimitPerTransaction;
            StepsCount = CleverRoundSteps(steps);
        }

        private static int CleverRoundSteps(double steps)
        {
            var roundedSteps = Math.Round(steps);
            if (roundedSteps >= steps)
            {
                return (int)Math.Round(steps);
            }

            return (int)roundedSteps + 1;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            var sourceSite = SiteRepository.GetById(SiteId);
            var destinationSite = SiteRepository.GetById(NewSiteId.Value);

            var helper = new CopySiteFilesHelper(sourceSite, destinationSite);
            helper.CopyDirectories();
            result.ProcessedItemsCount = helper.CopyFiles(step);

            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteFiles
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = AllFileCount,
            StepCount = StepsCount,
            Name = string.Format(LibraryStrings.CopySiteFiles, SiteName ?? string.Empty)
        };
    }
}
