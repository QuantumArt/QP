using System;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : BaseDataProcessor
    {
        private readonly BaseSettingsModel _settings;
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(
            XmlSettingsModel settings,
            IXmlDbUpdateLogService xmlDbUpdateLogService,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateActionCorrecterService actionCorrecterService,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor,
            IServiceProvider provider
            )
            : base(settings)
        {
            _settings = settings;
            _xmlDbUpdateReplayService = new XmlDbUpdateNonMvcReplayService(
                QPContext.CurrentDbConnectionString,
                QPContext.DatabaseType,
                CommonHelpers.GetDbIdentityInsertOptions(settings.GenerateNewFieldIds, settings.GenerateNewContentIds),
                settings.UserId,
                settings.UseGuidSubstitution,
                xmlDbUpdateLogService,
                appInfoRepository,
                actionCorrecterService,
                httpContextProcessor,
                provider
                );
        }

        public override void Process()
        {
            string xmlActionsString = XmlReaderProcessor.Process(_settings.FilePathes);
            _xmlDbUpdateReplayService.Process(xmlActionsString, _settings.FilePathes);
        }

        public override void Process(string xmlActionsString)
        {
            _xmlDbUpdateReplayService.Process(xmlActionsString, _settings.FilePathes);
        }
    }
}
