using System;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal class XmlConsoleArgsProcessor : BaseConsoleArgsProcessor
    {
        private bool _generateNewFieldIds;
        private bool _generateNewContentIds;
        private bool _useGuidSubstitution;
        private bool _disableDataIntegrity;

        protected internal override OptionSet BuildOptionSet()
        {
            return new OptionSet
            {
                { "g|generateIds=|d|disable=", "generate new ids for: [field|content]", ParseGenerateIdsOption },
                { "useGuid", "enable guid substitution mode", ug => _useGuidSubstitution = ug != null },
                { "disableDataIntegrity", "disable data integrity", ug => _disableDataIntegrity = ug != null }
            };
        }

        protected internal override BaseSettingsModel CreateSettingsFromArguments()
        {
            return new XmlSettingsModel(
                FilePathes,
                CustomerCode,
                DbType,
                ConfigPath,
                _generateNewFieldIds,
                _generateNewContentIds,
                _useGuidSubstitution,
                _disableDataIntegrity,
                QpConfigUrl,
                QpConfigToken,
                QpConfigPath,
                ConnectionString);
        }

        protected internal override void PrintEnteredData()
        {
            Console.WriteLine("Generate new field ids: " + _generateNewFieldIds);
            Console.WriteLine("Generate new content ids: " + _generateNewContentIds);
            Console.WriteLine("Use Guid substitution: " + _useGuidSubstitution);
            Console.WriteLine("Disable data integrity: " + _disableDataIntegrity);
            base.PrintEnteredData();
        }

        private void ParseGenerateIdsOption(string disableOption)
        {
            if (!string.IsNullOrWhiteSpace(disableOption))
            {
                foreach (string option in disableOption.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (option.ToLower())
                    {
                        case "field":
                            _generateNewFieldIds = true;
                            break;
                        case "content":
                            _generateNewContentIds = true;
                            break;
                        default:
                            throw new NotImplementedException($"Unknown generate ids option: \"{option.ToLower()}\"");
                    }
                }
            }
        }
    }
}
