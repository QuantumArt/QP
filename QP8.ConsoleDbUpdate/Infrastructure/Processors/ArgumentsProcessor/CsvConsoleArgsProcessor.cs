using System;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using Mono.Options;
using Quantumart.QP8.BLL;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal class CsvConsoleArgsProcessor : BaseConsoleArgsProcessor
    {
        private string _encoding = "windows-1251";
        private string _cultureInfo = "ru-RU";
        private bool _updateExisting = false;

        protected internal override OptionSet BuildOptionSet()
        {
            return new OptionSet
            {
                { "e|encoding=", "csv file encoding", enc => _encoding = enc },
                { "l|culture=", "csv file culture info", ci => _cultureInfo = ci },
                { "u|updateExisting", "update existing articles", ue => _updateExisting = ue != null }
            };
        }

        protected internal override void PrintEnteredData()
        {
            Console.WriteLine("File Encoding: " + _encoding);
            Console.WriteLine("File Culture: " + _cultureInfo);
            Console.WriteLine("Use existing articles: " + _updateExisting);
            base.PrintEnteredData();
        }

        protected internal override BaseSettingsModel CreateSettingsFromArguments() => new CsvSettingsModel(FilePathes, CustomerCode, DbType, ConfigPath, new CsvConfiguration
        {
            HasHeaderRecord = true,
            TrimFields = true,
            TrimHeaders = true,
            Encoding = Encoding.GetEncoding(_encoding),
            CultureInfo = CultureInfo.GetCultureInfo(_cultureInfo),
            HasExcelSeparator = true
        }, _updateExisting);
    }
}
