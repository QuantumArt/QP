using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Adapters;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : IDataProcessor
    {
        private readonly BaseSettingsModel _settings;
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(XmlSettingsModel settings)
        {
            _settings = settings;
            QPContext.CurrentCustomerCode = _settings.CustomerCode;

            var actionsCorrecterService = new XmlDbUpdateActionCorrecterService();
            var dbLogService = new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());
            _xmlDbUpdateReplayService = new XmlDbUpdateNonMvcAppReplayServiceWrapper(new XmlDbUpdateReplayService(settings.DisableFieldIdentity, settings.DisableContentIdentity, _settings.UserId, dbLogService, actionsCorrecterService));
        }

        public void Process()
        {
            _xmlDbUpdateReplayService.Process(XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath), _settings.FilePathes);
        }
    }
}
