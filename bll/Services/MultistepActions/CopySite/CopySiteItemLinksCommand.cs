using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteItemLinksCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 100;

        public int SourceSiteId { get; set; }

        public int DestinationSiteId { get; set; }

        public int SiteArticlesCount { get; set; }

        public CopySiteItemLinksCommand(MultistepActionStageCommandState state)
            : this(state.Id, 0)
        {
        }

        public CopySiteItemLinksCommand(int sourceSiteId, int siteArticlesCount)
        {
            SourceSiteId = sourceSiteId;
            SiteArticlesCount = siteArticlesCount;

            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            DestinationSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            var result = new MultistepActionStepResult();
            var skip = step * ItemsPerStep;
            var xDocument = XDocument.Load(prms.PathForFileWithLinks);
            var items = xDocument.Elements().Elements().Skip(skip).Take(ItemsPerStep).ToArray();
            ContentService.CopyArticlesLinks(SourceSiteId, DestinationSiteId, string.Concat(items.AsEnumerable()));
            result.ProcessedItemsCount = items.Count();

            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SourceSiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteItemLinks
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = SiteArticlesCount,
            StepCount = MultistepActionHelper.GetStepCount(SiteArticlesCount, ItemsPerStep),
            Name = SiteStrings.CopySiteItemLinks
        };
    }
}
