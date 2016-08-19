using System;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class DataProcessorFactory
    {
        internal static IDataProcessor Create(BaseSettingsModel settings)
        {
            var xmlSettings = settings as XmlSettingsModel;
            if (xmlSettings != null)
            {
                return new XmlDataProcessor(xmlSettings);
            }

            var csvSettings = settings as CsvSettingsModel;
            if (csvSettings != null)
            {
                return new CsvDataProcessor(csvSettings);
            }

            throw new NotImplementedException($"Processor for current settings ({settings.GetType().Name}) is not implemented yet..");
        }
    }
}
