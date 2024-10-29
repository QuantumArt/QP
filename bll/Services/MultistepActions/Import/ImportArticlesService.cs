using System;
using System.IO;
using NLog;
using NLog.Fluent;
using QP8.Infrastructure.Web.Extensions;
using QP8.Infrastructure.Extensions;

using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesService : MultistepActionServiceAbstract
    {
        private static readonly ILogger ImportLogger = LogManager.GetCurrentClassLogger();


        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            if (settingsParams is ImportSettings importSettings)
            {
                ImportLogger.ForInfoEvent()
                    .Message("Start import articles with settings id: {guid}", importSettings.Id)
                    .Log();

                var content = ContentRepository.GetById(id);
                if (content == null)
                {
                    throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
                }

                importSettings.IsWorkflowAssigned = content.WorkflowBinding.IsAssigned;
                HttpContext.Session.SetValue(HttpContextSession.ImportSettingsSessionKey, importSettings);
            }


        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, S3Options options)
        {
            var settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            try
            {
                var fileReader = new FileReader(settings);
                var linesTotalCount = fileReader.RowsCount();
                Commands.Add(new ImportArticlesCommand(parentId, id, linesTotalCount, options));

                return base.Setup(parentId, id, boundToExternal, options);
            }
            catch (Exception ex)
            {
                throw new ImportException(ex.Message, ex, settings);
            }
        }

        protected override string ContextSessionKey => HttpContextSession.ImportContextSessionKey;

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state) => new ImportArticlesCommand(state);

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId) => new ImportArticlesParams(siteId, contentId);

        public override void TearDown()
        {
            var importSettings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            RemoveFileFromTemp();

            var logData = new ImportArticlesLogData()
            {
                Id = importSettings.Id,
                InsertedArticleIds = importSettings.InsertedArticleIds.ToArray(),
                UpdatedArticleIds = importSettings.UpdatedArticleIds.ToArray(),
                ImportAction = (CsvImportMode)importSettings.ImportAction
            };

            ImportLogger.ForInfoEvent()
                .Message("Articles import was finished")
                .Property("result", logData)
                .Property("customerCode", QPContext.CurrentCustomerCode)
                .Log();

            HttpContext.Session.Remove(HttpContextSession.ImportSettingsSessionKey);

            base.TearDown();
        }

        private static void RemoveFileFromTemp()
        {
            var settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            RemoveTempFiles(settings);
        }

        private static void RemoveTempFiles(ImportSettings setts)
        {
            if (File.Exists(setts.TempFileUploadPath))
            {
                File.Delete(setts.TempFileUploadPath);
            }

            if (File.Exists(setts.TempFileForRelFields))
            {
                File.Delete(setts.TempFileForRelFields);
            }
        }
    }
}
