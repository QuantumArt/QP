#if !NET_STANDARD
using System.Collections.Generic;
using System.Web;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants.Mvc;
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

        private IEnumerable<Content> ExtensionContents { get; }

        public ExportArticlesCommand(MultistepActionStageCommandState state)
            : this(state.ParentId, state.Id, 0, state.Ids, state.ExtensionContents)
        {
        }

        public ExportArticlesCommand(int siteId, int contentId, int itemCount, int[] ids, IEnumerable<Content> extensionContent)
        {
            SiteId = siteId;
            ContentId = contentId;
            ItemCount = itemCount;
            Ids = ids;
            ExtensionContents = extensionContent;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            ParentId = SiteId,
            Id = ContentId,
            Ids = Ids,
            ExtensionContents = ExtensionContents
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
            Name = ContentStrings.ExportArticles
        };

        public MultistepActionStepResult Step(int step)
        {
            var content = ContentRepository.GetById(ContentId);
            var settings = HttpContext.Current.Session[HttpContextSession.ExportSettingsSessionKey] as ExportSettings;
            Ensure.NotNull(content, string.Format(ContentStrings.ContentNotFound, ContentId));
            Ensure.NotNull(settings);

            var csv = new CsvWriter(SiteId, ContentId, Ids, ExtensionContents, settings);
            var result = new MultistepActionStepResult { ProcessedItemsCount = csv.Write(step, ItemsPerStep) };
            if (csv.CsvReady)
            {
                result.AdditionalInfo = csv.CopyFileToTempSiteLiveDirectory();
            }

            return result;
        }
    }
}
#endif
