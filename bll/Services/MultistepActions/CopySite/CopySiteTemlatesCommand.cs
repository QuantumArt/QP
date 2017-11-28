using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteTemlatesCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 1;

        public int SiteId { get; set; }

        public int? NewSiteId { get; set; }

        public string SiteName { get; set; }

        public int TemplatesElementsCount { get; set; }

        public CopySiteTemlatesCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, 0)
        {
        }

        public CopySiteTemlatesCommand(int siteId, string siteName, int elementsCount)
        {
            SiteId = siteId;
            SiteName = siteName;
            TemplatesElementsCount = elementsCount;

            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            NewSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            var templateNumber = step * ItemsPerStep + 1;
            result.ProcessedItemsCount = PageTemplateService.CopySiteTemplates(SiteId, NewSiteId.Value, templateNumber);
            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteTemplates
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = TemplatesElementsCount,
            StepCount = MultistepActionHelper.GetStepCount(TemplatesElementsCount, ItemsPerStep),
            Name = string.Format(TemplateStrings.CopySiteTemplates, SiteName ?? string.Empty)
        };
    }
}
