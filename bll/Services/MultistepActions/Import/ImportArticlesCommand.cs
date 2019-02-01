#if !NET_STANDARD
using System;
using System.Transactions;
using System.Web;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesCommand : IMultistepActionStageCommand
    {
        private readonly ILog _importLogger;

        private const int ItemsPerStep = 20;

        public int SiteId { get; set; }

        public int ContentId { get; set; }

        public int ItemCount { get; set; }

        public ImportArticlesCommand(MultistepActionStageCommandState state)
            : this(state.ParentId, state.Id, 0)
        {
        }

        public ImportArticlesCommand(int siteId, int contentId, int itemCount)
        {
            SiteId = siteId;
            ContentId = contentId;
            ItemCount = itemCount;
            _importLogger = LogProvider.GetLogger(GetType());
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            ParentId = SiteId,
            Id = ContentId
        };

        public MultistepActionStepResult Step(int step)
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
            Ensure.NotNull(settings);

            var reader = new CsvReader(SiteId, ContentId, settings);
            var result = new MultistepActionStepResult();
            using (var ts = new TransactionScope())
            {
                using (new QPConnectionScope())
                {
                    try
                    {
                        reader.Process(step, ItemsPerStep, out var processedItemsCount);
                        if (step * ItemsPerStep >= reader.ArticleCount - ItemsPerStep)
                        {
                            reader.PostUpdateM2MRelationAndO2MRelationFields();
                        }

                        var logData = new
                        {
                            settings.Id,
                            ImportAction = (CsvImportMode)settings.ImportAction,
                            Inserted = settings.InsertedArticleIds.Count,
                            Updated = settings.UpdatedArticleIds.Count
                        };

                        _importLogger.Trace($"Import articles step: {step}. Settings: {logData.ToJsonLog()}");
                        result.ProcessedItemsCount = processedItemsCount;
                        result.AdditionalInfo = $"{MultistepActionStrings.InsertedArticles}: {settings.InsertedArticleIds.Count}; {MultistepActionStrings.UpdatedArticles}: {settings.UpdatedArticleIds.Count}.";
                    }
                    catch (Exception ex)
                    {
                        throw new ImportException(string.Format(ImportStrings.ImportInterrupted, ex.Message, reader.LastProcessed), ex, settings);
                    }
                }

                ts.Complete();
            }

            return result;
        }

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
            Name = ContentStrings.ImportArticles
        };
    }
}
#endif
