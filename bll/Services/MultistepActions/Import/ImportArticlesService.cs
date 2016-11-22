using System;
using System.IO;
using System.Web;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Logging.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesService : MultistepActionServiceAbstract
    {
        private ImportArticlesCommand command;
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

            HttpContext.Current.Session[ImportSettingsSessionKey] = importSettings;
        }

		public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
        {
            var setts = HttpContext.Current.Session[ImportSettingsSessionKey] as ImportSettings;
			try
			{
				var fileReader = new FileReader(setts);
				fileReader.CopyFileToTempDir();
				var linesTotalCount = fileReader.RowsCount();
				command = new ImportArticlesCommand(parentId, id, linesTotalCount, _logReader, _logger);
				return base.Setup(parentId, id, boundToExternal);
			}
			catch (Exception ex)
			{
				throw new ImportException(ex.Message, ex, setts);
			}
        }
        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            return new MultistepActionSettings
            {
                Stages = new[]
				{
                    command.GetStageSettings()
				}
            };
        }

		protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            var commandState = command.GetState();
            return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
        }

        protected override string ContextSessionKey
        {
            get { return "ImportArticlesService.ProcessingContext"; }
        }

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
			return new ImportArticlesCommand(state, _logReader, _logger);
        }

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId)
        {
            IMultistepActionSettings prms = new ImportArticlesParams(siteId, contentId);
            return prms;
        }

        public override void TearDown()
        {
			var setts = HttpContext.Current.Session[ImportSettingsSessionKey] as ImportSettings;
			RemoveFileFromTemp();
			_logger.LogEndImport(setts);
			HttpContext.Current.Session.Remove(ImportSettingsSessionKey);
			base.TearDown();
        }

        private void RemoveFileFromTemp()
        {
            var setts = HttpContext.Current.Session[ImportSettingsSessionKey] as ImportSettings;
            RemoveTempFiles(setts);
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

        public static string ImportSettingsSessionKey
        {
            get { return "ImportArticlesService.Settings"; }
        }
    }
}
