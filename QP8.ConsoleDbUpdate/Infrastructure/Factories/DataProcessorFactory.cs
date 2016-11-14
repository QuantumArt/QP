using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class DataProcessorFactory
    {
        internal static IDataProcessor Create(BaseSettingsModel settings)
        {
            var xmlSettings = settings as XmlSettingsModel;
            if (xmlSettings != null)
            {
                return new XmlDataProcessor(
                    xmlSettings,
                    new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository()),
                    new ApplicationInfoRepository(),
                    new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())),
                    new XmlDbUpdateHttpContextProcessor()
                );
            }

            var csvSettings = settings as CsvSettingsModel;
            if (csvSettings != null)
            {
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
