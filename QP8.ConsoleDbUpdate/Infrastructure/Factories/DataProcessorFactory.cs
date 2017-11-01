using System;
using Moq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class DataProcessorFactory
    {
        internal static IDataProcessor Create(BaseSettingsModel settings)
        {
            Program.Logger.Debug("Init data processor..");

            switch (settings)
            {
                case XmlSettingsModel xmlSettings:
                    var dbLogService = xmlSettings.DisableDataIntegrity
                        ? new Mock<IXmlDbUpdateLogService>().Object
                        : new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());

                    return new XmlDataProcessor(
                        xmlSettings,
                        dbLogService,
                        new ApplicationInfoRepository(),
                        new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())),
                        new XmlDbUpdateHttpContextProcessor()
                    );
                case CsvSettingsModel csvSettings:
                    return new CsvDataProcessor(
                        csvSettings,
                        new FieldRepository(),
                        new ContentRepository(),
                        new ArticleRepository()
                    );
            }

            throw new NotImplementedException($"Processor for current settings ({settings.GetType().Name}) is not implemented yet..");
        }
    }
}
