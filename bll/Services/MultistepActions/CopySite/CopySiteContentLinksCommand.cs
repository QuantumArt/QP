using Quantumart.QP8.Resources;
using System.Web;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteContentLinksCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; set; }

        public int? NewSiteId { get; set; }

        public string SiteName { get; set; }

		public int Count { get; set; }

        public CopySiteContentLinksCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

		public CopySiteContentLinksCommand(int siteId, string siteName, int count)
		{
            SiteId = siteId;
            SiteName = siteName;
			Count = count;
            var prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            NewSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            ContentService.CopyContentLinks(SiteId, NewSiteId.Value);
			result.ProcessedItemsCount = Count;
            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
				Type = CopySiteStageCommandTypes.CopySiteContentLinks
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = Count,
				StepCount = 1,
                Name = string.Format(SiteStrings.CopySiteContentLinks, SiteName ?? string.Empty)
            };
        }
    }
}
