using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : IDataProcessor
    {
        private readonly BaseSettingsModel _settings;
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(XmlSettingsModel settings)
        {
            QPContext.CurrentCustomerCode = settings.CustomerCode;
            var dbLogService = new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());
            var dbActionService = new XmlDbUpdateActionService();

            _settings = settings;
            _xmlDbUpdateReplayService = new XmlDbUpdateNonMvcReplayService(settings.DisableFieldIdentity, settings.DisableContentIdentity, _settings.UserId, dbLogService, dbActionService);
        }

        public void Process()
        {
            var xmlActionsString = XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath, (XmlSettingsModel) _settings);
            _xmlDbUpdateReplayService.Process(xmlActionsString, _settings.FilePathes);
        }
    }
}
