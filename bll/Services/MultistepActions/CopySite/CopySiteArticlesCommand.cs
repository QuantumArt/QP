#if !NET_STANDARD
using System;
using System.Collections.Generic;
using System.Data;
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
    public class CopySiteArticlesCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 20;

        public int CopyFromSiteId { get; set; }

        public int? NewSiteId { get; set; }

        public int SiteArticlesCount { get; set; }

        public string SiteName { get; set; }

        private string FileNameForLinks { get; }

        private string ContentsToCopy { get; }

        public CopySiteArticlesCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, 0)
        {
        }

        public CopySiteArticlesCommand(int copyFromSiteId, string siteName, int siteArticlesCount)
        {
            var prms = (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];
            NewSiteId = prms.DestinationSiteId;
            CopyFromSiteId = copyFromSiteId;
            FileNameForLinks = prms.PathForFileWithLinks;
            SiteArticlesCount = siteArticlesCount;
            SiteName = siteName;
            ContentsToCopy = prms.ContentsToCopy;
        }

        public MultistepActionStepResult Step(int step)
        {
            var result = new MultistepActionStepResult();
            var startFrom = step * ItemsPerStep + 1;
            var endOn = startFrom - 1 + ItemsPerStep;
            var articlesLinks = ContentService.CopyContentsData(CopyFromSiteId, NewSiteId.Value, ContentsToCopy, startFrom, endOn);
            WriteToTempFile(articlesLinks);
            result.ProcessedItemsCount = articlesLinks.Count();

            return result;
        }

        private void WriteToTempFile(IEnumerable<DataRow> articlesLinks)
        {
            try
            {
                var pathForFile = FileNameForLinks;
                XDocument xDocument;
                if (File.Exists(pathForFile))
                {
                    xDocument = XDocument.Load(pathForFile);
                }
                else
                {
                    xDocument = new XDocument();
                    var items = new XElement("items");
                    items.Add(new XAttribute("oldSiteId", CopyFromSiteId));
                    items.Add(new XAttribute("newSiteId", NewSiteId.Value));
                    xDocument.Add(items);
                }

                foreach (var row in articlesLinks)
                {
                    var itemXml = new XElement("item");
                    itemXml.Add(new XAttribute("sourceId", row["source_item_id"].ToString()));
                    itemXml.Add(new XAttribute("destinationId", row["destination_item_id"].ToString()));
                    xDocument.Root.Add(itemXml);
                }

                xDocument.Save(pathForFile);
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Id = CopyFromSiteId,
            ParentId = 0,
            Type = CopySiteStageCommandTypes.CopySiteArticles
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = SiteArticlesCount,
            StepCount = MultistepActionHelper.GetStepCount(SiteArticlesCount, ItemsPerStep),
            Name = string.Format(SiteStrings.CopySiteArticles, SiteName ?? string.Empty)
        };
    }
}
#endif
