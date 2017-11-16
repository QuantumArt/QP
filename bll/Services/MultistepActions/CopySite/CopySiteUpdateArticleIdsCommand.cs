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
    public class CopySiteUpdateArticleIdsCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 100;

        public int SourceSiteId { get; set; }

        public int DestinationSiteId { get; set; }

        public int SiteArticlesCount { get; set; }

        public CopySiteUpdateArticleIdsCommand(MultistepActionStageCommandState state)
            : this(state.Id, 0)
        {
        }

        public CopySiteUpdateArticleIdsCommand(int sourceSiteId, int siteArticlesCount)
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
            var startFrom = step * ItemsPerStep + 1;
            var xDocument = XDocument.Load(prms.PathForFileWithLinks);
            var elements = string.Concat(xDocument.Elements().Elements().Skip(startFrom).Take(ItemsPerStep));

            ContentService.UpdateArticlesLinks(SourceSiteId, DestinationSiteId, elements);
            result.ProcessedItemsCount = xDocument.Elements().Elements().Skip(startFrom).Take(ItemsPerStep).Count();

            var siteArticlesCount = xDocument.Elements().Elements().Count();
            if (startFrom >= siteArticlesCount - ItemsPerStep)
            {
                ContentService.FillLinkTables(SourceSiteId, DestinationSiteId);
                File.Delete(prms.PathForFileWithLinks);
            }

            return result;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = SourceSiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteUpdateArticleIds
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = SiteArticlesCount,
            StepCount = MultistepActionHelper.GetStepCount(SiteArticlesCount, ItemsPerStep),
            Name = SiteStrings.CopySiteUpdateArticleIds
        };
    }
}
