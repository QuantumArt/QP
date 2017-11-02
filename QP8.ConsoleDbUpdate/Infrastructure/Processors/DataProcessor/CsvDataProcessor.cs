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
    internal class CsvDataProcessor : BaseDataProcessor
    {
        private readonly CsvSettingsModel _settings;
        private readonly ICsvDbUpdateService _csvDbUpdateService;

        public CsvDataProcessor(
            CsvSettingsModel settings,
            IFieldRepository fieldRepository,
            IContentRepository contentRepository,
            IArticleRepository articleRepository)
            : base(settings)
        {
            _settings = settings;
            _csvDbUpdateService = new CsvDbUpdateService(new ArticleService(QPContext.CurrentDbConnectionString, _settings.UserId), fieldRepository, contentRepository, articleRepository);
        }

        public override void Process()
        {
            var csvData = CsvReaderProcessor.Process(_settings.FilePathes, _settings.CsvConfiguration);
            _csvDbUpdateService.Process(csvData);
        }

        public override void Process(string csvRawData)
        {
            var csvData = CsvReaderProcessor.Process(csvRawData, _settings.CsvConfiguration);
            _csvDbUpdateService.Process(csvData);
        }
    }
}
