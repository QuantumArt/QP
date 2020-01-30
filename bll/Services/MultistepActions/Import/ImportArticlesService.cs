using System;
using System.IO;
using System.Web;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesService : MultistepActionServiceAbstract
    {
        private ImportArticlesCommand _command;

        private static readonly ILogger ImportLogger = LogManager.GetCurrentClassLogger();

        public ImportArticlesService()
        {
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            var importSettings = settingsParams as ImportSettings;
            ImportLogger.Info()
                .Message("Start import articles with settings id: {guid}", importSettings.Id)
                .Write();

            var content = ContentRepository.GetById(id);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            importSettings.IsWorkflowAssigned = content.WorkflowBinding.IsAssigned;
            HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] = importSettings;
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
            try
            {
                var fileReader = new FileReader(settings);
                fileReader.CopyFileToTempDir();

                var linesTotalCount = fileReader.RowsCount();
                _command = new ImportArticlesCommand(parentId, id, linesTotalCount);

                return base.Setup(parentId, id, boundToExternal);
            }
            catch (Exception ex)
            {
                throw new ImportException(ex.Message, ex, settings);
            }
        }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id) => new MultistepActionSettings
        {
            Stages = new[]
            {
                _command.GetStageSettings()
            }
        };

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal) => new MultistepActionServiceContext { CommandStates = new[] { _command.GetState() } };

        protected override string ContextSessionKey => HttpContextSession.ImportContextSessionKey;

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state) => new ImportArticlesCommand(state);

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId) => new ImportArticlesParams(siteId, contentId);

        public override void TearDown()
        {
            var importSettings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
            RemoveFileFromTemp();

            var logData = new ImportArticlesLogData()
            {
                Id = importSettings.Id,
                InsertedArticleIds = importSettings.InsertedArticleIds.ToArray(),
                UpdatedArticleIds = importSettings.UpdatedArticleIds.ToArray(),
                ImportAction = (CsvImportMode)importSettings.ImportAction
            };

            ImportLogger.Info()
                .Message("Articles import was finished")
                .Property("result", logData)
                .Write();

            HttpContext.Current.Session.Remove(HttpContextSession.ImportSettingsSessionKey);

            base.TearDown();
        }

        private static void RemoveFileFromTemp()
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
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
