using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class DataProcessorFactory
    {
        internal static IDataProcessor Create(BaseSettingsModel settings, IServiceProvider provider, HttpClient client)
        {

            Program.Logger.Debug("Init data processor..");
            var modelExpressionProvider = provider.GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

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
                        new XmlDbUpdateActionCorrecterService(
                            new ArticleService(new ArticleRepository(), new PathHelper(new DbService(new S3Options()))),
                            new ContentService(new ContentRepository(), new PathHelper(new DbService(new S3Options()))),
                            modelExpressionProvider
                        ),
                        new XmlDbUpdateHttpContextProcessor(),
                        provider
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
