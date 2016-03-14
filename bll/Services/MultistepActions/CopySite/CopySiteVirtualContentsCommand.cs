using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteVirtualContentsCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; set; }
        public int? NewSiteId { get; set; }
        public string SiteName { get; set; }
        public int ContentsCount { get; set; }
        public CopySiteVirtualContentsCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

        public CopySiteVirtualContentsCommand(int siteId, string siteName, int contentsCount) {
            this.SiteId = siteId;
            this.SiteName = siteName;
            this.ContentsCount = contentsCount;
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;
        }
        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();
            IEnumerable<DataRow> contentsToInsert = VirtualContentService.CopyVirtualContents(this.SiteId, this.NewSiteId.Value);
            string errors = VirtualContentService.UpdateVirtualContents(this.SiteId, this.NewSiteId.Value, contentsToInsert);
            if(!String.IsNullOrEmpty(errors))
                result.AdditionalInfo = String.Format("{0}{1}",ContentStrings.ErrorsOnCopyingVirtualContents, errors);
            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteVirtualContents
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = ContentsCount,
                StepCount = 1,
                Name = String.Format(SiteStrings.CopySiteVirtualContents, (SiteName ?? ""))
            };
        }
    }
}
