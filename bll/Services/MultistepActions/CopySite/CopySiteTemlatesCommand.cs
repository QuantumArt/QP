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
    public class CopySiteTemlatesCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 1;

        public int SiteId { get; set; }
        public int? NewSiteId { get; set; }
        public string SiteName { get; set; }
        public int TemplatesElementsCount { get; set; }

        public CopySiteTemlatesCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

        public CopySiteTemlatesCommand(int siteId, string siteName, int elementsCount)
        {
            this.SiteId = siteId;
            this.SiteName = siteName;
            this.TemplatesElementsCount = elementsCount;
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;
        }
        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();
            int templateNumber = step * ITEMS_PER_STEP + 1;
            result.ProcessedItemsCount = PageTemplateService.CopySiteTemplates(this.SiteId, this.NewSiteId.Value, templateNumber);
            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteTemplates
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = TemplatesElementsCount,
                StepCount = MultistepActionHelper.GetStepCount(this.TemplatesElementsCount, ITEMS_PER_STEP),
                Name = String.Format(TemplateStrings.CopySiteTemplates, (SiteName ?? ""))
            };
        }
    }
}
