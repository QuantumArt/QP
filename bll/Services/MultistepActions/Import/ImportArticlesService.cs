using System;
using System.IO;
using System.Web;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Logging.Services;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesService : MultistepActionServiceAbstract
    {
        private ImportArticlesCommand _command;
        private readonly ILogReader _logReader;
        private readonly IImportArticlesLogger _logger;

        public ImportArticlesService(ILogReader logReader, IImportArticlesLogger logger)
        {
            _logReader = logReader;
            _logger = logger;
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            var importSettings = settingsParams as ImportSettings;
            _logger.LogStartImport(importSettings);

            var content = ContentRepository.GetById(id);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            HttpContext.Current.Session[CsvExport.ImportSettingsSessionKey] = importSettings;
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = HttpContext.Current.Session[CsvExport.ImportSettingsSessionKey] as ImportSettings;
            try
            {
                var fileReader = new FileReader(settings);
                fileReader.CopyFileToTempDir();

                var linesTotalCount = fileReader.RowsCount();
                _command = new ImportArticlesCommand(parentId, id, linesTotalCount, _logReader, _logger);

                return base.Setup(parentId, id, boundToExternal);
            }
            catch (Exception ex)
            {
                throw new ImportException(ex.Message, ex, settings);
            }
        }
        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            return new MultistepActionSettings
            {
                Stages = new[]
                {
                    _command.GetStageSettings()
                }
            };
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            var commandState = _command.GetState();
            return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
        }

        protected override string ContextSessionKey => CsvExport.ImportContextSessionKey;

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            return new ImportArticlesCommand(state, _logReader, _logger);
        }

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId)
        {
            return new ImportArticlesParams(siteId, contentId);
        }

        public override void TearDown()
        {
            var settings = HttpContext.Current.Session[CsvExport.ImportSettingsSessionKey] as ImportSettings;
            RemoveFileFromTemp();

            _logger.LogEndImport(settings);
            HttpContext.Current.Session.Remove(CsvExport.ImportSettingsSessionKey);

            base.TearDown();
        }

        private static void RemoveFileFromTemp()
        {
            var settings = HttpContext.Current.Session[CsvExport.ImportSettingsSessionKey] as ImportSettings;
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
