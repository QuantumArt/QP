using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteUpdateArticleIdsCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 100;

        public int SourceSiteId { get; set; }
        public int DestinationSiteId { get; set; }
        public int SiteArticlesCount { get; set; }

        public CopySiteUpdateArticleIdsCommand(MultistepActionStageCommandState state) : this(state.Id, 0) { }

        public CopySiteUpdateArticleIdsCommand(int sourceSiteId, int siteArticlesCount)
        {
            this.SourceSiteId = sourceSiteId;
            this.SiteArticlesCount = siteArticlesCount;

            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.DestinationSiteId = prms.DestinationSiteId;
        }
        public MultistepActionStepResult Step(int step)
        {
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];

            MultistepActionStepResult result = new MultistepActionStepResult();
            int startFrom = step * ITEMS_PER_STEP + 1;

            XDocument xDocument = XDocument.Load(prms.PathForFileWithLinks);

            
            string elements = string.Concat(xDocument.Elements().Elements().Skip(startFrom).Take(ITEMS_PER_STEP));

            ContentService.UpdateArticlesLinks(this.SourceSiteId, this.DestinationSiteId, elements);

            result.ProcessedItemsCount = xDocument.Elements().Elements().Skip(startFrom).Take(ITEMS_PER_STEP).Count(); 
                
            int siteArticlesCount = xDocument.Elements().Elements().Count();

            if (startFrom >= siteArticlesCount - ITEMS_PER_STEP)
            {
                File.Delete(prms.PathForFileWithLinks);
            }

            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SourceSiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteUpdateArticleIds
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = this.SiteArticlesCount,
                StepCount = MultistepActionHelper.GetStepCount(this.SiteArticlesCount, ITEMS_PER_STEP),
                Name = SiteStrings.CopySiteUpdateArticleIds
            };
        }
    }
}
