using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class CsvDataProcessor : IDataProcessor
    {
        private readonly CsvSettingsModel _settings;

        public CsvDataProcessor(CsvSettingsModel settings)
        {
            _settings = settings;
        }

        public void Process()
        {
            throw new System.NotImplementedException();
        }
    }
}
