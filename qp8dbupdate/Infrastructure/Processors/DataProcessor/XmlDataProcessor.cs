using Quantumart.QP8.BLL;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : IDataProcessor
    {
        private readonly XmlSettingsModel _settings;
        private readonly XmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(XmlSettingsModel settings)
        {
            _settings = settings;
            QPContext.CurrentCustomerCode = _settings.CustomerCode;
            _xmlDbUpdateReplayService = new XmlDbUpdateReplayService(_settings.DisableFieldIdentity, _settings.DisableContentIdentity);
        }

        public void Process()
        {
            _xmlDbUpdateReplayService.Process(XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath), _settings.FilePathes);
        }
    }
}
