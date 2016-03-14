using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteContentsCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 10;

        public int SiteId { get; set; }
        public int? NewSiteId { get; set; }
        public string SiteName { get; set; }
        public int ContentsCount { get; set; }
        public CopySiteContentsCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

        public CopySiteContentsCommand(int siteId, string siteName, int contentsCount) {
            this.SiteId = siteId;
            this.SiteName = siteName;
            this.ContentsCount = contentsCount;
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;
        }
        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();

            int startFrom = step * ITEMS_PER_STEP + 1;
            int endOn = (startFrom - 1) + ITEMS_PER_STEP;

            result.ProcessedItemsCount = ContentService.CopyContents(this.SiteId, this.NewSiteId.Value, startFrom, endOn);

            ContentsCount = SiteRepository.GetSiteRealContentCount(this.SiteId);

            if (endOn >= ContentsCount) {
                ContentService.UpdateContents(this.SiteId, this.NewSiteId.Value);
            }

            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteContents
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = ContentsCount,
                StepCount = MultistepActionHelper.GetStepCount(this.ContentsCount, ITEMS_PER_STEP),
                Name = String.Format(SiteStrings.CopySiteContents, (SiteName ?? ""))
            };
        }
    }
}
