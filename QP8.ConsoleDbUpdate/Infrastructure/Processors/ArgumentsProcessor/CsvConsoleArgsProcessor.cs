using System;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal class CsvConsoleArgsProcessor : BaseConsoleArgsProcessor
    {
        private string _encoding = "windows-1251";
        private string _cultureInfo = "ru-RU";

        protected internal override OptionSet BuildOptionSet()
        {
            return new OptionSet
            {
                { "encoding=", "csv file encoding", enc => _encoding = enc },
                { "culture=", "csv file culture info", ci => _cultureInfo = ci }
            };
        }

        protected internal override void PrintEnteredData()
        {
            Console.WriteLine("File Encoding: " + _encoding);
            Console.WriteLine("File Culture: " + _cultureInfo);
            base.PrintEnteredData();
        }

        protected internal override BaseSettingsModel CreateSettingsFromArguments()
        {
            return new CsvSettingsModel(FilePathes, CustomerCode, ConfigPath, new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                TrimHeaders = true,
                Encoding = Encoding.GetEncoding(_encoding),
                CultureInfo = CultureInfo.GetCultureInfo(_cultureInfo),
                HasExcelSeparator = true
            });
        }
    }
}
