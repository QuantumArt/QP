using CsvHelper;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Db;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class CsvDataProcessor : IDataProcessor
    {
        private readonly CsvSettingsModel _settings;
        private readonly ICsvDbUpdateService _csvDbUpdateService;

        public CsvDataProcessor(CsvSettingsModel settings, IFieldRepository fieldRepository, IContentRepository contentRepository, IArticleRepository articleRepository)
        {
            QPContext.CurrentCustomerCode = settings.CustomerCode;

            _settings = settings;
            _csvDbUpdateService = new CsvDbUpdateService(new ArticleService(_settings.UserId), fieldRepository, contentRepository, articleRepository);
        }

        public void Process()
        {
            var csvData = CsvReaderProcessor.Process(_settings.FilePathes, _settings.CsvConfiguration);
            _csvDbUpdateService.Process(csvData);
        }

        public void Process(string csvData)
        {
            const string validationMessage = "Cannot use pipes for processing csv data";
            Logger.Log.Error(validationMessage);
            throw new CsvBadDataException(validationMessage);
        }
    }
}
