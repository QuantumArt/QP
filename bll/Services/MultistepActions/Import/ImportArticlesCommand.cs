using System;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using QP8.Infrastructure;
using NLog;
using NLog.Fluent;
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
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private static readonly ILogger ImportLogger = LogManager.GetCurrentClassLogger();

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
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            ParentId = SiteId,
            Id = ContentId
        };

        public MultistepActionStepResult Step(int step)
        {
            var settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            Ensure.NotNull(settings);

            var reader = new CsvReader(SiteId, ContentId, settings);
            var result = new MultistepActionStepResult();

            using (new QPConnectionScope())
            {
                try
                {
                    reader.Process(step, ItemsPerStep, out var processedItemsCount);

                    settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);

                    var logData = new ImportArticlesLogData()
                    {
                        Id = settings.Id,
                        InsertedArticleIds = settings.InsertedArticleIds.ToArray(),
                        UpdatedArticleIds = settings.UpdatedArticleIds.ToArray(),
                        ImportAction = (CsvImportMode)settings.ImportAction
                    };

                    ImportLogger.Trace()
                       .Message("Import articles step: {step}.", step)
                       .Property("result", logData)
                       .Property("customerCode", QPContext.CurrentCustomerCode)
                       .Write();

                    result.ProcessedItemsCount = processedItemsCount;
                    result.TraceResult = reader.GetTraceResult();
                    result.AdditionalInfo = $"{MultistepActionStrings.InsertedArticles}: {settings.InsertedArticleIds.Count}; {MultistepActionStrings.UpdatedArticles}: {settings.UpdatedArticleIds.Count}.";
                }
                catch (Exception ex)
                {
                    throw new ImportException(string.Format(ImportStrings.ImportInterrupted, ex.Message, reader.LastProcessed), ex, settings);
                }
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

