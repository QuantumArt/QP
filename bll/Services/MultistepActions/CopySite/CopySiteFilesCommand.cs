using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
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

        public CopySiteFilesCommand(MultistepActionStageCommandState state) : this(state.Id, null, false) { }

        public CopySiteFilesCommand(int siteId, string siteName, bool doNotCopyFiles)
        {
            this.SiteId = siteId;
            this.SiteName = siteName;
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;

            Site sourceSite = SiteRepository.GetById(SiteId);
            if (doNotCopyFiles) { 
                this.StepsCount = 0;
                return;
            }
            AllFilesSize = CopySiteFilesHelper.GetAllFileSize(sourceSite);
            AllFileCount = CopySiteFilesHelper.FilesCount(sourceSite);
            double steps = (double)AllFilesSize / CopySiteFilesHelper.FilesSizeLimitPerTransaction;
            this.StepsCount = CleverRoundSteps(steps);
        }
        private int CleverRoundSteps(double steps) {
            double roundedSteps = Math.Round(steps);
            if (roundedSteps >= steps)
            {
                return (int)Math.Round(steps);
            }
            else
            {
                return (int)roundedSteps + 1;
            }
        }
        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();
            Site sourceSite = SiteRepository.GetById(SiteId);
            Site destinationSite = SiteRepository.GetById(this.NewSiteId.Value);

            CopySiteFilesHelper helper = new CopySiteFilesHelper(sourceSite, destinationSite);
            helper.CopyDirectories();
            result.ProcessedItemsCount = helper.CopyFiles(step);

            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteFiles
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = AllFileCount,
                StepCount = StepsCount,
                Name = String.Format(LibraryStrings.CopySiteFiles, (SiteName ?? ""))
            };
        }
    }
}
