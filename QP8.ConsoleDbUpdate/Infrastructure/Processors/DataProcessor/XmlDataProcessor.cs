using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : IDataProcessor
    {
        private readonly BaseSettingsModel _settings;
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(
            XmlSettingsModel settings,
            IXmlDbUpdateLogService xmlDbUpdateLogService,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateActionCorrecterService actionCorrecterService,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor)
        {
            QPContext.CurrentCustomerCode = settings.CustomerCode;

            _settings = settings;
            _xmlDbUpdateReplayService = new XmlDbUpdateNonMvcReplayService(
                QPConfiguration.GetConnectionString(QPContext.CurrentCustomerCode),
                CommonHelpers.GetDbIdentityInsertOptions(settings.DisableFieldIdentity, settings.DisableContentIdentity),
                settings.UserId,
                settings.UseGuidSubstitution,
                xmlDbUpdateLogService,
                appInfoRepository,
                actionCorrecterService,
                httpContextProcessor);
        }

        public void Process()
        {
            var xmlActionsString = XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath, (XmlSettingsModel)_settings);
            _xmlDbUpdateReplayService.Process(xmlActionsString, _settings.FilePathes);
        }
    }
}
