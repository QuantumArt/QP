using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteUpdateArticleIdsCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

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

            var prms = HttpContext.Session.GetValue<CopySiteSettings>(HttpContextSession.CopySiteServiceSettings);
            DestinationSiteId = prms.DestinationSiteId;
        }

        public MultistepActionStepResult Step(int step)
        {
            var prms = HttpContext.Session.GetValue<CopySiteSettings>(HttpContextSession.CopySiteServiceSettings);
            var result = new MultistepActionStepResult();
            var skip = step * ItemsPerStep;
            var xDocument = XDocument.Load(prms.PathForFileWithLinks);
            var items = xDocument.Elements().Elements().Skip(skip).Take(ItemsPerStep).ToArray();
            ContentService.UpdateArticlesLinks(SourceSiteId, DestinationSiteId, string.Concat(items.AsEnumerable()));
            result.ProcessedItemsCount = items.Count();

            var siteArticlesCount = xDocument.Elements().Elements().Count();
            if (skip + 1 >= siteArticlesCount - ItemsPerStep)
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
