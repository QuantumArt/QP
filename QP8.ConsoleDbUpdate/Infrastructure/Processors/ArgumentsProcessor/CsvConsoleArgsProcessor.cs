using System.Text;
using CsvHelper.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal class CsvConsoleArgsProcessor : BaseConsoleArgsProcessor
    {
        protected internal override BaseSettingsModel CreateSettingsFromArguments()
        {
            return new CsvSettingsModel(FilePathes, CustomerCode, ConfigPath, new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                TrimHeaders = true,
                Encoding = Encoding.GetEncoding(1251),
                HasExcelSeparator = true
            });
        }
    }
}
