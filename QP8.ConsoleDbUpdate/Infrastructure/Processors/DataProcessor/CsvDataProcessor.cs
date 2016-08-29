using Quantumart.QP8.BLL;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class CsvDataProcessor : IDataProcessor
    {
        private readonly CsvSettingsModel _settings;
        private readonly ICsvDbUpdateService _csvDbUpdateService;

        public CsvDataProcessor(CsvSettingsModel settings)
        {
            QPContext.CurrentCustomerCode = settings.CustomerCode;

            _settings = settings;
            _csvDbUpdateService = new CsvDbUpdateService(_settings.UserId);
        }

        public void Process()
        {
            _csvDbUpdateService.Process(CsvReaderProcessor.Process(_settings.FilePathes, _settings.CsvConfiguration));
        }
    }
}
