using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteContentsCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const int ItemsPerStep = 10;

        public int SiteId { get; set; }

        public int? NewSiteId { get; set; }

        public string SiteName { get; set; }

        public int ContentsCount { get; set; }

        public CopySiteContentsCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, 0)
        {
        }

        public CopySiteContentsCommand(int siteId, string siteName, int contentsCount)
        {
            SiteId = siteId;
            SiteName = siteName;
            ContentsCount = contentsCount;

            var prms = HttpContext.Session.GetValue<CopySiteSettings>(HttpContextSession.CopySiteServiceSettings);
            NewSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            var startFrom = step * ItemsPerStep + 1;
            var endOn = startFrom - 1 + ItemsPerStep;

            result.ProcessedItemsCount = ContentService.CopyContents(SiteId, NewSiteId.Value, startFrom, endOn);
            ContentsCount = SiteRepository.GetSiteRealContentCount(SiteId);
            if (endOn >= ContentsCount)
            {
                ContentService.UpdateContents(SiteId, NewSiteId.Value);
            }

            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteContents
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ContentsCount,
            StepCount = MultistepActionHelper.GetStepCount(ContentsCount, ItemsPerStep),
            Name = string.Format(SiteStrings.CopySiteContents, SiteName ?? string.Empty)
        };
    }
}
