using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    internal class ExportArticlesCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 20;

        public int ContentId { get; private set; }
        public int SiteId { get; private set; }
        public int ItemCount { get; private set; }
        public int[] Ids { get; private set; }

        public ExportArticlesCommand(MultistepActionStageCommandState state) : this(state.ParentId, state.Id, 0, state.Ids) { }

        public ExportArticlesCommand(int siteId, int contentId, int itemCount, int[] ids)
        {
            SiteId = siteId;
            ContentId = contentId;
            Ids = ids;
            ItemCount = itemCount;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                ParentId = SiteId,
                Id = ContentId,
                Ids = Ids
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = ItemCount,
                StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
                Name = ContentStrings.ExportArticles
            };
        }

        public MultistepActionStepResult Step(int step)
        {
            Content content = ContentRepository.GetById(ContentId);
            if (content == null)
                throw new Exception(String.Format(ContentStrings.ContentNotFound, ContentId));

            if (HttpContext.Current.Session["ExportArticlesService.Settings"] == null) {
                throw new ArgumentException("There is no specified settings");
            }
            ExportSettings setts = HttpContext.Current.Session["ExportArticlesService.Settings"] as ExportSettings;

            CsvWriter csv = new CsvWriter(SiteId, ContentId, Ids, setts);
            MultistepActionStepResult result = new MultistepActionStepResult();
            result.ProcessedItemsCount = csv.Write(step, ITEMS_PER_STEP);

            if (csv.CsvReady)
            {
                result.AdditionalInfo = csv.CopyFileToTempSiteLiveDirectory();
            }
            return result;
        }
    }
}
