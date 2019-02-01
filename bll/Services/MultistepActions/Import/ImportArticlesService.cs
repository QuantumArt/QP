#if !NET_STANDARD
using System;
using System.IO;
using System.Web;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
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

        private readonly ILog _importLogger;

        public ImportArticlesService()
        {
            _importLogger = LogProvider.GetLogger(GetType());
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            var importSettings = settingsParams as ImportSettings;
            _importLogger.Trace($"Start import articles with settings id: {importSettings.Id}");

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

            var logData = new
            {
                importSettings.Id,
                importSettings.InsertedArticleIds,
                importSettings.UpdatedArticleIds,
                ImportAction = (CsvImportMode)importSettings.ImportAction
            };

            _importLogger.Info($"Articles import was finished {logData.ToJsonLog()}");
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
#endif
