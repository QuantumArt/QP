using System.Web;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteVirtualContentsCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; set; }

        public int? NewSiteId { get; set; }

        public string SiteName { get; set; }

        public int ContentsCount { get; set; }

        public CopySiteVirtualContentsCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, 0)
        {
        }

        public CopySiteVirtualContentsCommand(int siteId, string siteName, int contentsCount)
        {
            SiteId = siteId;
            SiteName = siteName;
            ContentsCount = contentsCount;
            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            NewSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            var contentsToInsert = VirtualContentService.CopyVirtualContents(SiteId, NewSiteId.Value);
            var errors = VirtualContentService.UpdateVirtualContents(SiteId, NewSiteId.Value, contentsToInsert);
            if (!string.IsNullOrEmpty(errors))
            {
                result.AdditionalInfo = $"{ContentStrings.ErrorsOnCopyingVirtualContents}{errors}";
            }

            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteVirtualContents
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ContentsCount,
            StepCount = 1,
            Name = string.Format(SiteStrings.CopySiteVirtualContents, SiteName ?? string.Empty)
        };
    }
}
