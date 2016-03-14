using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteArticlesCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 20;

        public int CopyFromSiteId { get; set; }
        public int? NewSiteId { get; set; }
        public int SiteArticlesCount { get; set; }
        public string SiteName { get; set; }
        private string fileNameForLinks { get; set; }
        private string contentsToCopy { get; set; }

        public CopySiteArticlesCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

        public CopySiteArticlesCommand(int copyFromSiteId, string siteName, int siteArticlesCount)
        {
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;

            this.CopyFromSiteId = copyFromSiteId;
            this.fileNameForLinks = prms.PathForFileWithLinks;
            this.SiteArticlesCount = siteArticlesCount;
            this.SiteName = siteName;
            this.contentsToCopy = prms.ContentsToCopy;
        }

        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();
            int startFrom = step * ITEMS_PER_STEP + 1;
            int endOn = (startFrom-1) + ITEMS_PER_STEP;
            IEnumerable<DataRow> articlesLinks = ContentService.CopyContentsData(this.CopyFromSiteId, this.NewSiteId.Value, this.contentsToCopy, startFrom, endOn);
            WriteToTempFile(articlesLinks);
            result.ProcessedItemsCount = articlesLinks.Count();

            return result;
        }

        private void WriteToTempFile(IEnumerable<DataRow> articlesLinks)
        {
            try
            {
                string pathForFile = this.fileNameForLinks;
                XDocument xDocument;
                if (File.Exists(pathForFile))
                {
                    xDocument = XDocument.Load(pathForFile);
                }
                else
                {
                    xDocument = new XDocument();
                    XElement items = new XElement("items");
                    items.Add(new XAttribute("oldSiteId", this.CopyFromSiteId));
                    items.Add(new XAttribute("newSiteId", this.NewSiteId.Value));
                    xDocument.Add(items);
                }

                foreach (DataRow row in articlesLinks)
                {
                    XElement itemXML = new XElement("item");
                    itemXML.Add(new XAttribute("sourceId", row["source_item_id"].ToString()));
                    itemXML.Add(new XAttribute("destinationId", row["destination_item_id"].ToString()));
                    xDocument.Root.Add(itemXML);
                }

                xDocument.Save(pathForFile);
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = CopyFromSiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteArticles
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = this.SiteArticlesCount,
                StepCount = MultistepActionHelper.GetStepCount(this.SiteArticlesCount, ITEMS_PER_STEP),
                Name = String.Format(SiteStrings.CopySiteArticles, (SiteName ?? ""))
            };
        }
    }
}
