using System;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    internal class ExportArticlesCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 20;

        public int ContentId { get; }

        public int SiteId { get; }

        public int ItemCount { get; }

        public int[] Ids { get; }

        public ExportArticlesCommand(MultistepActionStageCommandState state)
            : this(state.ParentId, state.Id, 0, state.Ids) { }

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
                StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
                Name = ContentStrings.ExportArticles
            };
        }

        public MultistepActionStepResult Step(int step)
        {
            var content = ContentRepository.GetById(ContentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, ContentId));
            }

            if (HttpContext.Current.Session[CsvExport.SettingsSessionKey] == null)
            {
                throw new ArgumentException("There is no specified settings");
            }

            var setts = HttpContext.Current.Session[CsvExport.SettingsSessionKey] as ExportSettings;
            var csv = new CsvWriter(SiteId, ContentId, Ids, setts);
            var result = new MultistepActionStepResult { ProcessedItemsCount = csv.Write(step, ItemsPerStep) };
            if (csv.CsvReady)
            {
                result.AdditionalInfo = csv.CopyFileToTempSiteLiveDirectory();
            }

            return result;
        }
    }
}
