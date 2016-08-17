using Quantumart.QP8.BLL;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class CsvDataProcessor : IDataProcessor
    {
        private readonly CsvSettingsModel _settings;
        private readonly CsvDbUpdateService _csvDbUpdateService;

        public CsvDataProcessor(CsvSettingsModel settings)
        {
            _settings = settings;
            QPContext.CurrentCustomerCode = _settings.CustomerCode;
            _csvDbUpdateService = new CsvDbUpdateService(_settings.UserId);
        }

        public void Process()
        {
            _csvDbUpdateService.Process(CsvReaderProcessor.Process(_settings.FilePathes, _settings.CsvConfiguration));
        }
    }
}
